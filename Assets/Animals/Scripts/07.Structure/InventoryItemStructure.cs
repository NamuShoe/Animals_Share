using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemStructure : MonoBehaviour {
    [SerializeField] public int index;
    [SerializeField] private EquipmentSpecific equipSpec;

    public bool Activate { set {
        if (!Equipment) CheckBox.SetActive(value);
        //isActivate = value;
    }}
    //private bool isActivate;

    public bool isEmpty => equipSpec.id == -1 && equipSpec.type == -1 && equipSpec.grade == -1 && equipSpec.level == -1;

    [Header("ItemStructureType")] 
    [SerializeField] private bool Equipment = false;
    
    [Header("Components")]
    [HideInInspector] public Button ItemButton;
    
    [SerializeField] private Image ItemFrame;
    [SerializeField] private Image ItemImage;
    [SerializeField] private GameObject CheckBox;
    [SerializeField] private Text ItemLevel;
    [SerializeField] private Image ItemIndicator;

    private void Awake()
    {
        if (CheckBox != null && Equipment)
            CheckBox.SetActive(false);

        equipSpec = new EquipmentSpecific(-1, -1, -1, -1);
    }

    private void Reset()
    {
        ItemButton = GetComponent<Button>();

        ItemFrame = GetComponent<Image>();
        ItemImage = transform.Find("Item_Img").GetComponent<Image>();
        CheckBox = transform.Find("CheckBox").gameObject;
        ItemLevel = transform.Find("Item_Level").GetComponent<Text>();
        ItemIndicator = transform.Find("Item_Ind").GetComponent<Image>();
    }

    public void SetEquipSpec(EquipmentSpecific _equipSpec, int _index = -1)
    {
        bool isEpic = false;
        index = _index;

        //grade
        ItemFrame.sprite = InventoryManager.instance.itemGradeSprites[_equipSpec.grade];
        ItemIndicator.color = InventoryManager.instance.itemIndicatorColors[_equipSpec.grade];
        if (_equipSpec.grade >= 3) {
            isEpic = true;
        }

        //level
        ItemLevel.text = "Lv." + _equipSpec.level;

        //id, type      Eqic은 grade를 확인해야하기 때문에 마지막에 확인
        var imagePath = "WeaponThumbnail/" + GetItemTypePrefix((EquipmentType)_equipSpec.type) +
                        _equipSpec.id.ToString("D3");
        if (isEpic) imagePath += "_Epic";
        ItemImage.sprite = Resources.Load<Sprite>(imagePath);

        //type
        ItemIndicator.sprite = InventoryManager.instance.itemIndicatorSprites[_equipSpec.type];
        
        if (ItemButton != null && 0 <= index) {
            ItemButton.onClick.RemoveAllListeners();
            ItemButton.onClick.AddListener(() => {
                if (InventoryManager.instance.isInventory) { // 인벤토리
                    InventoryManager.instance.EquipPopUpSetting(index);
                    PopupManager.instance.OpenTap((int)PopupType.EquipInfo);
                }
                else {
                    UpgradeManager.instance.CheckItem(index);
                }
            });
        }

        equipSpec = _equipSpec;
    }
    
    /// <summary>
    /// Equipment가 true인 InventoryItemStructure에서만 호출하기
    /// </summary>
    /// <param name="_index"></param>
    /// <param name="_equipSpec"></param>
    public void TakeEquipmentEquipSpec(int _index, EquipmentSpecific _equipSpec)
    {
        ItemImage.gameObject.SetActive(true);
        ItemLevel.gameObject.SetActive(true);
        ItemIndicator.gameObject.SetActive(true);
        SetEquipSpec(_equipSpec, _index);
    }

    /// <summary>
    /// Equipment가 true인 InventoryItemStructure에서만 호출하기
    /// </summary>
    public void ClearEquipmentEquipSpec()
    {
        if(ItemButton != null)
            ItemButton.onClick.RemoveAllListeners();
        ItemFrame.sprite = InventoryManager.instance.itemEmptySprite;
        ItemImage.gameObject.SetActive(false);
        ItemLevel.gameObject.SetActive(false);
        ItemIndicator.gameObject.SetActive(false);
        index = -1;
        equipSpec = new EquipmentSpecific(-1, -1, -1, -1);
    }

    private static char GetItemTypePrefix(EquipmentType equipType)
    {
        return equipType switch {
            EquipmentType.Weapon => 'w',
            EquipmentType.Hat => 'h',
            EquipmentType.Accessory => 'a',
            EquipmentType.Shoes => 's',
            EquipmentType.Coat => 'c',
            _ => throw new ArgumentOutOfRangeException(nameof(equipType), equipType, null)
        };
    }
}
