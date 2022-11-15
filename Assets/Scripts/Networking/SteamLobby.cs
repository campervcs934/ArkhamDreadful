using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace Networking
{
    public class SteamLobby : MonoBehaviour
    {
        //Callbacks
        private Callback<LobbyCreated_t> _lobbyCreated;
        private Callback<GameLobbyJoinRequested_t> _joinRequest;
        private Callback<LobbyEnter_t> _lobbyEntered;
        private Callback<LobbyMatchList_t> _lobbyList;

        //Variables
        private const string HostAddressKey = "HostAddress";
        private const string NameKey = "name";
        private CustomNetworkManager _manager;

        //GameObject
        public GameObject hostButton;
        public Text lobbyNameText;
        public InputField lobbyNameInput;
        public Text lobbyId;
        public GameObject joinButton;
        public Dropdown lobbyListDropdown;

        void Start()
        {
            if (!SteamManager.Initialized) return;
            _manager = GetComponent<CustomNetworkManager>();

            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _joinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _lobbyList = Callback<LobbyMatchList_t>.Create(OnLobbyListRequested);
            RequestLobbyList();
        }

        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, _manager.maxConnections);
        }

        public void JoinLobby(Dropdown ids)
        {
            SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(ids.captionText.text)));
        }

        public void RequestLobbyList()
        {
            SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterClose);
            SteamMatchmaking.RequestLobbyList();
        }

        private void OnLobbyListRequested(LobbyMatchList_t callback)
        {
            for (var i = 0; i < callback.m_nLobbiesMatching; i++)
            {
                var lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                var membersCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                //var image = SteamFriends.GetLargeFriendAvatar(ownerID);

                lobbyListDropdown.options.Add(new Dropdown.OptionData
                {
                    text = "id: " + lobbyID + " members: " + membersCount,
                });
            }
        }


        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK) return;
            Debug.Log("Lobby Created");
            _manager.StartHost();
            var id = new CSteamID(callback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(id, HostAddressKey, SteamUser.GetSteamID().ToString());
            SteamMatchmaking.SetLobbyData(id, NameKey, lobbyNameInput.text);
        }

        private void OnJoinRequest(GameLobbyJoinRequested_t callback)
        {
            Debug.Log("Request to join lobby");
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            var id = new CSteamID(callback.m_ulSteamIDLobby);

            //Everyone
            hostButton.SetActive(false);
            lobbyNameText.gameObject.SetActive(true);
            lobbyNameText.text = SteamMatchmaking.GetLobbyData(id, NameKey);
            lobbyId.gameObject.SetActive(true);
            lobbyId.text = id.ToString();

            //Clients
            if (NetworkServer.active) return;
            _manager.networkAddress = SteamMatchmaking.GetLobbyData(id, HostAddressKey);
            _manager.StartClient();
        }

        private static Sprite GetSteamImageAsSprite(int iImage)
        {
            var bIsValid = SteamUtils.GetImageSize(iImage, pnWidth: out var imageWidth, pnHeight: out var imageHeight);

            if (!bIsValid) return null;
            var image = new byte[imageWidth * imageHeight * 4];

            bIsValid = SteamUtils.GetImageRGBA(iImage, image, (int)(imageWidth * imageHeight * 4));
            if (!bIsValid) return null;
            var ret = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, true);
            ret.LoadRawTextureData(image);
            ret.Apply();

            return Sprite.Create(ret, new Rect(0, 0, imageWidth, imageHeight), Vector2.zero);
        }
    }
}