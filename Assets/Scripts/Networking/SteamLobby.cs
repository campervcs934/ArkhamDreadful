using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Enums;

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
        private const string BossKey = "boss";
        private CustomNetworkManager _manager;

        //GameObject
        public GameObject hostButton;
        public TMP_Text lobbyNameText;
        public TMP_InputField lobbyNameInput;
        public TMP_Text lobbyId;
        public GameObject joinButton;
        public TMP_Dropdown selectedBossDropDown;
        public GameObject lobbyContent;

        //Services
        private LobbyContentManager _lobbyContentManager;

        void Start()
        {
            if (!SteamManager.Initialized) return;
            _manager = GetComponent<CustomNetworkManager>();

            _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            _joinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
            _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            _lobbyList = Callback<LobbyMatchList_t>.Create(OnLobbyListRequested);
            _lobbyContentManager = lobbyContent.GetComponent<LobbyContentManager>();
            RequestLobbyList();
            _lobbyContentManager.FillBossNames(selectedBossDropDown);

        }

        public void HostLobby()
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, _manager.maxConnections);
        }

        public void JoinLobby(TMP_Text ids)
        {
            SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(ids.text)));


        }

        public void RequestLobbyList()
        {
           
           // SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterClose);
            SteamMatchmaking.RequestLobbyList();
        }

        private void OnLobbyListRequested(LobbyMatchList_t callback)
        {
            for (var i = 0; i < callback.m_nLobbiesMatching; i++)
            {
                var lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
                var membersCount = SteamMatchmaking.GetNumLobbyMembers(lobbyID);
                var boss = SteamMatchmaking.GetLobbyData(lobbyID, BossKey);
                boss = string.IsNullOrEmpty(boss) ? BossNames.Dagon.ToString() : boss;
                var lobbyName = SteamMatchmaking.GetLobbyData(lobbyID, NameKey);
               
                _lobbyContentManager.FillLobbyContent(lobbyName, boss, lobbyID.ToString());
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
            SteamMatchmaking.SetLobbyData(id, BossKey, selectedBossDropDown.options[selectedBossDropDown.value].text);
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
            lobbyNameInput.gameObject.SetActive(false);
            lobbyId.text = id.ToString();

            //Clients
            if (NetworkServer.active) return;
            _manager.networkAddress = SteamMatchmaking.GetLobbyData(id, HostAddressKey);
            _manager.StartClient();
        }

        //private static Sprite GetSteamImageAsSprite(int iImage)
        //{
        //    var bIsValid = SteamUtils.GetImageSize(iImage, pnWidth: out var imageWidth, pnHeight: out var imageHeight);

        //    if (!bIsValid) return null;
        //    var image = new byte[imageWidth * imageHeight * 4];

        //    bIsValid = SteamUtils.GetImageRGBA(iImage, image, (int)(imageWidth * imageHeight * 4));
        //    if (!bIsValid) return null;
        //    var ret = new Texture2D((int)imageWidth, (int)imageHeight, TextureFormat.RGBA32, false, true);
        //    ret.LoadRawTextureData(image);
        //    ret.Apply();

        //    return Sprite.Create(ret, new Rect(0, 0, imageWidth, imageHeight), Vector2.zero);
        //}

        public void SearchLobby(TMP_InputField nameLobby)
        {
  
          SteamMatchmaking.AddRequestLobbyListStringFilter(NameKey, nameLobby.text, ELobbyComparison.k_ELobbyComparisonEqual);
          SteamMatchmaking.RequestLobbyList();
          Debug.Log(nameLobby.text);

        }
    }
}