using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;

//using UnityEditor.Purchasing;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    [SerializeField] protected int pointAmount = 0;
    [SerializeField] protected Text pointText;
    [SerializeField] protected GameObject BGPanel;
    [SerializeField] private Image fadeInOut;

    [Header("Game Result Popup")]
    [SerializeField] protected RectTransform gameResultRect;
    [SerializeField] protected Image gameResultBorder;
    [SerializeField] protected Sprite loseSprite;
    [SerializeField] protected Text gameResultText;
    [SerializeField] protected Text _pointText;
    [SerializeField] protected Button restartButton;
    [SerializeField] protected Button mainMenuButton;

    [Header("Resurrection Popup")]
    [SerializeField] protected RectTransform resurrectionRect;
    [SerializeField] protected TextMeshProUGUI resurrectionAmountText;
    [SerializeField] protected Button yesButton;
    [SerializeField] protected Button noButton;
    private bool bisResurrection = false;

    [Header("Setting Popup")] 
    [SerializeField] protected RectTransform settingRect;
    [SerializeField] protected Button resumeButton;
    [SerializeField] protected Button homeButton;
    [Space(10f)]
    [SerializeField] private DropItemData dropItemData;
    [SerializeField] private Transform itemFrame;
    [SerializeField] private List<DropItemType> itemIdData = new List<DropItemType>();
    [SerializeField] private List<int> itemAmountData = new List<int>();

    [Header("Skill Setting")]
    [SerializeField] protected RectTransform skillsRect;
    [SerializeField] protected Button[] skillsButton;

    [Space(10f)]
    [SerializeField] protected bool bisWin;
    [SerializeField] protected bool bisBossDead;
    public bool isWin { get { return bisWin; } set { bisWin = value; SetGame(); } }
    private float timer;
    protected StageData stageData;
    public int enemyTimes = 1;

    protected virtual void Awake()
    {
        instance = this;
        restartButton.onClick.AddListener(() => _SceneManager.instance.GameStart(_SceneManager.stageIdx));
        mainMenuButton.onClick.AddListener(() => _SceneManager.instance.MainMenu());
        Time.timeScale = 1.0f;
        SoundManager.instance.SetSFXPitchByTimeScale();
        pointText.text = pointAmount.ToString();
        _pointText.text = pointAmount.ToString();
        // coinText.text = coinAmount.ToString();
        // _coinText.text = coinAmount.ToString();

        gameResultRect.transform.localScale = Vector3.zero;
        resurrectionRect.transform.localScale = Vector3.zero;
        settingRect.transform.localScale = Vector3.zero;
        skillsRect.transform.localScale = Vector3.zero;
        BGPanel.SetActive(false);
        
        yesButton.onClick.AddListener(Resurrection);
        noButton.onClick.AddListener(Lose);
        
        resumeButton.onClick.AddListener(() => HidePopup(settingRect));
        homeButton.onClick.AddListener(() => _SceneManager.instance.MainMenu());

        foreach (Transform child in itemFrame)
            foreach (Transform cchild in child)
                cchild.gameObject.SetActive(false);

        stageData = StageManager.instance.stageData;
        //var a = DataManager.instance.userData;
        //var stageFulltime = stageData.stageTime * stageData.stageStep;
        //DOVirtual.DelayedCall(stageFulltime, () => isWin = true, false);

        //Debug.LogError(dropItemData.EquipmentProbability + " " + dropItemData.EquipmentProbability);
        var temp = Instantiate(dropItemData) as DropItemData;
        dropItemData = temp;
        //Debug.LogError(dropItemData.AllProbability);
         for (int i = 0; i < stageData.dropItemType.Count; i++) {
             dropItemData.SetProbability(stageData.dropItemType[i], stageData.dropItemRate[i]);
         }
        
        StartCoroutine(FadeInOut());
        
        //Game BGM 재생***************************************************
        var gameBGM = (SoundManager.GAME_BGM)Enum.Parse(typeof(SoundManager.GAME_BGM), stageData.stageType.ToString());
        SoundManager.instance.PlayBGM(gameBGM);

        //플레이어 생성
        var user_Character = DataManager.instance.userData.CurrentCharacterId;
        var characterPrefab = Resources.Load<GameObject>("Prefabs/InGame/Player/" + "c" + user_Character.ToString("D3"));
        Instantiate(characterPrefab, new Vector3(0, -6, 0), new Quaternion(0, 0, 0, 0)).name = "Player";
    }

    private void Start()
    {
        PlayerController.instance.maxLevel = 11;
        PlayerController.instance.gameObject.transform.DOMoveY(-3, 1f);
    }

    public void AddPoint(int point)
    {
        pointAmount += point;
        //pointText.DOCounter(int.Parse(Regex.Replace(pointText.text, @"\D", "")), pointAmount, 0.2f).SetUpdate(true);
        pointText.DOCounter(int.Parse(pointText.text), pointAmount, 0.2f, false).SetUpdate(true);
    }

    // public void AddCoin(int coin)
    // {
    //     coinAmount += coin;
    //     coinText.DOCounter(coinAmount - coin, coinAmount, 0.2f).SetDelay(0.75f).SetUpdate(true);
    // }
    protected virtual void SetGame()
    {
        if (isWin) Win();
        else Lose();
    }

    protected void Win()
    {
        gameResultText.text = "Win";
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Victory);
        SetData();
        
        Time.timeScale = 0.0f;
        SoundManager.instance.SetSFXPitchByTimeScale();
        gameResultRect.DOScale(Vector3.one, 0.2f).SetUpdate(true);
        _pointText.DOCounter(0, pointAmount, 0.5f).SetDelay(0.2f).SetUpdate(true);
        // _coinText.DOCounter(0, coinAmount, 0.5f).SetDelay(0.2f).SetUpdate(true);
    }

    protected void Lose()
    {
        Time.timeScale = 0.0f;
        SoundManager.instance.SetSFXPitchByTimeScale();
        var itemList = DataManager.instance.userData.itemList;
        if (itemList.Exists(x => x.itemType == ItemType.ResurrectionTicket && x.amount > 0) &&
            bisResurrection == false) {
            bisResurrection = true;
            resurrectionRect.DOScale(Vector3.one, 0.2f).SetUpdate(true);
        }
        else {
            gameResultBorder.sprite = loseSprite;
            gameResultText.text = "Lose";
            SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Defeat);
            SetData();
        
            gameResultRect.DOScale(Vector3.one, 0.2f).SetUpdate(true);
            _pointText.DOCounter(0, pointAmount, 0.5f).SetDelay(0.2f).SetUpdate(true);
            // _coinText.DOCounter(0, coinAmount, 0.5f).SetDelay(0.2f).SetUpdate(true);
        }
    }

    private void Resurrection()
    {
        Time.timeScale = 1.0f;
        SoundManager.instance.SetSFXPitchByTimeScale();
        DataManager.instance.userData.itemList.Find(x => x.itemType == ItemType.ResurrectionTicket).amount -= 1;
        PlayerController.instance.HealHp(100f);
        bisResurrection = true;
        resurrectionRect.DOScale(Vector3.zero, 0.2f).SetUpdate(true);
    }
    
    protected virtual void SetData()
    {
        if (isWin) {
            GoodsManager.instance.IncreaseGoods(0, stageData.rewardGold + pointAmount, 0);
            var characterId = DataManager.instance.userData.CurrentCharacterId;
            var characterSpecific = DataManager.instance.userData.characterInforms.Find(x => x.id == characterId);
            characterSpecific.experience += 100;
            //DataManager.instance.userData.stageClearCheck[stageData]
            if(DataManager.instance.userData.stageClearCheck[stageData.stageId - 1] <
               StageManager.instance.stageMultipleIndex + 1)
            DataManager.instance.userData.stageClearCheck[stageData.stageId - 1] =
                StageManager.instance.stageMultipleIndex + 1;
        }
        else {
            GoodsManager.instance.IncreaseGoods(0, (stageData.rewardGold + pointAmount) / 3, 0);
        }
        
        MissionManager.instance.MissionClearCheck(2, EnemyManager.instance.TotalKillCount);
        MissionManager.instance.MissionClearCheck(3);

        // 아이템 재료 갯수 구하기
        var dropItemIndexList = itemIdData
            .Select((value, index) => new { value, index })
            .Where(x => x.value is DropItemType.Tree or DropItemType.Iron or DropItemType.Cotton or DropItemType.MagicPowder or DropItemType.Emerald)
            .Select(x => x.index)
            .ToList();
        var materialAmount = dropItemIndexList.Sum(dropItemIndex => itemAmountData[dropItemIndex]);
        MissionManager.instance.MissionClearCheck(7, materialAmount);
        
        MissionManager.instance.MissionClearCheck(10, 1);
        
        Destroy(dropItemData);
        // DataManager.instance.RecordRanking(pointAmount);
        DataManager.instance.SaveUserData();
    }

    public void ShowPopup(RectTransform _rect)
    {
        Time.timeScale = 0.0f;
        BGPanel.SetActive(true);
        _rect.DOScale(Vector3.one, 0.2f).SetUpdate(true);
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Button);
    }

    public void HidePopup(RectTransform _rect)
    {
        Time.timeScale = 1.0f;
        BGPanel.SetActive(false);
        _rect.DOScale(Vector3.zero, 0.2f).SetUpdate(true);
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Close);
    }

    public void LevelUp()
    {
        ShowPopup(skillsRect);
        SetSkills();
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Skill);
    }

    // private void SetSkillsTemp()
    // {
    //     skillsButton[0].onClick.RemoveAllListeners();
    //     skillsButton[0].onClick.AddListener(SkillManager.instance.GetSkill("멀티샷").InvokeSkillEvent);
    //     skillsButton[0].onClick.AddListener(() => SkillManager.instance.ConfirmSkill("멀티샷"));
    //     skillsButton[0].onClick.AddListener(() => HidePopup(skillsRect));
    //     
    //     skillsButton[1].onClick.RemoveAllListeners();
    //     skillsButton[1].onClick.AddListener(SkillManager.instance.GetSkill("더블샷").InvokeSkillEvent);
    //     skillsButton[1].onClick.AddListener(() => SkillManager.instance.ConfirmSkill("더블샷"));
    //     skillsButton[1].onClick.AddListener(() => HidePopup(skillsRect));
    //     
    //     skillsButton[2].onClick.RemoveAllListeners();
    //     skillsButton[2].onClick.AddListener(SkillManager.instance.GetSkill("트리플 버스트").InvokeSkillEvent);
    //     skillsButton[2].onClick.AddListener(() => SkillManager.instance.ConfirmSkill("트리플 버스트"));
    //     skillsButton[2].onClick.AddListener(() => HidePopup(skillsRect));
    // }

    protected void SetSkills()
    {
        List<int> randomSkillNumber = GetSkillNumbers(SkillManager.instance.Skills, skillsButton.Length);

        for (int i = 0; i < skillsButton.Length; i++)
        {
            int num = i; // 람다 함수를 사용하기에, 변수를 따로 선언 및 복사
            Skill skill = SkillManager.instance.Skills[randomSkillNumber[num]];
            SkillData skillData = SkillManager.instance.GetSkillData(skill.skillName);
            
            // skillsButton[num].transform.Find("skill_image").GetComponent<Image>().sprite =
            //     Resources.Load<Sprite>("CharacterSkillIcon/" + skillData.id);
            // skillsButton[num].transform.Find("skill_text").GetComponent<Text>().text = skillData.name;
            // skillsButton[num].transform.Find("skill_description_text").GetComponent<Text>().text = skillData.description;
            
            skillsButton[num].transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite =
                Resources.Load<Sprite>("CharacterSkillIcon/" + skillData.id);
            skillsButton[num].transform.GetChild(1).GetChild(0).GetComponent<Text>().text = skillData.name;
            skillsButton[num].transform.GetChild(1).GetChild(2).GetComponent<Text>().text =
                string.Format(skillData.description, "");

            
            skillsButton[num].onClick.RemoveAllListeners();
            skillsButton[num].onClick.AddListener(skill.InvokeSkillEvent);
            skillsButton[num].onClick.AddListener(() => SkillManager.instance.ConfirmSkill(randomSkillNumber[num]));
            skillsButton[num].onClick.AddListener(() => HidePopup(skillsRect));
        }
    }

    List<int> GetSkillNumbers(List<Skill> skillDatas, int needLength)
    {
        HashSet<int> selectedIndices = new HashSet<int>(); // 중복 방지를 위한 HashSet
        List<float> probabilities = new List<float>();

        // 스킬의 확률을 기반으로 리스트 생성
        foreach (var skillData in skillDatas)
            probabilities.Add(skillData.probability);

        // 총 확률 계산
        float totalProbability = probabilities.Sum();

        // 필요한 스킬 수만큼 선택
        while (selectedIndices.Count < needLength && selectedIndices.Count < skillDatas.Count)
        {
            float randomValue = Random.Range(0f, totalProbability);
            float cumulativeProbability = 0;

            for (int i = 0; i < skillDatas.Count; i++)
            {
                cumulativeProbability += probabilities[i];
                if (randomValue <= cumulativeProbability && selectedIndices.Add(i)) // 중복되지 않으면 추가
                {
                    break;
                }
            }
        }

        return selectedIndices.ToList(); // 중복 없이 선택된 인덱스를 리스트로 변환하여 반환
    }
    
    public Sprite GetDropItem(DropType dropType, bool isGuarantee = false)
    {
        var dropItem = dropItemData.GetItem(dropType, isGuarantee);
        if (dropItem == null) return null;
        
        var index = itemIdData.IndexOf(dropItem.id);
        
        if (index == -1) // 해당 아이템이 itemNameData에 없다면
        {
            var item = itemFrame.GetChild(itemIdData.Count);
            item.gameObject.SetActive(true);
            
            itemIdData.Add(dropItem.id);
            itemAmountData.Add(1);

            foreach (Transform child in item)
                child.gameObject.SetActive(true);
            
            item.Find("Image").GetComponent<Image>().sprite = dropItem.sprite;
            item.Find("NumberText").GetComponent<Text>().text = "x" + itemAmountData[itemIdData.Count - 1];
        }
        else
        {
            var item = itemFrame.GetChild(index);
            
            itemAmountData[index]++;
            
            item.Find("NumberText").GetComponent<Text>().text = "x" + itemAmountData[index];
        }
        
        return dropItem.sprite;
    }
    IEnumerator FadeInOut()
    {
        fadeInOut.gameObject.SetActive(true);
        float corner = GameObject.Find("UI").GetComponent<RectTransform>().rect.height;
        float timer = 0f; // 타이머 초기화
        
        Image maskBg = fadeInOut.transform.gameObject.transform.Find("FadeInOut_Bg").GetComponent<Image>();
        RectTransform rectTransform = fadeInOut.GetComponent<RectTransform>();
        Vector2 initialSize = rectTransform.sizeDelta; // 초기 크기 저장
        Vector2 targetSize = new Vector2(corner, corner); // 목표 크기는 corner 값

        maskBg.rectTransform.sizeDelta = new Vector2(corner, corner);  // 배경 크기는 전체 화면 사이즈
        // maskBg.rectTransform.SetHeight(corner);  // 배경 크기는 전체 화면 사이즈
        // maskBg.rectTransform.SetWidth(corner);
        yield return new WaitForSeconds(0.5f); // 0.5초 후 드러다니 시작

        while (rectTransform.sizeDelta.magnitude < targetSize.magnitude) // 크기가 목표에 근접할 때까지
        {
            timer += Time.deltaTime / 0.2f; // 페이드 인아웃 시간
            rectTransform.sizeDelta = Vector2.Lerp(initialSize, targetSize, timer); // 크기 보간
            yield return null;
        }

        rectTransform.sizeDelta = targetSize; // 최종 크기 설정
        fadeInOut.gameObject.SetActive(false); // 비활성화
    }
}
