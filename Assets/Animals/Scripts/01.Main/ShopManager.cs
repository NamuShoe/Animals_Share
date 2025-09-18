using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<PackageData> packageDatas;
    [SerializeField] private List<BoxData> boxDatas;
    [SerializeField] private List<PackageData> goodsDatas;

    [Space(10f)]
    [SerializeField] private List<GameObject> itemInfoItemList;
    [SerializeField] private TextMeshProUGUI equipmentName;
    [SerializeField] private TextMeshProUGUI equipmentDrawChance;
    [SerializeField] private List<GameObject> gachaInfoItemList;
    [SerializeField] private Button gachaButton;

    [Space(10f)]
    [SerializeField] private Transform shopContent;
    [SerializeField] private ScrollRect shopScrollRect;
    [SerializeField] private GameObject shopItemPrefab;
    [Space(10f)]
    [SerializeField] private GameObject shopItemContainerPrefab;
    [SerializeField] private GameObject shopItemInContainerPrefab;
    [Space(10f)]
    [SerializeField] private GameObject boxesItemContent;
    [SerializeField] private List<GameObject> goodsItemContent;

    [SerializeField] private List<ShopItemStructure> shopItemStructures;

    private void Awake()
    {
        LoadShopData();
    }

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        SetPackageData();
        SetBoxesData();
        SetGoodsData();
    }

    public void RenewPriceUnit()
    {
        foreach (var shopItemStructure in shopItemStructures) {
            shopItemStructure.RenewPriceUnit();
        }
    }
    
    private void SetPackageData()
    {
        var groupedPackages = packageDatas.GroupBy(p => p.row)
            .ToDictionary(g => g.Key, g => g.ToList());

        int i = 1;
        foreach (var package in groupedPackages)
        {
            GameObject packageGameObject;
            if(package.Value.Count <= 1)
                packageGameObject = SetPackage(package.Value[0]);
            else
                packageGameObject = SetPackages(package.Value);
            
            packageGameObject.transform.SetSiblingIndex(i);
            i++;
        }
    }
    private GameObject SetPackage(PackageData packageData)
    {
        //별도로 할 때
        var shopItem = Instantiate(shopItemPrefab, shopContent).GetComponent<ShopItemStructure>();
        shopItem.shopManager = this;
        shopItem.SetShopItemStructure(packageData);
        shopItemStructures.Add(shopItem);

        return shopItem.gameObject;
    }
    private GameObject SetPackages(List<PackageData> packageDatas)
    {
        //패키지 묶을 때
        var shopItemContainer = Instantiate(shopItemContainerPrefab, shopContent);
        shopItemContainer.GetComponent<ScrollController>().parentScrollRect = shopScrollRect;
        var shopItemContainerScrollRect = shopItemContainer.GetComponent<ScrollRect>().content;
        foreach (var package in packageDatas)
        {
            var shopItem = Instantiate(shopItemInContainerPrefab, shopItemContainerScrollRect).GetComponent<ShopItemStructure>();
            shopItem.shopManager = this;
            shopItem.SetShopItemStructure(package);
            shopItemStructures.Add(shopItem);
        }
        
        return shopItemContainer.gameObject;
    }

    private void SetBoxesData()
    {
        var boxesItemStructures = boxesItemContent.GetComponentsInChildren<ShopItemStructure>();
        for (int i = 0; i < boxesItemStructures.Length; i++) {
            boxesItemStructures[i].shopManager = this;
            boxesItemStructures[i].SetShopItemStructure(boxDatas[i]);
            shopItemStructures.Add(boxesItemStructures[i]);
        }
    }
    
    private void SetGoodsData()
    {
        var groupedGoods = goodsDatas.GroupBy(p => p.row)
            .ToDictionary(g => g.Key, g => g.ToList());

        int i = 0;
        foreach (var goods in groupedGoods) 
        {
            var goodsItemStructures = goodsItemContent[i].GetComponentsInChildren<ShopItemStructure>();
            for (int j = 0; j < goodsItemStructures.Length; j++) {
                goodsItemStructures[j].shopManager = this;
                goodsItemStructures[j].SetShopItemStructure(goods.Value[j]);
                shopItemStructures.Add(goodsItemStructures[j]);
            }
            i++;
        }
    }
    
    public void SetItemInfoItem(PackageData packageData)
    {
        foreach (var itemInfoItem in itemInfoItemList)
        {
            itemInfoItem.transform.Find("Item").gameObject.SetActive(false);
            itemInfoItem.transform.Find("ItemAmount").gameObject.SetActive(false);
        }
        
        for (int i = 0; i < packageData.components.Count; i++)
        {
            var itemInfoItem = itemInfoItemList[i].transform;
            
            var itemInfoItemImage = itemInfoItem.Find("Item").GetComponent<Image>();
            itemInfoItemImage.gameObject.SetActive(true);
            itemInfoItemImage.sprite = GetShopItemImage(packageData.components[i].id);
            itemInfoItemImage.GetComponentInChildren<Text>().text =
                GetShopItemTypeDescription(packageData.components[i].id);
            
            var itemInfoItemAmount = itemInfoItem.Find("ItemAmount").GetComponent<Text>();
            itemInfoItemAmount.gameObject.SetActive(true);
            itemInfoItemAmount.text = "x" + packageData.components[i].quantity;
        }
    }

    public void SetBoxInfo(BoxData boxData)
    {
        var items = ItemManager.instance.items;
        var totalDrawChance = boxData.drawChances.Sum();
        string[] colors = { "white", "green", "blue", "purple" };
        
        equipmentName.text = "";
        equipmentDrawChance.text = "";
        
        for (int i = 0; i < boxData.drawChances.Count; i++) {
            equipmentName.text += "<color=" + colors[boxData.components[i][2]] + ">";
            equipmentName.text += items.Find(x =>
                x.id == boxData.components[i][0] && (int)x.equipmentType == boxData.components[i][1]).name + "\n";
            equipmentName.text += "</color>";

            equipmentDrawChance.text += ((boxData.drawChances[i] / totalDrawChance) * 100f).ToString("n2") + "%\n";
        }
    }

    private static Sprite GetShopItemImage(ShopItemType shopItemType)
    {
        return Resources.Load<Sprite>("Shop/ShopItem/" + shopItemType);
    }
    
    public List<KeyValuePair<Sprite, int>> GetItem(ShopItemType shopItemType, int quantity)
    {
        List<KeyValuePair<Sprite, int>> returnSpriteList = new List<KeyValuePair<Sprite, int>>();
        var userData = DataManager.instance.userData;
        var itemList = userData.itemList;
        
        switch (shopItemType)
        {
            case ShopItemType.Gold:
                userData.Gold += quantity;
                break;
            case ShopItemType.Diamond:
                userData.Diamond += quantity;
                break;
            case ShopItemType.NormalBox:
                if (itemList.Exists(x => x.itemType == ItemType.NormalBox) == false) {
                    itemList.Add(new ConsumeItem(ItemType.NormalBox, quantity));
                }
                else {
                    itemList.Find(x => x.itemType == ItemType.NormalBox).amount += quantity;
                }
                break;
            case ShopItemType.MagicBox:
                if (itemList.Exists(x => x.itemType == ItemType.MagicBox) == false) {
                    itemList.Add(new ConsumeItem(ItemType.MagicBox, quantity));
                }
                else {
                    itemList.Find(x => x.itemType == ItemType.MagicBox).amount += quantity;
                }
                break;
            case ShopItemType.EquipLevelUpMaterial:
                var levelUpMaterial = userData.levelUpMaterial;
                for (int i = 0; i < levelUpMaterial.Count; i++)
                    levelUpMaterial[i] += quantity;
                break;
            case ShopItemType.Tree:
                userData.levelUpMaterial[0] += quantity;
                break;
            case ShopItemType.Iron:
                userData.levelUpMaterial[1] += quantity;
                break;
            case ShopItemType.Cotton:
                userData.levelUpMaterial[2] += quantity;
                break;
            case ShopItemType.MagicPowder:
                userData.levelUpMaterial[3] += quantity;
                break;
            case ShopItemType.Emerald:
                userData.levelUpMaterial[4] += quantity;
                break;
            case ShopItemType.ResurrectionTicket:
                if (itemList.Exists(x => x.itemType == ItemType.ResurrectionTicket) == false) {
                    itemList.Add(new ConsumeItem(ItemType.ResurrectionTicket, quantity));
                }
                else {
                    itemList.Find(x => x.itemType == ItemType.ResurrectionTicket).amount += quantity;
                }
                break;
            case ShopItemType.CommonEquipmentTicket:
                for (int i = 0; i < quantity; i++)
                    returnSpriteList.Add(new KeyValuePair<Sprite, int>(GetEquipmentItemByGrade(0), 0));
                return returnSpriteList;
            case ShopItemType.UncommonEquipmentTicket:
                for (int i = 0; i < quantity; i++)
                    returnSpriteList.Add(new KeyValuePair<Sprite, int>(GetEquipmentItemByGrade(1), 1));
                return returnSpriteList;
            case ShopItemType.RareEquipmentTicket:
                for (int i = 0; i < quantity; i++)
                    returnSpriteList.Add(new KeyValuePair<Sprite, int>(GetEquipmentItemByGrade(2), 2));
                return returnSpriteList;
            case ShopItemType.EpicEquipmentTicket:
                for (int i = 0; i < quantity; i++)
                    returnSpriteList.Add(new KeyValuePair<Sprite, int>(GetEquipmentItemByGrade(3), 3));
                return returnSpriteList;
            case ShopItemType.CharacterNero:
                PediaManager.instance.GetCharacter(3);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(shopItemType), shopItemType, null);
        }

        returnSpriteList.Add(new KeyValuePair<Sprite, int>(GetShopItemImage(shopItemType), 0));
        return returnSpriteList;
    }

    public string GetShopItemTypeDescription(ShopItemType shopItemType)
    {
        return shopItemType switch {
            ShopItemType.Gold => "골드",
            ShopItemType.Diamond => "통조림",
            ShopItemType.NormalBox => "일반 뽑기권",
            ShopItemType.MagicBox => "고급 뽑기권",
            ShopItemType.EquipLevelUpMaterial => "장비에 사용되는 강화재료들",
            ShopItemType.Tree => "무기, 모자에 사용되는 강화재료",
            ShopItemType.Iron => "무기에 사용되는 강화재료",
            ShopItemType.Cotton => "상의, 신발에 사용되는 강화재료",
            ShopItemType.MagicPowder => "모자에 사용되는 강화재료",
            ShopItemType.Emerald => "목걸이에 사용되는 강화재료",
            ShopItemType.ResurrectionTicket => "세션에서 사망 시, 부활 할 수 있는 티켓",
            ShopItemType.CharacterNero => "캐릭터 네로를 획득",
            ShopItemType.CommonEquipmentTicket => "일반 등급 장비를 얻을 수 있는 티켓",
            ShopItemType.UncommonEquipmentTicket => "매직 등급 장비를 얻을 수 있는 티켓",
            ShopItemType.RareEquipmentTicket => "고급 등급 장비를 얻을 수 있는 티켓",
            ShopItemType.EpicEquipmentTicket => "에픽 등급 장비를 얻을 수 있는 티켓",
            _ => throw new ArgumentOutOfRangeException(nameof(shopItemType), shopItemType, null)
        };
    }

    public KeyValuePair<Sprite, int> GetBoxItem(BoxData boxData)
    {
        List<float> probabilities = new List<float>();
        Sprite returnSprite = null;
        int grade = 0;
        
        foreach (var drawChance in boxData.drawChances)
            probabilities.Add(drawChance);

        float totalProbability = probabilities.Sum();

        var randomValue = UnityEngine.Random.Range(0f, totalProbability);
        var cumulativeProbability = 0f;
        for (int i = 0; i < boxData.drawChances.Count; i++) {
            cumulativeProbability += probabilities[i];
            if (randomValue <= cumulativeProbability) {
                //itemType, itemId, itemGrade
                DataManager.instance.AddEquipment(boxData.components[i][0], boxData.components[i][1],
                    boxData.components[i][2]);
                returnSprite = GetEquipmentSprite(new EquipmentSpecific(boxData.components[i][0],
                    boxData.components[i][1],
                    boxData.components[i][2]));
                grade = boxData.components[i][2];
                break;
            }
        }

        InventoryManager.instance.Sorting();
        return new KeyValuePair<Sprite, int>(returnSprite,grade);
    }

    /// <summary>
    /// 1:Common 2:Uncommon 3:Rare 4:Epic
    /// </summary>
    private Sprite GetEquipmentItemByGrade(int grade)
    {
        var temp = boxDatas[1].components.Where(c => c.Count == 4 && c[2] == grade).ToList();

        var randomValue = UnityEngine.Random.Range(0, temp.Count);
        DataManager.instance.AddEquipment(temp[randomValue][0], temp[randomValue][1], temp[randomValue][2]);

        return GetEquipmentSprite(new EquipmentSpecific(temp[randomValue][0], temp[randomValue][1], temp[randomValue][2]));
    }

    private Sprite GetEquipmentSprite(EquipmentSpecific equipmentSpecific)
    {
        var typePrefix = equipmentSpecific.type switch {
            0 => 'w',
            1 => 'h',
            2 => 'a',
            3 => 's',
            4 => 'c',
            _ => default
        };

        var imagePath = "WeaponThumbnail/" + typePrefix +
                        equipmentSpecific.id.ToString("D3");
        if (equipmentSpecific.grade > 3) imagePath += "_Epic";
        
        return Resources.Load<Sprite>(imagePath);
    }

    public void Gacha(List<KeyValuePair<Sprite, int>> gachaList)
    {
        foreach (var gachaInfo in gachaInfoItemList) {
            gachaInfo.gameObject.SetActive(false);
        }

        for (int i = 0; i < gachaList.Count; i++) {
            gachaInfoItemList[i].gameObject.SetActive(true);
            gachaInfoItemList[i].transform.localScale = Vector3.left + Vector3.one; // 0, 1, 1
            gachaInfoItemList[i].GetComponent<CanvasGroup>().alpha = 0f;
            gachaInfoItemList[i].transform.GetChild(0).GetComponent<Image>().sprite = gachaList[i].Key;
            gachaInfoItemList[i].GetComponent<Image>().sprite =
                InventoryManager.instance.itemGradeSprites[gachaList[i].Value];
        }
        
        gachaButton.interactable = false;
        PopupManager.instance.OpenTap((int)PopupType.GachaInfo);

        for (int i = 0; i < gachaList.Count; i++) {
            var num = i;
            DOVirtual.DelayedCall(i * 0.1f, () => {
                gachaInfoItemList[num].GetComponent<CanvasGroup>().DOFade(1f, 0.2f);
                gachaInfoItemList[num].transform.DOScaleX(1f, 0.2f);
            });
        }

        DOVirtual.DelayedCall(gachaList.Count * 0.1f, () => {
            gachaButton.interactable = true;
        });
    }
    
    private void LoadShopData()
    {
        // PackageData.json 파일 로드
        TextAsset packageDataJson = Resources.Load<TextAsset>("GameData/PackageData");
        if (packageDataJson != null)
        {
            packageDatas = JsonConvert.DeserializeObject<List<PackageData>>(packageDataJson.text);
        }
        else
        {
            Debug.LogError("PackageData.json 파일을 찾을 수 없습니다.");
        }
        
        // GoodsData.json 파일 로드
        TextAsset goodsDataJson = Resources.Load<TextAsset>("GameData/GoodsData");
        if (goodsDataJson != null)
        {
            goodsDatas = JsonConvert.DeserializeObject<List<PackageData>>(goodsDataJson.text);
        }
        else
        {
            Debug.LogError("GoodsData.json 파일을 찾을 수 없습니다.");
        }
        
        // BoxData.json 파일 로드
        TextAsset boxDataJson = Resources.Load<TextAsset>("GameData/BoxData");
        if (boxDataJson != null)
        {
            boxDatas = JsonConvert.DeserializeObject<List<BoxData>>(boxDataJson.text);
        }
        else
        {
            Debug.LogError("BoxData.json 파일을 찾을 수 없습니다.");
        }
    }
}
