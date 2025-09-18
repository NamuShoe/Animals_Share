using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Serialization;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;
    private DataManager dataManager;
    
    private Dictionary<int, GameObject> itemFrames; //userData.equipmentSpecificList index, itemFrame
    [SerializeField] private Transform ItemContent; // 아이템 Content
    [SerializeField] private GameObject Equip_Frame;
    [SerializeField] private InventoryItemStructure[] UpgradeMaterials;
    [SerializeField] private InventoryItemStructure UpgradeResult;
    [SerializeField] private GameObject itemFramePrefab; // 가진 아이템에 대한 상호작용 버튼
    [SerializeField] private GameObject scrollArrow;
    [SerializeField] private GameObject scrollObj;
    [SerializeField] private Button upgradeBtn; // 업그레이드 버튼

    // [Header("ItemResource")]
    // [SerializeField] private Sprite emptySprite;
    // [SerializeField] private Sprite[] itemGradeSprites;
    // [SerializeField] private Sprite[] itemIndicatorSprites;
    // [SerializeField] private Color[] itemIndicatorColors;
    
    [Space(10f)]
    ScrollRect scrollRect;
    //public List<EquipmentSpecific> upgradeList;
    public EquipmentSpecific upgradeStandard;
    public Dictionary<int, int> materialId; // UpgradeMaterial index, itemFrame index
    public List<int> sameInfo; // 같은 정보 아이템
    private const int MAX_MATERIAL = 3;

    private void Awake()
    {
        Setting();
        instance = this;
        itemFrames = InventoryManager.instance.ItemFrames;
        materialId = new Dictionary<int, int>();
        sameInfo = new List<int>();
    }

    private void Start()
    {
        dataManager = DataManager.instance;
        UpgradeMaterials = Equip_Frame.GetComponentsInChildren<InventoryItemStructure>();
        Initialization();
    }

    private void Setting()
    {
        scrollRect = scrollObj.GetComponent<ScrollRect>(); // 스크롤 지정도 포함
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }
    }

    void OnScrollRectValueChanged(Vector2 normalizedPosition)
    {
        if (normalizedPosition.y <= 0) // 스크롤 끝까지 내렸을 때
        {
            scrollArrow.gameObject.SetActive(false);
        }
        else scrollArrow.gameObject.SetActive(true);
    }

    public void Initialization() // 초기화
    {
        for (int i = 0; i < MAX_MATERIAL; i++) {
            UpgradeMaterials[i].ClearEquipmentEquipSpec();
        }
        UpgradeResult.ClearEquipmentEquipSpec();

        foreach (var itemFrame in itemFrames) {
            itemFrame.Value.GetComponent<InventoryItemStructure>().ItemButton.interactable = true;
        }

        upgradeStandard = new EquipmentSpecific(-1, -1, -1, -1);
        materialId.Clear();
        sameInfo.Clear();
        upgradeBtn.gameObject.SetActive(false);
    }

    public void CheckItem(int index)
    {
        if (materialId.Count == 0) {
            TakeMaterial(index);
        }
        else {
            if (materialId.ContainsValue(index)) {
                ClearMaterial(index);
            }
            else {
                TakeMaterial(index);
            }
        }
    }

    private void TakeMaterial(int index) // 강화창에서 인덱스로 계산
    {
        var userData = dataManager.userData;
        EquipmentSpecific equipSpec = new EquipmentSpecific(userData.equipmentSpecificList[index]);

        itemFrames[index].GetComponent<InventoryItemStructure>().Activate = true;

        if (materialId.Count == 0)
        {
            UpgradeMaterials[0].TakeEquipmentEquipSpec(index, equipSpec);
            
            upgradeStandard = new EquipmentSpecific(equipSpec);
            materialId.Add(0, index); // 아이템 인덱스 추가
            
            //같은 아이템만 활성화
            foreach (var item in itemFrames)
            {
                if(equipSpec == userData.equipmentSpecificList[item.Key]) {
                    item.Value.GetComponent<InventoryItemStructure>().ItemButton.interactable = true;
                    sameInfo.Add(item.Key);
                }
                else item.Value.GetComponent<InventoryItemStructure>().ItemButton.interactable = false;
            }
        }
        else if (0 < materialId.Count && materialId.Count < MAX_MATERIAL)
        {
            if (upgradeStandard.id == equipSpec.id 
                && upgradeStandard.type == equipSpec.type
                && upgradeStandard.grade == equipSpec.grade)
            {
                int num = 0;
                for (int i = 0; i < MAX_MATERIAL; i++)
                {
                    if (materialId.ContainsKey(i) == false) {
                        num = i;
                        break;
                    }
                }
                UpgradeMaterials[num].TakeEquipmentEquipSpec(index, equipSpec);
                
                materialId.Add(num, index); // 아이템 인덱스 추가
            }
            else // 아니면 초기화
            {
                Debug.Log("같은 아이템을 올려야함, 초기화 예정");
            }
        }
        
        if (materialId.Count == MAX_MATERIAL)
        {
            upgradeBtn.gameObject.SetActive(true);
            for (int i = 0; i < materialId.Count; i++) // 가장 높은 레벨 뽑기
                if (userData.equipmentSpecificList[materialId[i]].level > equipSpec.level)
                    equipSpec.level = userData.equipmentSpecificList[materialId[i]].level;

            equipSpec.grade += 1;
            UpgradeResult.TakeEquipmentEquipSpec(-1, equipSpec);
            
            //모든 아이템 버튼 비활성화
            foreach (var itemFrame in itemFrames)
                itemFrame.Value.GetComponent<InventoryItemStructure>().ItemButton.interactable = false;
            
            // 들어가있는 아이템 버튼 활성화
            foreach (var id in materialId)
                itemFrames[id.Value].GetComponent<InventoryItemStructure>().ItemButton.interactable = true;
        }
        else if (materialId.Count != MAX_MATERIAL)
            upgradeBtn.gameObject.SetActive(false);
    }
    
    private void ClearMaterial(int index)
    {
        // Transform upgradeMaterial = UpgradeMaterials[index].transform;
        // upgradeMaterial.GetComponent<Button>().onClick.RemoveAllListeners();
        // SetEmptyFrame(upgradeMaterial);
        
        //UpgradeMaterial 중 index와 같은 material이 있으면 제거
        foreach (var upgradeMaterial in UpgradeMaterials) {
            if (upgradeMaterial.GetComponent<InventoryItemStructure>().index == index) {
                upgradeMaterial.GetComponent<InventoryItemStructure>().ClearEquipmentEquipSpec();
                break;
            }
        }

        itemFrames[index].GetComponent<InventoryItemStructure>().Activate = false;

        foreach (var id in materialId) {
            if (id.Value == index) {
                materialId.Remove(id.Key);
                break;
            }
        }
        // materialId.Remove(index);

        if (materialId.Count == 0) // 전부 빠진 후
        {
            materialId.Clear();
            sameInfo.Clear();
            
            foreach (var itemFrame in itemFrames) {
                itemFrame.Value.GetComponent<InventoryItemStructure>().ItemButton.interactable = true;
            }
        }
        else if (materialId.Count < MAX_MATERIAL) // 아이템 빠진 후 개수 확인
        {
            upgradeBtn.gameObject.SetActive(false); // 업그레이드 버튼 비활성화
            foreach (int info in sameInfo) {
                if (materialId.ContainsValue(info) == false) {
                    itemFrames[info].GetComponent<InventoryItemStructure>().ItemButton.interactable = true; // 빠진 버튼 다시 활성화
                }
            }
        }
        // SetEmptyFrame(UpgradeResult.transform);
        UpgradeResult.GetComponent<InventoryItemStructure>().ClearEquipmentEquipSpec();
    }

    public void UpgradeItem() // 강화 버튼 눌렀을 때 처리
    {
        var userData = dataManager.userData;
        
        SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Upgrade);
        Debug.Log("강화(등급 상승)!");

        int level = 0;
        for (int i = 0; i < materialId.Count; i++) // 가장 높은 레벨 뽑기
            if (userData.equipmentSpecificList[materialId[i]].level > level)
                level = userData.equipmentSpecificList[materialId[i]].level;

        //index 역순으로 제거
        materialId = materialId.OrderByDescending(item => item.Value).
            ToDictionary(x => x.Key, x => x.Value);
        
        //장비 해제
        foreach (var id in materialId)
            DismantleEquipment(id.Value);
        
        //재료 장비 삭제
        Dictionary<int, GameObject> tempItemFrames = new Dictionary<int, GameObject>();
        int count = 0;
        foreach (var itemFrame in itemFrames) {
            var key = itemFrame.Key;
            var value = itemFrame.Value;
            
            if (materialId.ContainsValue(itemFrame.Key)) {
                Destroy(itemFrame.Value);
                count++;
            }
            else {
                tempItemFrames.Add(key - count, value);
                value.GetComponent<InventoryItemStructure>().index = key - count;
            }
        }
        
        foreach (var id in materialId) {
            userData.equipmentSpecificList.RemoveAt(id.Value);
        }
        
        //itemFrames <= tempItemFrames로 복사
        itemFrames.Clear();
        foreach (var itemFrame in tempItemFrames) {
            itemFrames.Add(itemFrame.Key, itemFrame.Value);
        }
        
        //삭제 후처리
        RemovePostProcessing(materialId);
        
        //Upgrade된 아이템 생성
        userData.equipmentSpecificList.
            Add(new EquipmentSpecific(upgradeStandard.id, upgradeStandard.type, upgradeStandard.grade + 1, level)); // 등급 올라간 결과물
        InventoryManager.instance.CreateItemFrame(userData.equipmentSpecificList.Count - 1);
        
        if(upgradeStandard.type == 0) // 일일 미션 확인
            MissionManager.instance.MissionClearCheck(13);
        else
            MissionManager.instance.MissionClearCheck(14);
        
        Initialization();
        
        InventoryManager.instance.Sorting();
    }
    
    //업그레이드 재료들이 장착되어 있는 경우 해제
    private void DismantleEquipment(int id)
    {
        UserData userData = dataManager.userData;
        InventoryManager inventoryManager = InventoryManager.instance;

        if (userData.CurrentWeaponId == id)
        {
            dataManager.ChangeEquipments(EquipmentType.Weapon, 0);
            inventoryManager.ClearEquipment(id, EquipmentType.Weapon);
        }
        else if (userData.currentHatId == id)
        {
            dataManager.ChangeEquipments(EquipmentType.Hat, 0);
            inventoryManager.ClearEquipment(id, EquipmentType.Hat);
        }
        else if (userData.currentAccessoryId == id)
        {
            dataManager.ChangeEquipments(EquipmentType.Accessory, 0);
            inventoryManager.ClearEquipment(id, EquipmentType.Accessory);
        }
        else if (userData.currentShoesId == id)
        {
            dataManager.ChangeEquipments(EquipmentType.Shoes, 0);
            inventoryManager.ClearEquipment(id, EquipmentType.Shoes);
        }
        else if (userData.currentCoatId == id)
        {
            dataManager.ChangeEquipments(EquipmentType.Coat, 0);
            inventoryManager.ClearEquipment(id, EquipmentType.Coat);
        }
    }
    
    //아이템 제거 후 후처리
    private void RemovePostProcessing(Dictionary<int, int> _materialId)
    {
        UserData userData = dataManager.userData;

        foreach (var itemId in _materialId)
        {
            if (userData.CurrentWeaponId > itemId.Value) userData.CurrentWeaponId -= 1;
            if (userData.currentHatId > itemId.Value) userData.currentHatId -= 1;
            if (userData.currentAccessoryId > itemId.Value) userData.currentAccessoryId -= 1;
            if (userData.currentShoesId > itemId.Value) userData.currentShoesId -= 1;
            if (userData.currentCoatId > itemId.Value) userData.currentCoatId -= 1;
        }
    }
}