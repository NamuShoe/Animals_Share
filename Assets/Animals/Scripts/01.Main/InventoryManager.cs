using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

public class InventoryManager : MonoBehaviour {
    public static InventoryManager instance;
    private DataManager dataManager;

    private Dictionary<int, GameObject> itemFrames = new Dictionary<int, GameObject>();
    public Dictionary<int, GameObject> ItemFrames => itemFrames;

    [Space(10f)] [SerializeField] private GameObject itemFramePrefab; // 가진 아이템에 대한 상호작용 버튼
    [SerializeField] private Transform weaponContent; // 무기(아이템) Content
    [SerializeField] private Transform equipmentsLayout; // 장착 장비 레이아웃

    [Header("ItemResource")] public Sprite itemEmptySprite;
    public Sprite[] itemGradeSprites; // 비어있는 칸, 일반, 고급, 희귀, 에픽
    public Sprite[] itemIndicatorSprites;
    public Color[] itemIndicatorColors;

    [Header("Upgrade")] [SerializeField] private GameObject inventoryFrame;
    [SerializeField] private GameObject upgradeFrame;

    [Space(10f)] [SerializeField] private Button inventoryTurnButton;
    [SerializeField] private Image FadeImage;
    public bool isInventory = true;
    [SerializeField] private Button SortButton;
    private SORT_TYPE sortType = default;

    private enum SORT_TYPE {
        Recent,
        Type,
        Grade,
        Level,
    }

    [Header("ItemLevelMaterialImage")] [SerializeField]
    private Sprite[] itemLevelMaterials; // 목재, 강철, 목화, 마법의 가루, 보석

    [Header("UserSpec")] // 무기 + 캐릭터
    [SerializeField]
    private Text userAttack; // 합산 공격력

    [SerializeField] private Text userAttackSpeed; // 합산 공격속도
    [SerializeField] private Text userHp; // 이동속도
    [SerializeField] private Text userAttack_Ranking; // 합산 공격력
    [SerializeField] private Text userAttackSpeed_Ranking; // 합산 공격속도
    [SerializeField] private Text userMoveSpeed_Ranking; // 이동속도

    [Header("EquipPopUp")] [SerializeField]
    private GameObject equipPopUp;

    [SerializeField] private InventoryItemStructure popUpItemStructure;
    [SerializeField] private Text equipmentNameText;

    /// <summary>
    /// child 0Icon 1Option 2Amount
    /// </summary>
    [SerializeField] private List<Transform> equipmentSpecs;

    /// <summary>
    /// 0공격력 1공격속도 2이동속도 3체력 4치명타
    /// </summary>
    [SerializeField] private List<Sprite> equipmentOptionIcon;

    [SerializeField] private Text itemExplainText;
    [SerializeField] private Text itemStatText;
    [SerializeField] private Text itemStatAmountText;
    [SerializeField] private Transform materialGold;
    [SerializeField] private List<Transform> materials = new List<Transform>();
    [SerializeField] private Button equipButton;
    [SerializeField] private Button dismantleButton;
    [SerializeField] private Button levelUpButton;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        dataManager = DataManager.instance;

        sortType = (SORT_TYPE)PlayerPrefs.GetInt("SortType", 0);

