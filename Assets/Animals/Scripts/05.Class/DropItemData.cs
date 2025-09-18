using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public enum DropType
{
    All,
    Equipment,
    Material
}

public enum DropItemType {
    EquipmentAll,
    MaterialAll,
    
    Arrow,
    Dart,
    Boomerang,
    Crown,
    Hat,
    Necklace,
    PearlNecklace,
    Sneakers,
    Slipper,
    Shirt,
    Cape,
    
    Tree,
    Iron,
    Cotton,
    MagicPowder,
    Emerald
}

[Serializable]
public abstract class DropItem {
    public DropItemType id;// = (DropItemType)Enum.Parse(typeof(ItemType), name);
    public string name;
    public Sprite sprite;
    public float probability;

    public abstract void AddItem();
}

[Serializable]
public class EquipmentItem : DropItem
{
    public EquipmentSpecific specific;

    public override void AddItem()
    {
        DataManager.instance.AddEquipment(specific.id, specific.type, specific.grade);
    }
}

[Serializable]
public class MaterialItem : DropItem
{
    public int type;

    public override void AddItem()
    {
        DataManager.instance.AddMaterial(type);
    }
}

[CreateAssetMenu(fileName = "DropItemSO", menuName = "Scriptable Object/ItemDropData", order = int.MaxValue)]
public class DropItemData : ScriptableObject
{
    [SerializeField] private List<EquipmentItem> DropEquipmentList;
    [SerializeField] private List<MaterialItem> DropMaterialList;
    private List<DropItem> DropItemList = new List<DropItem>();

    private float EquipmentProbability {
        get {
            return DropEquipmentList.Sum(dropEquipment => dropEquipment.probability);
        }
    }

    private float MaterialProbability {
        get {
            return DropMaterialList.Sum(dropEquipment => dropEquipment.probability);
        }
    }

    private float AllProbability => EquipmentProbability + MaterialProbability;

    private void OnEnable()
    {
        DropItemList.AddRange(DropEquipmentList);
        DropItemList.AddRange(DropMaterialList);
    }

    public DropItem GetItem(DropType dropType, bool isGuarantee = false)
    {
        switch (dropType)
        {
            case DropType.All:
                return GetAllItem(isGuarantee);
            case DropType.Equipment:
                return GetEquipmentItem(isGuarantee);
            case DropType.Material:
                return GetMaterialItem(isGuarantee);
            default:
                throw new ArgumentOutOfRangeException(nameof(dropType), dropType, null);
        }
    }

    private DropItem GetEquipmentItem(bool isGuarantee = false)
    {
        if (isGuarantee == false && Random.Range(0f, 100f) > EquipmentProbability)
                return null;
        
        float randomValue = Random.Range(0f, EquipmentProbability);
        float cumulativeProbability = 0; // 누계 확률

        foreach (var equipmentItem in DropEquipmentList)
        {
            cumulativeProbability += equipmentItem.probability;
            if (randomValue <= cumulativeProbability)
            {
                equipmentItem.AddItem();
                return equipmentItem;
            }
        }
        return null;
    }

    private DropItem GetMaterialItem(bool isGuarantee = false)
    {
        if (isGuarantee == false && Random.Range(0f, 100f) > MaterialProbability)
            return null;

        float randomValue = Random.Range(0f, MaterialProbability);
        float cumulativeProbability = 0; // 누계 확률
        
        foreach (var materialItem in DropMaterialList)
        {
            cumulativeProbability += materialItem.probability;
            if (randomValue <= cumulativeProbability)
            {
                materialItem.AddItem();
                return materialItem;
            }
        }
        return null;
    }

    private DropItem GetAllItem(bool isGuarantee = false)
    {
        if (isGuarantee == false && Random.Range(0f, 100f) > AllProbability)
            return null;
        
        List<DropItem> dropItemList = new List<DropItem>();
        dropItemList.AddRange(DropEquipmentList);
        dropItemList.AddRange(DropMaterialList);
        float dropTotalProbability = AllProbability;
        
        float randomValue = Random.Range(0f, dropTotalProbability);
        float cumulativeProbability = 0; // 누계 확률

        foreach (var dropItem in dropItemList)
        {
            cumulativeProbability += dropItem.probability;
            if (randomValue <= cumulativeProbability)
            {
                dropItem.AddItem();
                return dropItem;
            }
        }
        return null;
    }

    public void SetProbability(DropItemType dropItemType, float rate)
    {
        switch (dropItemType) {
            case DropItemType.EquipmentAll:
                foreach (var dropEquipment in DropEquipmentList) {
                    dropEquipment.probability *= rate;
                }
                break;
            case DropItemType.MaterialAll:
                foreach (var dropMaterial in DropMaterialList) {
                    dropMaterial.probability *= rate;
                }
                break;
            case DropItemType.Arrow:
            case DropItemType.Dart:
            case DropItemType.Boomerang:
            case DropItemType.Crown:
            case DropItemType.Hat:
            case DropItemType.Necklace:
            case DropItemType.PearlNecklace:
            case DropItemType.Sneakers:
            case DropItemType.Slipper:
            case DropItemType.Shirt:
            case DropItemType.Cape:
                // DropItemList.Find(x => x.id == dropItemType).probability *= rate;
                // break;
            case DropItemType.Tree:
            case DropItemType.Iron:
            case DropItemType.Cotton:
            case DropItemType.MagicPowder:
            case DropItemType.Emerald:
                DropItemList.Find(x => x.id == dropItemType).probability *= rate;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dropItemType), dropItemType, null);
        }
    }
}
