using System;
using System.Collections.Generic;
using Newtonsoft.Json;    
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

public enum ShopItemType
{
    Gold, // 골드
    Diamond, // 통조림
    NormalBox, // 일반 뽑기
    MagicBox, // 고급 뽑기
    EquipLevelUpMaterial, // 장비 레벨업 재료
    Tree,
    Iron,
    Cotton,
    MagicPowder,
    Emerald,
    ResurrectionTicket, // 인게임 부활권
    CommonEquipmentTicket,
    UncommonEquipmentTicket,
    RareEquipmentTicket,
    EpicEquipmentTicket,
    CharacterNero, // 지급 캐릭터 id
}

[Serializable]
public class ItemComponent
{
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public ShopItemType id;
    public int quantity; // 수량 또는 번호
}

public enum PriceUnit
{
    Ads,
    NormalBox,
    MagicBox,
    Gold,
    Diamond,
    Won
}

public enum ShopType
{
    None = 0,
    C_Package_Starter_0,
    C_Package_StepUp_0,
    C_Package_Challenge_0,
    C_Package_StepUp_1,
    C_Character_Nero_0,
    C_Character_Nero_1,
    
    NC_NoAds_NoAds_0,
    S_Subscription_Membership_0,
    S_Subscription_Membership_1,
    
    
    C_Goods_SmallGold,
    C_Goods_MediumGold,
    C_Goods_LargeGold,
    C_Goods_SmallDiamond,
    C_Goods_MediumDiamond,
    C_Goods_LargeDiamond,
    SmallLife,
    MediumLife,
    LargeLife,
    
    C_Goods_Tree,
    C_Goods_Iron,
    C_Goods_Cotton,
    C_Goods_MagicPowder,
    C_Goods_Emerald,
    C_Goods_EquipLevelUpMaterial,
    
    
    C_Gacha_NormalBox,
    C_Gacha_MagicBox
}

[Serializable]
public class ShopData {
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public ShopType id;
    public string name;
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<PriceUnit> priceUnit;
    public List<int> price;
    public string description;
}

[Serializable]
public class PackageData : ShopData {
    public int row;
    public int purchaseCount;
    public List<ItemComponent> components; // 받는 아이템 정보 포함
}

public enum BoxType
{
    None,
    NormalBox,
    MagicBox
}
[Serializable]
public class BoxData : ShopData {
    public List<float> drawChances; // 확률
    public List<List<int>> components; // 아이템 정보 포함
}
