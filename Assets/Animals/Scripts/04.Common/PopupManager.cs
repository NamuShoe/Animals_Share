using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public enum PopupType
{
    GameModeInfo = 0,
    RankingStageInfo,
    StageInfo,
    ProfileDetail,
    SettingInfo,
    LogOutWarning,
    EnterUserID,
    LifePurchase,
    UpgradeInfo,
    ItemInfo,
    DailyReward,
    RankingReward,
    BattlePassInfo,
    EquipInfo,
    CharacterInfo,
    CharacterSkillConfirm,
    GachaInfo,
    BoxInfo
}

public enum TapType {
    Fade,
    Scale
}

// PopupType 추가 시, Popup GameObject, Exit Popup Button, GetPoupGameObject, GetExitPopupButton 필수 추가
public class PopupManager : MonoBehaviour
{
    public static PopupManager instance;

    [SerializedDictionary] [SerializeField]
    private SerializedDictionary<PopupType, TapStructure> TapDictaionary;
    
    // [Header("Open Popup Button")]
    // [SerializeField] private Button Open_GameModeInfo; //게임모드
    // [SerializeField] private Button Open_RankingStageInfo; //랭킹스테이지
    // [SerializeField] private Button Open_StageInfo; //스테이지
    // [SerializeField] private Button Open_ProfileDetail; //프로필
    // [SerializeField] private Button Open_SettingInfo; //환경설정
    // [SerializeField] private Button Open_LogOutWarning; //로그아웃 경고
    // [SerializeField] private Button Open_EnterUserID; //UID로 불러오기
    // [SerializeField] private Button Open_UpgradeInfo; //합성 제작소
    // [SerializeField] private Button Open_ItemInfo; //상품 구매 안내
    // [SerializeField] private Button Open_DailyReward; //일일 보상
    // [SerializeField] private Button Open_RankingReward; //랭킹 보상
    // [SerializeField] private Button Open_BattlePassInfo; //배틀패스
    // [SerializeField] private Button Open_EquipInfo; //장비 장착&해제
    // [SerializeField] private Button Open_CharacterInfo; // 캐릭터 스킬
    // [SerializeField] private Button Open_LifePurchase; //라이프 구매
    //
    // [Header("Popup GameObject")]
    // [SerializeField] private GameObject GameModeInfo; //게임모드
    // [SerializeField] private GameObject RankingStageInfo; //랭킹스테이지
    // [SerializeField] private GameObject StageInfo; //스테이지
    // [SerializeField] private GameObject ProfileDetail; //프로필
    // [SerializeField] private GameObject SettingInfo; //환경설정
    // [SerializeField] private GameObject LogOutWarning; //로그아웃 경고
    // [SerializeField] private GameObject EnterUserID; //UID로 불러오기
    // [SerializeField] private GameObject UpgradeInfo; //합성 제작소
    // [SerializeField] private GameObject ItemInfo; //상품 구매 안내
    // [SerializeField] private GameObject DailyReward; //일일 보상
    // [SerializeField] private GameObject RankingReward; //랭킹 보상
    // [SerializeField] private GameObject BattlePassInfo; //배틀패스
    // [SerializeField] private GameObject EquipInfo; //장비 장착&해제
    // [SerializeField] private GameObject CharacterInfo; // 캐릭터 스킬
    // [SerializeField] private GameObject CharacterSkillConfirm; // 캐릭터 스킬 확인
    // [SerializeField] private GameObject LifePurchase; //라이프 구매
    //
    // [Header("Exit Popup Button")]
    // [SerializeField] private Button Exit_GameModeInfo; //게임모드
    // [SerializeField] private Button Exit_RankingStageInfo; //랭킹스테이지
    // [SerializeField] private Button Exit_StageInfo; //스테이지
    // [SerializeField] private Button Exit_ProfileDetail; //프로필
    // [SerializeField] private Button Exit_SettingInfo; //환경설정
    // [SerializeField] private Button[] Exit_LogOutWarning; //로그아웃 경고
    // [SerializeField] private Button Exit_EnterUserID; //UID로 불러오기
    // [SerializeField] private Button Exit_UpgradeInfo; //합성 제작소
    // [SerializeField] private Button Exit_ItemInfo; //상품 구매 안내
    // [SerializeField] private Button Exit_DailyReward; //일일 보상
    // [SerializeField] private Button Exit_RankingReward; //랭킹 보상
    // [SerializeField] private Button Exit_BattlePassInfo; //배틀패스
    // [SerializeField] private Button Exit_EquipInfo; //장비 장착&해제
    // [SerializeField] private Button Exit_CharacterInfo; // 캐릭터 스킬
    // [SerializeField] private Button Exit_CharacterSkillConfirm; // 캐릭터 스킬 확인
    // [SerializeField] private Button Exit_LifePurchase; //라이프 구매
    
