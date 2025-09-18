using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

public enum EquipmentOption
{
    AttackPower,
    AttackSpeed,
    MoveSpeed,
    Hp,
    CriticalPercent,
    CriticalMultipleMin,
    CriticalMultipleMax,
    AvoidPercent,
    Piercing,
    ProjectileSpeed
}

public enum EquipmentType
{
    Weapon,
    Hat,
    Accessory,
    Shoes,
    Coat
}

[Serializable]
public class Item
{
    public EquipmentType equipmentType; // 아이템 종류
    public int id; // 아이템 고유 ID
    public string name; // 아이템 이름
    public int level; // 아이템 레벨
    public int grade; // 아이템 등급
    public string itemDescription; // 아이템 설명

    //능력치
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<EquipmentOption> stat; // 능력치
    public List<float> statAmount; // 능력치 양
    public List<float> statCft; // 능력치 계수

    //숨겨진 능력
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<EquipmentOption> additionalStat; // 숨겨진 능력
    public List<float> additionalStatAmount; // 숨겨진 능력 매개변수
    
    //에픽 효과
    public string epicEffect;
    public float epicParameter;
    public string epicEffectDescription;

    public List<int> materialTypes;
}

[Serializable]
public class Weapon : Item
{
    public int weaponCode;
    public static List<int> Materials => new List<int> { 0, 1 };

    // 무기 생성자
    public Weapon()
    {
        materialTypes = new List<int> { 0, 1 };
    }
}

[Serializable]
public class Hat : Item // 모자
{
    public float skillCount;
    public static List<int> Materials => new List<int> { 0, 3 };

    public Hat()
    {
        materialTypes = new List<int> { 0, 3 };
    }
}

[Serializable]
public class Coat : Item // 상의
{
    public static List<int> Materials => new List<int> { 2 };

    public Coat()
    {
        materialTypes = new List<int> { 2 };
    }
}

[Serializable]
public class Accessory : Item // 악세서리
{
    public static List<int> Materials => new List<int> { 4 };

    public Accessory()
    {
        materialTypes = new List<int> { 4 };
    }
}

[Serializable]
public class Shoes : Item // 신발
{
    public static List<int> Materials => new List<int> { 2 };

    public Shoes()
    {
        materialTypes = new List<int> { 2 };
    }
}