        inventoryFrame.SetActive(true);
        upgradeFrame.SetActive(false);
        inventoryTurnButton.onClick.AddListener(InventoryTurnButtonOnClick);
        SortButton.onClick.AddListener(SortButtonOnClick);
        SortButton.transform.GetChild(0).GetComponent<Text>().text = GetSortTypeKr();
    }

    private void InventoryTurnButtonOnClick()
    {
        isInventory = !isInventory;

        FadeImage.DOFade(1f, 0.1f);
        DOVirtual.DelayedCall(0.15f, () => {
            FadeImage.DOFade(0f, 0.1f);

            inventoryFrame.SetActive(isInventory);
            upgradeFrame.SetActive(!isInventory);

            UpgradeManager.instance.Initialization();
            foreach (var itemFrame in itemFrames) {
                itemFrame.Value.GetComponent<InventoryItemStructure>().Activate = false;
            }

            var userData = dataManager.userData;
            if (isInventory) {
                //인벤토리에서 해야할 작업
                if (userData.CurrentWeaponId != -1)
                    itemFrames[userData.CurrentWeaponId].GetComponent<InventoryItemStructure>().Activate = true;
                if (userData.currentHatId != -1)
                    itemFrames[userData.currentHatId].GetComponent<InventoryItemStructure>().Activate = true;
                if (userData.currentAccessoryId != -1)
                    itemFrames[userData.currentAccessoryId].GetComponent<InventoryItemStructure>().Activate = true;
                if (userData.currentShoesId != -1)
                    itemFrames[userData.currentShoesId].GetComponent<InventoryItemStructure>().Activate = true;
                if (userData.currentCoatId != -1)
                    itemFrames[userData.currentCoatId].GetComponent<InventoryItemStructure>().Activate = true;

                //업그레이드에서 해야할 작업
                foreach (var itemFrame in itemFrames)
                    itemFrame.Value.GetComponent<Button>().interactable = true;

                inventoryTurnButton.transform.GetChild(0).GetComponent<Text>().text = "합성";
            }
            else {
                inventoryTurnButton.transform.GetChild(0).GetComponent<Text>().text = "인벤토리";
            }
        });
    }

    private void SortButtonOnClick()
    {
        // int sortTypeNum = (int)sortType;
        // sortType = (SORT_TYPE)(sortTypeNum + 1);
        if (sortType == SORT_TYPE.Level)
            sortType = SORT_TYPE.Recent;
        else {
            sortType += 1;
        }

        PlayerPrefs.SetInt("SortType", (int)sortType);

        SortButton.transform.GetChild(0).GetComponent<Text>().text = GetSortTypeKr();

        Sorting();
    }

    private string GetSortTypeKr()
    {
        switch (sortType) {
            case SORT_TYPE.Recent:
                return "최근 순";
            case SORT_TYPE.Type:
                return "타입 순";
            case SORT_TYPE.Grade:
                return "등급 순";
            case SORT_TYPE.Level:
                return "레벨 순";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void EquipPopUpSetting(int index)
    {
        var userData = dataManager.userData;

        if (index < 0) return;

        equipButton.onClick.RemoveAllListeners();
        dismantleButton.onClick.RemoveAllListeners();
        levelUpButton.onClick.RemoveAllListeners();

        foreach (var spec in equipmentSpecs)
            spec.gameObject.SetActive(false);

        Item selectedItem = null;
        EquipmentSpecific equipSpec = userData.equipmentSpecificList[index];

        switch ((EquipmentType)equipSpec.type) {
            case EquipmentType.Weapon: // 무기
                selectedItem = ItemManager.instance.weapons.Find(w => w.id == userData.equipmentSpecificList[index].id);

                // 팝업 스탯 추가
                equipButton.gameObject.SetActive(index != userData.CurrentWeaponId);
                dismantleButton.gameObject.SetActive(index == userData.CurrentWeaponId);
                break;
            case EquipmentType.Hat: // 모자
                selectedItem = ItemManager.instance.hats.Find(w => w.id == userData.equipmentSpecificList[index].id);

                // 팝업 스탯 추가
                equipButton.gameObject.SetActive(index != userData.currentHatId);
                dismantleButton.gameObject.SetActive(index == userData.currentHatId);
                break;
            case EquipmentType.Accessory: // 악세서리
                selectedItem =
                    ItemManager.instance.accessories.Find(w => w.id == userData.equipmentSpecificList[index].id);

                // 팝업 스탯 추가
                equipButton.gameObject.SetActive(index != userData.currentAccessoryId);
                dismantleButton.gameObject.SetActive(index == userData.currentAccessoryId);
                break;
            case EquipmentType.Shoes: // 신발
                selectedItem = ItemManager.instance.shoes.Find(w => w.id == userData.equipmentSpecificList[index].id);

                // 팝업 스탯 추가
                equipButton.gameObject.SetActive(index != userData.currentShoesId);
                dismantleButton.gameObject.SetActive(index == userData.currentShoesId);
                break;
            case EquipmentType.Coat: // 코트
                selectedItem = ItemManager.instance.coats.Find(w => w.id == userData.equipmentSpecificList[index].id);

                // 팝업 스탯 추가
                equipButton.gameObject.SetActive(index != userData.currentCoatId);
                dismantleButton.gameObject.SetActive(index == userData.currentCoatId);
                break;
        }

        if (selectedItem != null) {
            for (int i = 0; i < selectedItem.stat.Count; i++) {
                equipmentSpecs[i].gameObject.SetActive(true);
                equipmentSpecs[i].GetChild(0).GetComponent<Image>().sprite =
                    GetEquipmentOptionIcon(selectedItem.stat[i]);
                equipmentSpecs[i].GetChild(1).GetComponent<Text>().text = GetEquipmentOptionKr(selectedItem.stat[i]);
                equipmentSpecs[i].GetChild(2).GetComponent<Text>().text =
                    GetEquipmentOptionStat(selectedItem.stat[i], selectedItem, equipSpec).ToString();
            }

            SetPopupItemInfo(selectedItem, index);
        }

        SetupPopupButtons(index, (EquipmentType)equipSpec.type);
    }

    private void SetPopupItemInfo(Item item, int index)
    {
        equipmentNameText.text = item.name;
        itemExplainText.text = item.itemDescription;

        popUpItemStructure.SetEquipSpec(dataManager.userData.equipmentSpecificList[index], index);

        SetItemTotalDescription(item, index);

        var itemLevelData =
            ItemLevelManager.instance.ReadItemLevelData(dataManager.userData.equipmentSpecificList[index].level);
        materialGold.GetChild(1).GetComponent<Text>().text = itemLevelData.requiredGold.ToString();

        SetIfLevelUp(index, item.materialTypes);
    }

    private Sprite GetEquipmentOptionIcon(EquipmentOption equipmentOption)
    {
        switch (equipmentOption) {
            case EquipmentOption.AttackPower:
                return equipmentOptionIcon[0];
            case EquipmentOption.AttackSpeed:
                return equipmentOptionIcon[1];
            case EquipmentOption.MoveSpeed:
                return equipmentOptionIcon[2];
            case EquipmentOption.Hp:
                return equipmentOptionIcon[3];
            case EquipmentOption.CriticalPercent:
            case EquipmentOption.CriticalMultipleMin:
            case EquipmentOption.CriticalMultipleMax:
                return equipmentOptionIcon[4];
            default:
                return null;
        }
    }

    private void SetItemTotalDescription(Item item, int index)
    {
        string itemStatString = "";
        string itemStatAmountString = "";

        // stat
        // for (int i = 0; i < item.stat.Count; i++) {
        //     itemStatString += GetEquipmentOptionKr(item.stat[i]);
        //     itemStatString += "\n";
        //     if (item.statAmount[i] > 0) // -는 자동으로 붙음
        //         itemStatAmountString += "+";
        //     itemStatAmountString += item.statAmount[i] + GetEquipmentOptionUnit(item.stat[i]);
        //     itemStatAmountString += "\n";
        // }

        // additionalStat
        if (item.additionalStat != null) {
            for (int i = 0; i < item.additionalStat.Count; i++) {
                itemStatString += GetEquipmentOptionKr(item.additionalStat[i]);
                itemStatString += "\n";
                if (item.additionalStatAmount[i] > 0) // -는 자동으로 붙음
                    itemStatAmountString += "+";
                itemStatAmountString += item.additionalStatAmount[i] + GetEquipmentOptionUnit(item.additionalStat[i]);
                itemStatAmountString += "\n";
            }
        }

        if (dataManager.userData.equipmentSpecificList[index].grade >= 3) {
            itemStatAmountString += "\n";
            itemStatString += item.epicEffectDescription;
        }

        itemStatText.text = itemStatString;
        itemStatAmountText.text = itemStatAmountString;
    }

    private string GetEquipmentOptionKr(EquipmentOption equipmentOption)
    {
        switch (equipmentOption) {
            case EquipmentOption.AttackPower:
                return "공격력";
            case EquipmentOption.AttackSpeed:
                return "공격속도";
            case EquipmentOption.MoveSpeed:
                return "이동속도";
            case EquipmentOption.Hp:
                return "체력";
            case EquipmentOption.CriticalPercent:
                return "치명타 확률";
            case EquipmentOption.CriticalMultipleMin:
                return "치명타 최소 피해";
            case EquipmentOption.CriticalMultipleMax:
                return "치명타 최대 피해";
            case EquipmentOption.AvoidPercent:
                return "회피률";
            case EquipmentOption.Piercing:
                return "관통력";
            case EquipmentOption.ProjectileSpeed:
                return "투사체 속도";
            default:
                Debug.Log("존재하지 않는 장비 스탯입니다.");
                return "뭐임";
        }
    }

    private string GetEquipmentOptionUnit(EquipmentOption equipmentOption)
    {
        switch (equipmentOption) {
            case EquipmentOption.AttackPower:
            case EquipmentOption.AttackSpeed:
            case EquipmentOption.MoveSpeed:
            case EquipmentOption.Hp:
            case EquipmentOption.Piercing:
            case EquipmentOption.ProjectileSpeed:
                return "";
            case EquipmentOption.CriticalPercent:
            case EquipmentOption.CriticalMultipleMin:
            case EquipmentOption.CriticalMultipleMax:
            case EquipmentOption.AvoidPercent:
                return "%";
            default:
                Debug.Log("존재하지 않는 장비 스탯입니다.(단위)");
                return "뭐임";
        }
    }

    private void SetupPopupButtons(int index, EquipmentType itemType)
    {
        equipButton.onClick.AddListener(() => {
            DataManager.instance.ChangeEquipments(itemType, index);
            itemFrames[index].GetComponent<InventoryItemStructure>().Activate = true;
            TakeEquipment(index, itemType);
        });
        dismantleButton.onClick.AddListener(() => {
            DataManager.instance.ChangeEquipments(itemType, 0);
            itemFrames[index].GetComponent<InventoryItemStructure>().Activate = false;
            ClearEquipment(index, itemType);
        });
        levelUpButton.onClick.AddListener(() => {
            ItemLevelUp(index, itemType);
            RefreshEquipPopup(index, itemType);
            DataManager.instance.ChangeBasicStats();
        });
    }

    private void RefreshEquipPopup(int index, EquipmentType itemType)
    {
        var userData = dataManager.userData;

        popUpItemStructure.SetEquipSpec(userData.equipmentSpecificList[index], index);

        // 스탯 text 수정
        Item selectedItem = null;
        switch (itemType) {
            case EquipmentType.Weapon: // 무기
                selectedItem = ItemManager.instance.weapons.Find(w => w.id == userData.equipmentSpecificList[index].id);
                break;
            case EquipmentType.Hat: // 모자
                selectedItem = ItemManager.instance.hats.Find(w => w.id == userData.equipmentSpecificList[index].id);
                break;
            case EquipmentType.Accessory: // 악세서리
                selectedItem =
                    ItemManager.instance.accessories.Find(w => w.id == userData.equipmentSpecificList[index].id);
                break;
            case EquipmentType.Shoes: // 신발
                selectedItem = ItemManager.instance.shoes.Find(w => w.id == userData.equipmentSpecificList[index].id);
                break;
            case EquipmentType.Coat: // 코트
                selectedItem = ItemManager.instance.coats.Find(w => w.id == userData.equipmentSpecificList[index].id);
                break;
        }
        
        EquipmentSpecific equipSpec = userData.equipmentSpecificList[index];;
        
        if (selectedItem != null) {
            for (int i = 0; i < selectedItem.stat.Count; i++) {
                equipmentSpecs[i].gameObject.SetActive(true);
                equipmentSpecs[i].GetChild(0).GetComponent<Image>().sprite =
                    GetEquipmentOptionIcon(selectedItem.stat[i]);
                equipmentSpecs[i].GetChild(1).GetComponent<Text>().text = GetEquipmentOptionKr(selectedItem.stat[i]);
                equipmentSpecs[i].GetChild(2).GetComponent<Text>().text =
                    GetEquipmentOptionStat(selectedItem.stat[i], selectedItem, equipSpec).ToString();
            }
        }
    }

    private float GetEquipmentOptionStat(EquipmentOption equipOption, Item selectedItem, EquipmentSpecific equipSpec)
    {
        int index = selectedItem.stat.FindIndex(x => x == equipOption);

        return selectedItem.statAmount[index] + selectedItem.statCft[index] * (equipSpec.level - 1);
    }

    private void SetIfLevelUp(int index, List<int> materialTypes)
    {
        var userData = dataManager.userData;
        var itemLevelData = ItemLevelManager.instance.ReadItemLevelData(userData.equipmentSpecificList[index].level);

        materialGold.GetChild(1).GetComponent<Text>().text = itemLevelData.requiredGold.ToString();

        foreach (var material in materials)
            material.gameObject.SetActive(false);

        for (int i = 0; i < materialTypes.Count; i++) {
            materials[i].GetChild(0).GetComponent<Image>().sprite = itemLevelMaterials[materialTypes[i]];
            materials[i].GetChild(0).GetComponentInChildren<Text>().text = GetMaterialDescription(materialTypes[i]);
            materials[i].GetChild(1).GetComponent<Text>().text =
                userData.levelUpMaterial[materialTypes[i]] + " / " + itemLevelData.requiredMaterials[i];
            materials[i].gameObject.SetActive(true);
        }

        // 레벨업 가능 여부
        bool isCanLevelUp = true;
        if (userData.Gold >= itemLevelData.requiredGold) {
            for (int i = 0; i < materialTypes.Count; i++) {
                if (userData.levelUpMaterial[materialTypes[i]] < itemLevelData.requiredMaterials[i]) {
                    isCanLevelUp = false;
                    break;
                }
            }
        }
        else
            isCanLevelUp = false;

        levelUpButton.interactable = isCanLevelUp;
    }

    //json 생성 전 임시적으로 사용
    private static string GetMaterialDescription(int materialType)
    {
        return materialType switch {
            //목재
            0 => "무기, 모자에 사용되는 강화재료",
            //강철
            1 => "무기에 사용되는 강화재료",
            //목화
            2 => "상의, 신발에 사용되는 강화재료",
            //마법의 가루
            3 => "모자에 사용되는 강화재료",
            //보석
            4 => "목걸이에 사용되는 강화재료",
            _ => ""
        };
    }

    public void TakeEquipment(int equipId, EquipmentType itemType)
    {
        // 장착 아이템 칸 번호, 아이템 타입
        var userData = dataManager.userData;
        int itemTypeNum = (int)itemType;

        if (equipId != -1) // 미장착이 아니라면
        {
            var itemStructure = equipmentsLayout.GetChild(itemTypeNum).GetComponent<InventoryItemStructure>();
            itemStructure.TakeEquipmentEquipSpec(equipId, userData.equipmentSpecificList[equipId]);
        }
    }

    public void ClearEquipment(int equipId, EquipmentType itemType) // 해제
    {
        var userData = dataManager.userData;
        var itemTypeNum = (int)itemType;

        var itemStructure = equipmentsLayout.GetChild(itemTypeNum).GetComponent<InventoryItemStructure>();
        itemStructure.ClearEquipmentEquipSpec();
        if (equipId < 0) return;

        itemFrames[equipId].GetComponent<InventoryItemStructure>().Activate = false;

        //차후 Current시리즈 배열로 수정(전체 공사)
        switch (itemType) {
            case EquipmentType.Weapon:
                userData.CurrentWeaponId = -1;
                break;
            case EquipmentType.Hat:
                userData.currentHatId = -1;
                break;
            case EquipmentType.Accessory:
                userData.currentAccessoryId = -1;
                break;
            case EquipmentType.Shoes:
                userData.currentShoesId = -1;
                break;
            case EquipmentType.Coat:
                userData.currentCoatId = -1;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }

        DataManager.instance.ChangeBasicStats();
        EquipPopUpSetting(equipId); // 해당 번호 버튼 재설정
    }

    /// <summary>
    /// 오직 UpgradeManager에서 사용
    /// </summary>
    // public void RemoveAllListenersInEquipmentsLayout()
    // {
    //     foreach (Transform child in equipmentsLayout)
    //         child.GetComponent<Button>().onClick.RemoveAllListeners();
    // }
    private void ItemLevelUp(int index, EquipmentType itemType) // 아이템 레벨 강화
    {
        var userData = dataManager.userData;

        var itemLevelData =
            ItemLevelManager.instance.ReadItemLevelData(userData.equipmentSpecificList[index]
                .level); // 선택된 장비의 레벨 번호로 세팅
        // 배정 인덱스 번호 : 목재0, 강철1, 목화2, 마법의 가루3, 보석4
        List<int> materialsIndex;

        switch (itemType) {
            case EquipmentType.Weapon:
                // 무기, 장비별 일일 미션 확인
                materialsIndex = Weapon.Materials;
                MissionManager.instance.MissionClearCheck(11);
                break;
            case EquipmentType.Hat:
                // 모자
                materialsIndex = Hat.Materials;
                MissionManager.instance.MissionClearCheck(12);
                break;
            case EquipmentType.Accessory:
                // 악세서리
                materialsIndex = Accessory.Materials;
                MissionManager.instance.MissionClearCheck(12);
                break;
            case EquipmentType.Shoes:
                // 신발
                materialsIndex = Shoes.Materials;
                MissionManager.instance.MissionClearCheck(12);
                break;
            case EquipmentType.Coat:
                // 상의
                materialsIndex = Coat.Materials;
                MissionManager.instance.MissionClearCheck(12);
                break;
            default:
                materialsIndex = new List<int> { };
                break;
        }

        for (int i = 0; i < materialsIndex.Count; i++) {
            userData.levelUpMaterial[materialsIndex[i]] -= itemLevelData.requiredMaterials[i];
        }

        userData.Gold -= itemLevelData.requiredGold;
        userData.equipmentSpecificList[index].level++;
        //itemFrames[index].transform.Find("Item_Level").GetComponent<Text>().text = "Lv. " + userData.equipmentSpecificList[index].level;
        itemFrames[index].GetComponent<InventoryItemStructure>()
            .SetEquipSpec(userData.equipmentSpecificList[index], index);

        if (IsCurrentEquipment(index))
            equipmentsLayout.GetChild((int)itemType).GetComponent<InventoryItemStructure>()
                .TakeEquipmentEquipSpec(index, userData.equipmentSpecificList[index]);

        SetIfLevelUp(index, materialsIndex);
        Sorting();
    }

    private void SetupEquipment(int itemId, int layoutIndex)
    {
        var itemStructure = equipmentsLayout.GetChild(layoutIndex).GetComponent<InventoryItemStructure>();

        if (itemId < 0 || dataManager.userData.equipmentSpecificList.Count <= itemId) {
            itemStructure.ClearEquipmentEquipSpec();
        }
        else {
            itemStructure.TakeEquipmentEquipSpec(itemId, dataManager.userData.equipmentSpecificList[itemId]);
        }
    }

    /// <summary>
    /// 인벤토리 아이템 생성 시 호출
    /// </summary>
    private void SetupItemButton(InventoryItemStructure itemStructure, int index)
    {
        itemStructure.Activate = IsCurrentEquipment(index);
        itemStructure.SetEquipSpec(dataManager.userData.equipmentSpecificList[index], index);
    }

    private bool IsCurrentEquipment(int index)
    {
        var userData = dataManager.userData;

        return index == userData.CurrentWeaponId ||
               index == userData.currentHatId ||
               index == userData.currentAccessoryId ||
               index == userData.currentShoesId ||
               index == userData.currentCoatId;
    }

    public void UserStatsRefresh() // 현재 스탯에 맞게 갱신
    {
        userAttack.text = DataManager.instance.playerStat.AttackPower.ToString();
        userAttackSpeed.text = DataManager.instance.playerStat.AttackSpeed.ToString();
        userHp.text = DataManager.instance.playerStat.Hp.ToString();

        userAttack_Ranking.text = DataManager.instance.playerStat.AttackPower.ToString();
        userAttackSpeed_Ranking.text = DataManager.instance.playerStat.AttackSpeed.ToString();
        userMoveSpeed_Ranking.text = DataManager.instance.playerStat.MoveSpeed.ToString();
    }

    IEnumerator ReadSavedWeapons() // 있는 무기(아이템들) 불러오기
    {
        var userData = dataManager.userData;
        foreach (Transform child in weaponContent)
            Destroy(child.gameObject);

        // 무기, 모자, 악세서리, 신발, 상의 정보를 불러오는 부분
        SetupEquipment(userData.CurrentWeaponId, (int)EquipmentType.Weapon);
        SetupEquipment(userData.currentHatId, (int)EquipmentType.Hat);
        SetupEquipment(userData.currentAccessoryId, (int)EquipmentType.Accessory);
        SetupEquipment(userData.currentShoesId, (int)EquipmentType.Shoes);
        SetupEquipment(userData.currentCoatId, (int)EquipmentType.Coat);

        yield return null;

        itemFrames.Clear();
        for (int i = 0; i < userData.equipmentSpecificList.Count; i++) {
            GameObject buttonInstance = Instantiate(itemFramePrefab, weaponContent);
            SetupItemButton(buttonInstance.GetComponent<InventoryItemStructure>(), i);
            itemFrames.Add(i, buttonInstance);
        }

        Sorting();
        dataManager.SaveUserData(); //임시
    }

    public void CreateItemFrame(int index)
    {
        GameObject buttonInstance = Instantiate(itemFramePrefab, weaponContent);
        SetupItemButton(buttonInstance.GetComponent<InventoryItemStructure>(), index);
        if (itemFrames.ContainsKey(index)) Debug.LogError("inventory contain " + index);
        itemFrames.Add(index, buttonInstance);
    }

    public void Sorting()
    {
        Dictionary<int, EquipmentSpecific> sortDictionary = new Dictionary<int, EquipmentSpecific>();
        for (int i = 0; i < dataManager.userData.equipmentSpecificList.Count; i++)
            sortDictionary.Add(i, dataManager.userData.equipmentSpecificList[i]);

        Dictionary<int, EquipmentSpecific> sortedDictionary;
        switch (sortType) {
            case SORT_TYPE.Recent:
                sortedDictionary = sortDictionary
                    .OrderByDescending(e => e.Key) // key 내림차순
                    .ToDictionary(e => e.Key, e => e.Value);
                break;
            case SORT_TYPE.Type:
                sortedDictionary = sortDictionary
                    .OrderBy(e => e.Value.type) // type 오름차순
                    .ThenBy(e => e.Value.id) // id 오름차순
                    .ThenByDescending(e => e.Value.grade) // grade 내림차순
                    .ThenBy(e => e.Value.level) // level 오름차순)
                    .ToDictionary(e => e.Key, e => e.Value);
                break;
            case SORT_TYPE.Grade:
                sortedDictionary = sortDictionary
                    .OrderByDescending(e => e.Value.grade) // grade 내림차순
                    .ThenBy(e => e.Value.type) // type 오름차순
                    .ThenBy(e => e.Value.id) // id 오름차순
                    .ThenBy(e => e.Value.level) // level 오름차순)
                    .ToDictionary(e => e.Key, e => e.Value);
                break;
            case SORT_TYPE.Level:
                sortedDictionary = sortDictionary
                    .OrderByDescending(e => e.Value.level)
                    .ThenBy(e => e.Value.type) // type 오름차순
                    .ThenBy(e => e.Value.id) // id 오름차순
                    .ThenByDescending(e => e.Value.grade)
                    .ToDictionary(e => e.Key, e => e.Value); // grade 내림차순
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(sortType), sortType, null);
        }

        foreach (var dicKey in sortedDictionary.Keys) {
            itemFrames[dicKey].transform.SetAsLastSibling();
        }
    }

    public void InventoryOpen()
    {
        // 캐릭터
        StartCoroutine(ReadSavedWeapons());
        //EquipButtonSetting(dataManager.userData.CurrentWeaponId, 0); // 장착 아이템 반영
        DataManager.instance.ChangeBasicStats();
    }
}