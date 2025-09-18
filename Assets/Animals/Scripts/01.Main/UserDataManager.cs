using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }

    [Header("gold")]
    [SerializeField] private Text GoldText;
    [Header("diamond")]
    [SerializeField] private Text DiamondText;
    [Header("life")]
    [SerializeField] private Text LifeText;
    
    [Header("currentCharacterId")]
    [SerializeField] private Image Inventory_Character_Img;
    [SerializeField] private Text Inventory_Character_Text;
    [SerializeField] private Image Main_Character_Img;
    [SerializeField] private Image Pedia_Character_Img;
    [SerializeField] private Image RankingMode_Character_Img;
    [SerializeField] private Image CharacterInfo_Character_Img;

    [Header("currentWeaponId")] 
    [SerializeField] private Button GameStartBtn;
    
    public void SetGold()
    {
        if(SceneManager.GetActiveScene().name == "Main")
            GoldText.text = DataManager.instance.userData.Gold.ToString();
    }
    public void SetDiamond()
    {
        if(SceneManager.GetActiveScene().name == "Main")
            DiamondText.text = DataManager.instance.userData.Diamond.ToString();
    }
    public void SetLife()
    {
        if(SceneManager.GetActiveScene().name == "Main")
            LifeText.text = DataManager.instance.userData.Life + "/30";
    }

    public void SetCurrentCharacterId()
    {
        int currentCharacterId = DataManager.instance.userData.CurrentCharacterId;
        
        string characterStandingPath = "CharacterList/CharacterStanding/" + "c" + currentCharacterId.ToString("D3");
        Sprite characterStandingPathImage = Resources.Load<Sprite>(characterStandingPath);

        Inventory_Character_Img.sprite = characterStandingPathImage;
        Main_Character_Img.sprite = characterStandingPathImage;
        Pedia_Character_Img.sprite = characterStandingPathImage;
        RankingMode_Character_Img.sprite = characterStandingPathImage;
        CharacterInfo_Character_Img.sprite = characterStandingPathImage;
    
        //PediaManager json 불러오기전에 호출되어 오류(이후에 또 호출하기에 문제X)
        try {
            Inventory_Character_Text.text = PediaManager.instance.characters[currentCharacterId - 1].name;
        }
        catch (ArgumentException ex) {
            return;
        }
    }

    public void SetCurrentWeaponId()
    {
        var userData = DataManager.instance.userData;
        
        if (0 <= userData.CurrentWeaponId && userData.CurrentWeaponId < userData.equipmentSpecificList.Count)
            GameStartBtn.interactable = true;
        else
            GameStartBtn.interactable = false;
    }
}
