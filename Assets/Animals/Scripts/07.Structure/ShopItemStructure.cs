using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemStructure : MonoBehaviour {
    [HideInInspector] public ShopManager shopManager;
    private ShopData data;
    
    public Text ItemTypeText;
    public Image ItemImage;
    public Button ItemInfo;
    public GameObject ItemLayout;
    public Image[] ItemLayoutImages;
    public Text[] ItemLayoutAmountTexts;
    public GameObject ItemText;
    public Button ItemBuyButton;
    public Text ItemPriceText;
    public TextMeshProUGUI[] ItemPriceText1;
    public Image ItemPriceUnit;

    private void Reset()
    {
        var gb = FindObjectOfType<ShopManager>();
        shopManager = gb.GetComponent<ShopManager>();
    }

    public void SetShopItemStructure(ShopData shopData)
    {
        data = shopData;
        ItemTypeText.text = shopData.name;

        // int num = ConfirmPriceUnit(shopData);
        // var price = shopData.price[num] == 0 ? "" : shopData.price[num].ToString("N0");
        // ItemPriceText.text = price;
        //
        // if(ItemPriceUnit != null)
        //     ItemPriceUnit.sprite = GetPriceUnitImage(shopData.priceUnit[num]);
        RenewPriceUnit();

        if (GetShopImage(shopData.id) != null) {
            ItemImage.sprite = GetShopImage(shopData.id);
            ItemImage.SetNativeSize();
        }

        if (shopData is PackageData packageData) {

            if (shopData.description == null) {
                if(ItemLayout != null) 
                    ItemLayout.SetActive(true);
                if(ItemText != null)
                    ItemText.gameObject.SetActive(false);
                
                foreach (var t in ItemLayoutImages) {
                    t.gameObject.SetActive(false);
                }
            
                foreach (var t in ItemLayoutAmountTexts) {
                    t.gameObject.SetActive(false);
                }
            
                var num = Mathf.Min(ItemLayoutImages.Length, packageData.components.Count);
            
                for (int i = 0; i < num; i++) {
                    ItemLayoutImages[i].gameObject.SetActive(true);
                    ItemLayoutImages[i].sprite = GetShopItemImage(packageData.components[i].id);
                    ItemLayoutImages[i].GetComponentInChildren<Text>().text =
                        shopManager.GetShopItemTypeDescription(packageData.components[i].id);
                }
            
                num = Mathf.Min(ItemLayoutAmountTexts.Length, packageData.components.Count);

                for (int i = 0; i < num; i++) {
                    ItemLayoutAmountTexts[i].gameObject.SetActive(true);
                    ItemLayoutAmountTexts[i].text = "x" + packageData.components[i].quantity;
                }
            }
            else {
                if(ItemLayout != null) 
                    ItemLayout.SetActive(false);
                if (ItemText != null) {
                    ItemText.gameObject.SetActive(true);
                    foreach (var value in ItemText.GetComponentsInChildren<TextMeshProUGUI>()) {
                        value.text = shopData.description;
                    }
                }
            }

            if (ItemInfo != null) {
                ItemInfo.onClick.AddListener(() => {
                    shopManager.SetItemInfoItem(packageData);
                    PopupManager.instance.OpenTap((int)PopupType.ItemInfo);
                });
            }

            // if (ItemBuyButton != null) {
            //     ItemBuyButton.onClick.AddListener(() => {
            //         if (CanPay()) {
            //             StartCoroutine(ConfirmAds());
            //         }
            //     });
            // }
        }
        else if(shopData is BoxData boxData) {
            if (ItemInfo != null) {
                ItemInfo.onClick.AddListener(() => {
                    shopManager.SetBoxInfo(boxData);
                    PopupManager.instance.OpenTap((int)PopupType.BoxInfo);
                });
            }
        }
        
        
        
        if (ItemBuyButton != null) {
            ItemBuyButton.onClick.AddListener(() => {
                if (CanPay()) {
                    StartCoroutine(Buy(shopData));
                }
            });
        }
    }

    private IEnumerator Buy(ShopData shopData)
    {
        switch (shopData.priceUnit[GetPriceUnitNum(shopData)]) {
            case PriceUnit.Ads:
                var admobManager = AdmobManager.instance;
                admobManager.ShowRewardedAd();
                //yield return new WaitUntil(() => admobManager.onEarnedReward || admobManager.failedEarnedReward);
                //if (admobManager.failedEarnedReward) yield break;
                yield return new WaitUntil(() => admobManager.status != Status.Waiting);
                if (admobManager.status == Status.Fail) yield break;
                Pay();
                break;
            case PriceUnit.Won:
                var iapManager = IAPManager.instance;
                iapManager.InitiatePurchase(shopData.id.ToString().ToLower());
                yield return new WaitUntil(() => iapManager.status != Status.Waiting);
                if (iapManager.status == Status.Fail) yield break;
                Pay();
                break;
            case PriceUnit.NormalBox:
            case PriceUnit.MagicBox:
            case PriceUnit.Gold:
            case PriceUnit.Diamond:
                yield return null;
                Pay();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        List<KeyValuePair<Sprite, int>> gachaList = new List<KeyValuePair<Sprite, int>>();
        if (shopData is PackageData packageData) {
            
            foreach (var component in packageData.components) {
                gachaList.AddRange(shopManager.GetItem(component.id, component.quantity));
            }
            // SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Button);
            shopManager.RenewPriceUnit();
            yield return null;
        }
        else if (shopData is BoxData boxData) {
            gachaList.Add(shopManager.GetBoxItem(boxData));
            MissionManager.instance.MissionClearCheck(5);
            // SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Button);
            shopManager.RenewPriceUnit();
            yield return null;
        }
        shopManager.Gacha(gachaList);
    }

    private int GetPriceUnitNum(ShopData shopData)
    {
        var itemList = DataManager.instance.userData.itemList;
        for (int i = 0; i < shopData.priceUnit.Count; i++) {
            switch (shopData.priceUnit[i]) {
                case PriceUnit.Ads:
                    string shopDataId = shopData.id.ToString().Replace("C_Goods_", "");
                    var itemType = (ItemType)Enum.Parse(typeof(ItemType), "Ads" + shopDataId);
                    if (itemList.Exists(x => x.itemType == itemType && x.amount > 0))
                        return i;
                    continue;
                case PriceUnit.NormalBox:
                    if (itemList.Exists(x => x.itemType == ItemType.NormalBox && x.amount > 0))
                        return i;
                    continue;
                case PriceUnit.MagicBox:
                    if (itemList.Exists(x => x.itemType == ItemType.MagicBox && x.amount > 0))
                        return i;
                    continue;
                case PriceUnit.Gold:
                case PriceUnit.Diamond:
                case PriceUnit.Won:
                    return i;
                default:
                    return 0;
                    throw new ArgumentOutOfRangeException();
            }
        }
        return 0;
    }
    
    public void RenewPriceUnit()
    {
        int num = GetPriceUnitNum(data);

        string price = "";
        if (data.priceUnit[num] == PriceUnit.Ads) {
            string dataId = data.id.ToString().Replace("C_Goods_", "");
            var itemType = (ItemType)Enum.Parse(typeof(ItemType), "Ads" + dataId);
            if(DataManager.instance.userData.itemList.Exists(x => x.itemType == itemType)) {
                price += DataManager.instance.userData.itemList.Find(x => x.itemType == itemType).amount;
            }
            else {
                price += 0;
            }
            price += "/" + data.price[num];
        }
        else if (data.priceUnit[num] == PriceUnit.NormalBox || data.priceUnit[num] == PriceUnit.MagicBox) {
            price = "";
        }
        else {
            price = data.price[num].ToString("N0");
        }
        ItemPriceText.text = price;
        if (ItemPriceText1.Length > 0) {
            foreach (var value in ItemPriceText1) {
                value.text = price;
            }
        }
        
        if(ItemPriceUnit != null)
            ItemPriceUnit.sprite = GetPriceUnitImage(data.priceUnit[num]);
    }

    private bool CanPay()
    {
        var itemList = DataManager.instance.userData.itemList;
        int num = GetPriceUnitNum(data);
        
        switch (data.priceUnit[num]) {
            case PriceUnit.Ads:
                string dataId = data.id.ToString().Replace("C_Goods_", "");
                var itemType = (ItemType)Enum.Parse(typeof(ItemType), "Ads" + dataId);
                if (itemList.Exists(x => x.itemType == itemType && x.amount > 0)) {
                    return true;
                }
                break;
            case PriceUnit.NormalBox:
                if (itemList.Exists(x => x.itemType == ItemType.NormalBox &&
                                         x.amount >= data.price[num])) {
                    return true;
                }
                break;
            case PriceUnit.MagicBox:
                if (itemList.Exists(x => x.itemType == ItemType.MagicBox &&
                                         x.amount >= data.price[num])) {
                    return true;
                }
                break;
            case PriceUnit.Gold:
                if (DataManager.instance.userData.Gold >= data.price[num]) {
                    GoodsManager.instance.UiRefresh();
                    return true;
                }
                break;
            case PriceUnit.Diamond:
                if (DataManager.instance.userData.Diamond >= data.price[num]) {
                    GoodsManager.instance.UiRefresh();
                    return true;
                }
                break;
            case PriceUnit.Won:
                return true;
                //break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }
    
    private bool Pay()
    {
        var itemList = DataManager.instance.userData.itemList;
        int num = GetPriceUnitNum(data);
        
        switch (data.priceUnit[num]) {
            case PriceUnit.Ads:
                string dataId = data.id.ToString().Replace("C_Goods_", "");
                var itemType = (ItemType)Enum.Parse(typeof(ItemType), "Ads" + dataId);
                if (itemList.Exists(x => x.itemType == itemType && x.amount > 0)) {
                    itemList.Find(x => x.itemType == itemType).amount -= 1;
                    return true;
                }
                break;
            case PriceUnit.NormalBox:
                if (itemList.Exists(x => x.itemType == ItemType.NormalBox &&
                                         x.amount >= data.price[num])) {
                    itemList.Find(x => x.itemType == ItemType.NormalBox).amount -= 1;
                    return true;
                }
                break;
            case PriceUnit.MagicBox:
                if (itemList.Exists(x => x.itemType == ItemType.MagicBox &&
                                         x.amount >= data.price[num])) {
                    itemList.Find(x => x.itemType == ItemType.MagicBox).amount -= 1;
                    return true;
                }
                break;
            case PriceUnit.Gold:
                if (DataManager.instance.userData.Gold >= data.price[num]) {
                    DataManager.instance.userData.Gold -= data.price[num];
                    GoodsManager.instance.UiRefresh();
                    return true;
                }
                break;
            case PriceUnit.Diamond:
                if (DataManager.instance.userData.Diamond >= data.price[num]) {
                    DataManager.instance.userData.Diamond -= data.price[num];
                    GoodsManager.instance.UiRefresh();
                    return true;
                }
                break;
            case PriceUnit.Won:
                //IAP함수 구현???
                //테스트를 위해 true 반환
                return true;
                //break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }
    
    private static Sprite GetShopImage(ShopType shopType)
    {
        //None이면 기존에 이미지 사용
        if (shopType == ShopType.None)
            return null;
        return Resources.Load<Sprite>("Shop/" + shopType);
    }

    private static Sprite GetShopItemImage(ShopItemType shopItemType)
    {
        return Resources.Load<Sprite>("Shop/ShopItem/" + shopItemType);
    }
    
    private static Sprite GetPriceUnitImage(PriceUnit priceUnit)
    {
        return Resources.Load<Sprite>("Shop/Unit/" + priceUnit);
    }
}
