using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Scripts.Enums;
using System;

public class LobbyContentManager : MonoBehaviour
{
    [SerializeField] private GameObject button;


    public void FillLobbyContent(string lobbyName, string selectedBoss)
    {
        var buttonClone = Instantiate(button);
        buttonClone.SetActive(true);
        buttonClone.GetComponentInChildren<TMP_Text>().text = lobbyName;
        buttonClone.transform.parent = transform;
        buttonClone.GetComponent<RectTransform>().transform.position
            = button.GetComponent<RectTransform>().transform.position;

        buttonClone.GetComponent<RectTransform>().transform.localScale
            = button.GetComponent<RectTransform>().transform.localScale;

        buttonClone.GetComponent<RectTransform>().transform.rotation
            = button.GetComponent<RectTransform>().transform.rotation;

        var boss = Enum.Parse<BossNames>(selectedBoss);
        switch (boss)
        {
            case BossNames.Ktulhu:
                buttonClone.GetComponentsInChildren<Image>()[1].sprite = LoadImage("Assets/SlimUI/Modern Menu 1/Graphics/Images/CCP.jpg"); break;
            case BossNames.Dagon:
                buttonClone.GetComponentsInChildren<Image>()[1].sprite = LoadImage("Assets/SlimUI/Modern Menu 1/Graphics/Images/Essence.jpg"); break;
        }
        
    }

    private Sprite LoadImage(string path)
    {
        var imageBytes = File.ReadAllBytes(path);
        Texture2D returningTex = new Texture2D(1,1);
        returningTex.LoadImage(imageBytes);
        return Sprite.Create(returningTex, new Rect(0, 0, 326, returningTex.height), Vector2.zero);
    }

    public void FillBossNames(TMP_Dropdown dropDown)
    {
        dropDown.AddOptions(new List<TMP_Dropdown.OptionData>()
        {
            new TMP_Dropdown.OptionData()
            {
                text = BossNames.Ktulhu.ToString()
            },
            new TMP_Dropdown.OptionData()
            {
                text = BossNames.Dagon.ToString()
            }
        }) ; 
    }

    

}
