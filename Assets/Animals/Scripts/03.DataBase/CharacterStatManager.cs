using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterStatManager : MonoBehaviour
{
    [Header("TapTage")] 
    [SerializeField] private GameObject tapContainer;
    [SerializeField] private Button statButton;
    [SerializeField] private Button skillButton;

    [Header("CharacterStat")] 
    [SerializeField] private Text StatDescription;
    [SerializeField] private Text goldText;
    [SerializeField] private Text expText;
    [SerializeField] private Button levelUpButton;
    
    [Header("CharacterSkill")]
    [SerializeField] private List<SkillBox> skillBoxes;
    [SerializeField] private Transform skillBox;
    [SerializeField] private Text skillPointNumText;
    
    [Header("CharacterSkillConfirm")]
    [SerializeField] private Image skillImage;
    [SerializeField] private Text skillNameText;
    [SerializeField] private Text skillDescriptionText;
    
    [Space(5f)]
    [SerializeField] private GameObject textContainer;
    [SerializeField] private GameObject agreeText;
    [SerializeField] private GameObject lackText;
    [SerializeField] private Text requestSkillPointText;

    [Space(5f)] 
    [SerializeField] private GameObject buttonContainer;
    [SerializeField] private Button yesButton;
    public Button YesButton => yesButton;
    [SerializeField] private Button noButton;
    [NonSerialized] public int skillPoint;

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        statButton.onClick.AddListener(() => ShowTap(statButton.gameObject));
        skillButton.onClick.AddListener(() => ShowTap(skillButton.gameObject));
        levelUpButton.onClick.AddListener(LevelUp);
    }

    private void Init()
    {
        //스킬트리 관련
        foreach (var skillBox in skillBoxes)
            skillBox.Init();
    }
    
    /*탭 관련 함수*/
    private void ShowTap(GameObject tapObject)
    {
        tapObject.transform.parent.SetAsLastSibling(); // 스탯 정보, 스킬 정보 화면 스왑
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Book);
    }
    
    public void Set() // 레벨업 버튼 호출
    {
        var characterId = DataManager.instance.userData.CurrentCharacterId;
        var characterSpecific = DataManager.instance.userData.characterInforms.Find(x => x.id == characterId);
        var characterSkillList = characterSpecific.characterSkillList;
        
        //스킬트리 관련
        // 스킬 아이콘 반영
        SetSkillIcon(characterId);
        // 스킬 포인트 반영
        skillPoint = characterSpecific.level + 1;
        
        for (int i = 0; i < characterSkillList.Count; i++) {
            for (int j = 0; j < characterSkillList[i]; j++) {
                int num = i;
                skillBoxes[num].OnSkill();
            }
        }

        RefreshSkillPointText();
        
        //스탯 관련
        var expData = ExpManager.instance.GetExpData(characterSpecific.level);
        if (expData != null) {
            goldText.text = expData.requiredGold.ToString();
            expText.text = characterSpecific.experience + " / " + expData.requiredExp;
            
            if (characterSpecific.experience < expData.requiredExp ||
                DataManager.instance.userData.Gold < expData.requiredGold) {
                levelUpButton.interactable = false;
            }
            else {
                levelUpButton.interactable = true;
            }
        }
        else {
            goldText.text = "Max";
            expText.text = "Max";
            levelUpButton.interactable = false;
        }

        SetStatDescription();
    }

    /*캐릭터 스탯 관련 함수*/
    private void LevelUp()
    {
        var characterId = DataManager.instance.userData.CurrentCharacterId;
        var characterSpecific = DataManager.instance.userData.characterInforms.Find(x => x.id == characterId);
        
        var expData = ExpManager.instance.GetExpData(characterSpecific.level);

        if (characterSpecific.experience < expData.requiredExp ||
            DataManager.instance.userData.Gold < expData.requiredGold) {
            return;
        }

        DataManager.instance.userData.Gold -= expData.requiredGold;
        characterSpecific.experience -= expData.requiredExp;
        characterSpecific.level += 1;
        
        DataManager.instance.SaveUserData();
        skillPoint = characterSpecific.level;
        RefreshSkillPointText();

        expData = ExpManager.instance.GetExpData(characterSpecific.level);
        if (expData != null) {
            goldText.text = expData.requiredGold.ToString();
            expText.text = characterSpecific.experience + " / "  + expData.requiredExp;
            
            if (characterSpecific.experience < expData.requiredExp ||
                DataManager.instance.userData.Gold < expData.requiredGold) {
                levelUpButton.interactable = false;
            }
            else {
                levelUpButton.interactable = true;
            }
        }
        else {
            goldText.text = "Max";
            expText.text = "Max";
        }
        
        SetStatDescription();
        DataManager.instance.ChangeBasicStats();
        MissionManager.instance.MissionClearCheck(6);
    }

    private void SetStatDescription()
    {
        //StatDescription.text
        string description = null;
        
        var characterId = DataManager.instance.userData.CurrentCharacterId;
        var characterSpecific = DataManager.instance.userData.characterInforms.Find(x => x.id == characterId);
        var characterData = PediaManager.instance.characters[characterId - 1];
        
        if (characterSpecific.level - 1 == 0) {
            description += "체력 : " + characterData.hp;
            description += "\n공격력 : " + characterData.attackPower;
            description += "\n공격속도 : " + characterData.attackSpeed;
            description += "\n이동속도 : " + characterData.moveSpeed;
        }
        else {
            description += "체력 : " + characterData.hp
                                   + " (+" + (characterSpecific.level - 1) + ")";
            description += "\n공격력 : " + characterData.attackPower
                                    + " (+" + (characterSpecific.level - 1) + ")";
            description += "\n공격속도 : " + characterData.attackSpeed
                                       + " (+" + (characterSpecific.level - 1) + ")";
            description += "\n이동속도 : " + characterData.moveSpeed;
        }
        

        StatDescription.text = description;
    }
    
    /*스킬트리 관련 함수*/
    public void RefreshSkillPointText() => skillPointNumText.text = skillPoint.ToString();
    public void SetRequestSkillPoint(int num) => requestSkillPointText.text = num.ToString();

    public void IsEnoughSkillPoint(bool isEnough)
    {
        if (isEnough)
        {
            agreeText.SetActive(true);
            lackText.SetActive(false);
            //yesButton.gameObject.SetActive(true);
            yesButton.interactable = true;
            noButton.transform.GetChild(0).GetComponent<Text>().text = "아니요";
        }
        else
        {
            agreeText.SetActive(false);
            lackText.SetActive(true);
            //yesButton.gameObject.SetActive(false);
            yesButton.interactable = false;
            noButton.transform.GetChild(0).GetComponent<Text>().text = "돌아가기";
        }
    }

    public void IsMaxSkillCount(bool isMax)
    {
        if (!isMax)
        {
            textContainer.SetActive(true);
            buttonContainer.SetActive(true);
        }
        else
        {
            textContainer.SetActive(false);
            buttonContainer.SetActive(false);
        }
    }
    
    private void SetSkillIcon(int id)
    {
        var characterData = PediaManager.instance.characters;
        //임시
        List<SkillData> skillDatas = null;
        TextAsset jsonFile = Resources.Load<TextAsset>("GameData/SkillData");
        if (jsonFile != null)
        {
            skillDatas = JsonConvert.DeserializeObject<List<SkillData>>(jsonFile.text);
        }

        for (int i = 0; i < characterData[id - 1].characterSkillNameList.Count; i++)
        {
            SkillData skillData = skillDatas.Find(s => s.id == characterData[id - 1].characterSkillNameList[i]);
            skillBoxes[i].IconImage.GetComponent<Image>().sprite = Resources.Load<Sprite>("CharacterSkillIcon/" + skillData.id);
            skillBoxes[i].skillName = skillData.name;
            //skillBoxes[i].skillDescription = skillData.description;
            skillBoxes[i].skillDescription = string.Format(skillData.description, characterData[id - 1].characterSkillVariableList[i]);
        }
    }

    public void SetSkillDescription(Sprite skillSprite, string skillNameString, string skillDescriptionString)
    {
        skillImage.sprite = skillSprite;
        skillNameText.text = skillNameString;
        skillDescriptionText.text = skillDescriptionString;
    }
    public void ResetYesButton()
    {
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() => noButton.onClick.Invoke());
    }
    public void SaveUserData()
    {
        var characterId = DataManager.instance.userData.CurrentCharacterId;
        var characterSkillList =
            DataManager.instance.userData.characterInforms.Find(x => x.id == characterId).characterSkillList;

        for (int i = 0; i < characterSkillList.Count; i++) {
            characterSkillList[i] = skillBoxes[i].SkillCount;
        }
        
        DataManager.instance.SaveUserData();
    }
}