    [Header("BackPanel")]
    [SerializeField] private GameObject BackGroundPanel;
    private Stack<PopupType> popupStack = new Stack<PopupType>();
    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        popupStack.Clear();

        foreach (var tap in TapDictaionary) {
            if(tap.Value.openButton != null)
                tap.Value.openButton.onClick.AddListener(() => OpenTap((int)tap.Key));
            if(tap.Value.closeButton != null)
                tap.Value.closeButton.onClick.AddListener(() => CloseTap((int)tap.Key));
        }
        
        TapDictaionary[PopupType.GameModeInfo].closeButton.onClick.AddListener(() => SoundManager.instance.PlayMainBGM());
        
        // Open_GameModeInfo.onClick.AddListener(() => OpenTap((int)PopupType.GameModeInfo));
        // Open_RankingStageInfo.onClick.AddListener(() => OpenTap((int)PopupType.RankingStageInfo));
        // Open_StageInfo.onClick.AddListener(() => OpenTap((int)PopupType.StageInfo));
        // Open_ProfileDetail.onClick.AddListener(() => OpenTap((int)PopupType.ProfileDetail));
        // Open_SettingInfo.onClick.AddListener(() => OpenTap((int)PopupType.SettingInfo));
        // Open_LogOutWarning.onClick.AddListener(() => {OpenTap((int)PopupType.LogOutWarning); CloseTap((int)PopupType.SettingInfo);});
        //
        // Open_EnterUserID.onClick.AddListener(() => {OpenTap((int)PopupType.EnterUserID); CloseTap((int)PopupType.SettingInfo);});
        // Open_LifePurchase.onClick.AddListener(() => OpenTap((int)PopupType.LifePurchase));
        // //Open_UpgradeInfo.onClick.AddListener(() => OpenTap((int)PopupType.UpgradeInfo));
        // //Open_ItemInfo.onClick.AddListener(() => OpenTap((int)PopupType.ItemInfo)); // List로 구현해야 하나?
        // //Open_DailyReward.onClick.AddListener(() => OpenTap((int)PopupType.DailyReward)); // 버튼 필요
        // //Open_RankingReward.onClick.AddListener(() => OpenTap((int)PopupType.RankingReward)); // 버튼 필요
        // Open_BattlePassInfo.onClick.AddListener(() => OpenTap((int)PopupType.BattlePassInfo));
        // //Open_EquipInfo.onClick.AddListener(() => OpenTap((int)PopupType.EquipInfo)); // 씬에 별도 오브젝트 없음
        // Open_CharacterInfo.onClick.AddListener(() => OpenTap((int)PopupType.CharacterInfo)); // 임시버튼
        //
        //
        //
        // Exit_GameModeInfo.onClick.AddListener(() => CloseTap((int)PopupType.GameModeInfo));
        // Exit_RankingStageInfo.onClick.AddListener(() => CloseTap((int)PopupType.RankingStageInfo));
        // Exit_StageInfo.onClick.AddListener(() => CloseTap((int)PopupType.StageInfo));
        // Exit_ProfileDetail.onClick.AddListener(() => CloseTap((int)PopupType.ProfileDetail));
        // Exit_SettingInfo.onClick.AddListener(() => CloseTap((int)PopupType.SettingInfo));
        // foreach (var button in Exit_LogOutWarning) {
        //     button.onClick.AddListener(() => {CloseTap((int)PopupType.LogOutWarning); OpenTap((int)PopupType.SettingInfo);});
        // }
        // Exit_EnterUserID.onClick.AddListener(() => {CloseTap((int)PopupType.EnterUserID); OpenTap((int)PopupType.SettingInfo);});
        // Exit_LifePurchase.onClick.AddListener(() => CloseTap((int)PopupType.LifePurchase));
        // Exit_UpgradeInfo.onClick.AddListener(() => CloseTap((int)PopupType.UpgradeInfo));
        // Exit_ItemInfo.onClick.AddListener(() => CloseTap((int)PopupType.ItemInfo));
        // Exit_DailyReward.onClick.AddListener(() => CloseTap((int)PopupType.DailyReward));
        // Exit_RankingReward.onClick.AddListener(() => CloseTap((int)PopupType.RankingReward));
        // Exit_BattlePassInfo.onClick.AddListener(() => CloseTap((int)PopupType.BattlePassInfo));
        // Exit_EquipInfo.onClick.AddListener(() => CloseTap((int)PopupType.EquipInfo));
        // Exit_CharacterInfo.onClick.AddListener(() => CloseTap((int)PopupType.CharacterInfo));
        // Exit_CharacterSkillConfirm.onClick.AddListener(() => CloseTap((int)PopupType.CharacterSkillConfirm));
    }

    [VisibleEnum(typeof(PopupType))]
    public void OpenTap(int popupType, bool isSound = true) // 이름으로 확인하여 열고 닫기
    {
        //GameObject gameObject = GetPopupGameObject(popupType);
        GameObject gameObject = TapDictaionary[(PopupType)popupType].gameObject;
        switch ((PopupType)popupType)
        {
            case PopupType.GameModeInfo:
            case PopupType.RankingStageInfo:
            case PopupType.StageInfo:
            case PopupType.GachaInfo:
                OpenTapFade(gameObject);
                break;
            case PopupType.ProfileDetail:
            case PopupType.SettingInfo:
            case PopupType.LogOutWarning:
            case PopupType.EnterUserID:
            case PopupType.LifePurchase:
            case PopupType.UpgradeInfo:
            case PopupType.ItemInfo:
            case PopupType.DailyReward:
            case PopupType.RankingReward:
            case PopupType.BattlePassInfo:
            case PopupType.EquipInfo:
            case PopupType.CharacterInfo:
            case PopupType.CharacterSkillConfirm:
            case PopupType.BoxInfo:
                OpenTapScale(gameObject);
                break;
            default:
                Debug.Log("잘못된 PopupType 기입");
                return;
        }
        SetBackGroundPanelIndex(popupType);
        popupStack.Push((PopupType)popupType);
        
        //Button temp = GetExitPopupButton(popupType);
        Button temp = TapDictaionary[(PopupType)popupType].closeButton;
        BackGroundPanel.GetComponent<Button>().onClick.RemoveAllListeners();
        BackGroundPanel.GetComponent<Button>().onClick.AddListener(() => temp.onClick.Invoke());
        
        //SFX 호출
        if(isSound)
            SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Button);
    }
    
    [VisibleEnum(typeof(PopupType))]
    public void CloseTap(int popupType)
    {
        //GameObject gameObject = GetPopupGameObject(popupType);
        GameObject gameObject = TapDictaionary[(PopupType)popupType].gameObject;
        switch ((PopupType)popupType)
        {
            case PopupType.GameModeInfo:
            case PopupType.RankingStageInfo:
            case PopupType.StageInfo:
            case PopupType.GachaInfo:
                CloseTapFade(gameObject);
                break;
            case PopupType.ProfileDetail:
            case PopupType.SettingInfo:
            case PopupType.LogOutWarning:
            case PopupType.EnterUserID:
            case PopupType.LifePurchase:
            case PopupType.UpgradeInfo:
            case PopupType.ItemInfo:
            case PopupType.DailyReward:
            case PopupType.RankingReward:
            case PopupType.BattlePassInfo:
            case PopupType.EquipInfo:
            case PopupType.CharacterInfo:
            case PopupType.CharacterSkillConfirm:
            case PopupType.BoxInfo:
                CloseTapScale(gameObject);
                break;
            default:
                Debug.Log("잘못된 PopupType 기입");
                return;
        }
        popupStack.Pop();
        
        //SFX 호출
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Close);
        
        if (popupStack.Count <= 0) {
            SetBackGroundPanelIndex(0);
            BackGroundPanel.SetActive(false);
            return;
        }

        PopupType temp = popupStack.Peek();
        SetBackGroundPanelIndex((int)temp);
        //Button button = GetExitPopupButton((int)temp);
        Button button = TapDictaionary[temp].closeButton;
        BackGroundPanel.GetComponent<Button>().onClick.RemoveAllListeners();
        BackGroundPanel.GetComponent<Button>().onClick.AddListener(() => button.onClick.Invoke());
    }
    
    void OpenTapScale(GameObject gameObject)
    {
        gameObject.SetActive(true);
        gameObject.transform.localScale = Vector3.zero;
        gameObject.transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack);
        BackGroundPanel.SetActive(true);
    }

    void OpenTapFade(GameObject gameObject)
    {
        gameObject.SetActive(true);
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.2f);
        BackGroundPanel.SetActive(true);
    }

    void CloseTapScale(GameObject gameObject)
    {
        Sequence sequence = DOTween.Sequence();
        gameObject.transform.localScale = Vector3.one;
        sequence.Append(gameObject.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack))
        .AppendCallback(() => gameObject.SetActive(false));
    }

    void CloseTapFade(GameObject gameObject)
    {
        Sequence sequence = DOTween.Sequence();
        CanvasGroup canvasGroup = gameObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1.0f;
        sequence.Append(canvasGroup.DOFade(0.0f, 0.2f))
        .AppendCallback(() => gameObject.SetActive(false));
    }
    
    // GameObject GetPopupGameObject(int tapType)
    // {
    //     switch ((PopupType)tapType)
    //     {
    //         case PopupType.GameModeInfo:
    //             return GameModeInfo;
    //         case PopupType.RankingStageInfo:
    //             return RankingStageInfo;
    //         case PopupType.StageInfo:
    //             return StageInfo;
    //         case PopupType.ProfileDetail:
    //             return ProfileDetail;
    //         case PopupType.SettingInfo:
    //             return SettingInfo;
    //         case PopupType.LogOutWarning:
    //             return LogOutWarning;
    //         case PopupType.EnterUserID:
    //             return EnterUserID;
    //         case PopupType.LifePurchase:
    //             return LifePurchase;
    //         case PopupType.UpgradeInfo:
    //             return UpgradeInfo;
    //         case PopupType.ItemInfo:
    //             return ItemInfo;
    //         case PopupType.DailyReward:
    //             return DailyReward;
    //         case PopupType.RankingReward:
    //             return RankingReward;
    //         case PopupType.BattlePassInfo:
    //             return BattlePassInfo;
    //         case PopupType.EquipInfo:
    //             return EquipInfo;
    //         case PopupType.CharacterInfo:
    //             return CharacterInfo;
    //         case PopupType.CharacterSkillConfirm:
    //             return CharacterSkillConfirm;
    //         default:
    //             Debug.Log("잘못된 PopupType 기입");
    //             return null;
    //     }
    // }
    //
    // Button GetExitPopupButton(int tapType)
    // {
    //     switch ((PopupType)tapType)
    //     {
    //         case PopupType.GameModeInfo:
    //             return Exit_GameModeInfo;
    //         case PopupType.RankingStageInfo:
    //             return Exit_RankingStageInfo;
    //         case PopupType.StageInfo:
    //             return Exit_StageInfo;
    //         case PopupType.ProfileDetail:
    //             return Exit_ProfileDetail;
    //         case PopupType.SettingInfo:
    //             return Exit_SettingInfo;
    //         case PopupType.LogOutWarning:
    //             return Exit_LogOutWarning[0]; // 똑같이 돌아서 일단 0번으로(임시)
    //         case PopupType.EnterUserID:
    //             return Exit_EnterUserID;
    //         case PopupType.LifePurchase:
    //             return Exit_LifePurchase;
    //         case PopupType.UpgradeInfo:
    //             return Exit_UpgradeInfo;
    //         case PopupType.ItemInfo:
    //             return Exit_ItemInfo;
    //         case PopupType.DailyReward:
    //             return Exit_DailyReward;
    //         case PopupType.RankingReward:
    //             return Exit_RankingReward;
    //         case PopupType.BattlePassInfo:
    //             return Exit_BattlePassInfo;
    //         case PopupType.EquipInfo:
    //             return Exit_EquipInfo;
    //         case PopupType.CharacterInfo:
    //             return Exit_CharacterInfo;
    //         case PopupType.CharacterSkillConfirm:
    //             return Exit_CharacterSkillConfirm;
    //         default:
    //             Debug.Log("잘못된 PopupType 기입");
    //             return null;
    //     }
    // }

    void SetBackGroundPanelIndex(int tapType)
    {
        //int gameObjectIndex = GetPopupGameObject(tapType).transform.GetSiblingIndex();
        int gameObjectIndex = TapDictaionary[(PopupType)tapType].transform.GetSiblingIndex();
        int backGroundPanelIndex = BackGroundPanel.transform.GetSiblingIndex();
        
        if(gameObjectIndex > backGroundPanelIndex)
            BackGroundPanel.transform.SetSiblingIndex(gameObjectIndex - 1);
        else
            BackGroundPanel.transform.SetSiblingIndex(gameObjectIndex);
    }
}