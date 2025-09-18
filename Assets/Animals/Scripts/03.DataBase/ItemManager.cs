using System;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

public class ItemManager : MonoBehaviour
{
    public static ItemManager instance;
    public List<Item> items;
    public List<Weapon> weapons;
    public List<Hat> hats;
    public List<Accessory> accessories;
    public List<Shoes> shoes;
    public List<Coat> coats;
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        LoadItemData();
    }

    private void LoadItemData()
    {
        string jsonContent;
        
        // 무기 데이터 로드
        TextAsset weaponData = FileConnecter.GetEquipmentDataAsset(EquipmentType.Weapon);
        if (weaponData != null)
        {
            jsonContent = weaponData.text;
            List<Weapon> weaponList = JsonConvert.DeserializeObject<List<Weapon>>(jsonContent);
            items.AddRange(weaponList);
            weapons = weaponList;
        }
        
        // 모자 데이터 로드
        TextAsset hatData = FileConnecter.GetEquipmentDataAsset(EquipmentType.Hat);
        if (hatData != null)
        {
            jsonContent = hatData.text;
            List<Hat> hatList = JsonConvert.DeserializeObject<List<Hat>>(jsonContent);
            items.AddRange(hatList);
            hats = hatList;
        }
        
        // 악세서리 데이터 로드
        TextAsset accessoryData = FileConnecter.GetEquipmentDataAsset(EquipmentType.Accessory);
        if (accessoryData != null)
        {
            jsonContent = accessoryData.text;
            List<Accessory> accessoryList = JsonConvert.DeserializeObject<List<Accessory>>(jsonContent);
            items.AddRange(accessoryList);
            accessories = accessoryList;
        }
        
        // 신발 데이터 로드
        TextAsset shoesData = FileConnecter.GetEquipmentDataAsset(EquipmentType.Shoes);
        if (shoesData != null)
        {
            jsonContent = shoesData.text;
            List<Shoes> shoesList = JsonConvert.DeserializeObject<List<Shoes>>(jsonContent);
            items.AddRange(shoesList);
            shoes = shoesList;
        }
        
        // 코트 데이터 로드
        TextAsset coatData = FileConnecter.GetEquipmentDataAsset(EquipmentType.Coat);
        if (coatData != null)
        {
            jsonContent = coatData.text;
            List<Coat> coatList = JsonConvert.DeserializeObject<List<Coat>>(jsonContent);
            items.AddRange(coatList);
            coats = coatList;
        }
    }
    public void SelectWeapon(int index)
    {
        Weapon selectedWeapon;
        if(index >= DataManager.instance.userData.equipmentSpecificList.Count) {
            selectedWeapon = weapons.Find(w => w.id == 0); // 0번은 미장착
        }
        else {
            Debug.Log("무기 번호 : " + DataManager.instance.userData.equipmentSpecificList[index].id);
            selectedWeapon = weapons.Find(w => w.id == DataManager.instance.userData.equipmentSpecificList[index].id);
        }
    }
    public void SetStats(EquipmentType type)
    {
        var userData = DataManager.instance.userData;
        Item selectedItem;
        EquipmentSpecific equipSpec;
        switch (type)
        {
            case EquipmentType.Weapon:
                selectedItem = weapons.Find(w => w.id == userData.equipmentSpecificList[userData.CurrentWeaponId].id);
                equipSpec = userData.equipmentSpecificList[userData.CurrentWeaponId];
                Weapon selectedWeapon = (Weapon)selectedItem;
                for (int i = 0; i < selectedWeapon.additionalStat.Count; i++)
                    SetSecretStat(selectedWeapon.additionalStat[i], selectedWeapon.additionalStatAmount[i]);
                break;
            case EquipmentType.Hat:
                selectedItem = hats.Find(h => h.id == userData.equipmentSpecificList[userData.currentHatId].id);
                equipSpec = userData.equipmentSpecificList[userData.currentHatId];
                break;
            case EquipmentType.Accessory:
                selectedItem = accessories.Find(a => a.id == userData.equipmentSpecificList[userData.currentAccessoryId].id);
                equipSpec = userData.equipmentSpecificList[userData.currentAccessoryId];
                break;
            case EquipmentType.Shoes:
                selectedItem = shoes.Find(s => s.id == userData.equipmentSpecificList[userData.currentShoesId].id);
                equipSpec = userData.equipmentSpecificList[userData.currentShoesId];
                break;
            case EquipmentType.Coat:
                selectedItem = coats.Find(c => c.id == userData.equipmentSpecificList[userData.currentCoatId].id);
                equipSpec = userData.equipmentSpecificList[userData.currentCoatId];
                break;
            default:
                Debug.LogError("잘못된 EquipmentTypes 기입");
                return;
        }
        
        for (int i = 0; i < selectedItem.stat.Count; i++)
            SetStat(selectedItem.stat[i], selectedItem.statAmount[i], selectedItem.statCft[i], equipSpec.level);
    }

    private void SetStat(EquipmentOption option, float amount, float cft, int level)
    {
        var playerStat = DataManager.instance.playerStat;
        switch (option)
        {
            case EquipmentOption.AttackPower:
                playerStat.equipmentAttackPower += amount + cft * (level - 1);
                break;
            case EquipmentOption.AttackSpeed:
                playerStat.equipmentAttackSpeed += amount + cft * (level - 1);
                break;
            case EquipmentOption.MoveSpeed:
                playerStat.equipmentMoveSpeed += amount + cft * (level - 1);
                break;
            case EquipmentOption.Hp:
                playerStat.equipmentHp += amount + cft * (level - 1);
                break;
            case EquipmentOption.CriticalPercent:
                playerStat.equipmentCriticalPercent += amount + cft * (level - 1);
                break;
            case EquipmentOption.CriticalMultipleMin:
                playerStat.equipmentCriticalMultipleMin += amount + cft * (level - 1);
                break;
            case EquipmentOption.CriticalMultipleMax:
                playerStat.equipmentCriticalMultipleMax += amount + cft * (level - 1);
                break;
            case EquipmentOption.AvoidPercent:
                playerStat.equipmentAvoidPercent += amount + cft * (level - 1);
                break;
            default:
                Debug.LogError("잘못된 EquipmentOption이 기입되었습니다.");
                break;
        }
    }

    private void SetSecretStat(EquipmentOption option, float amount)
    {
        var playerStat = DataManager.instance.playerStat;
        switch (option)
        {
            case EquipmentOption.CriticalPercent:
                playerStat.secret_equipmentCriticalPercent += amount;
                break;
            case EquipmentOption.Piercing:
                playerStat.secret_equipmentPiercing += amount;
                break;
            case EquipmentOption.ProjectileSpeed:
                playerStat.secret_equipmentProjectileSpeed += amount;
                break;
            default:
                Debug.LogError("잘못된 EquipmentOption이 기입되었습니다.");
                break;
        }
    }
}