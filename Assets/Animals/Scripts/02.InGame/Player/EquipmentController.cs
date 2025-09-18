using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class EquipmentController : MonoBehaviour
{
    private PlayerController playerController;
    private WeaponController weaponController;
    
    private void Awake()
    {
        DOVirtual.DelayedCall(1f, () => enabled = true);
        enabled = false;
    }

    private void Start()
    {
        playerController = PlayerController.instance;
        weaponController = WeaponController.instance;
        SetItemsEpicEffect();
    }

    private void SetItemsEpicEffect()
    {
        var itemManager = ItemManager.instance;
        var userData = DataManager.instance.userData;
        Item selectedItem;
        
        if (0 <= userData.CurrentWeaponId && userData.CurrentWeaponId < userData.equipmentSpecificList.Count &&
            userData.equipmentSpecificList[userData.CurrentWeaponId].grade >= 3) {
            selectedItem = itemManager.weapons.Find(w => w.id == userData.equipmentSpecificList[userData.CurrentWeaponId].id);
            InvokeEpicEffect(selectedItem.epicEffect, selectedItem.epicParameter);
        }
        if (0 <= userData.currentHatId && userData.currentHatId < userData.equipmentSpecificList.Count &&
            userData.equipmentSpecificList[userData.currentHatId].grade >= 3) {
            selectedItem = itemManager.hats.Find(w => w.id == userData.equipmentSpecificList[userData.currentHatId].id);
            InvokeEpicEffect(selectedItem.epicEffect, selectedItem.epicParameter);
        }
        if (0 <= userData.currentAccessoryId && userData.currentAccessoryId < userData.equipmentSpecificList.Count &&
            userData.equipmentSpecificList[userData.currentAccessoryId].grade >= 3) {
            selectedItem = itemManager.accessories.Find(w => w.id == userData.equipmentSpecificList[userData.currentAccessoryId].id);
            InvokeEpicEffect(selectedItem.epicEffect, selectedItem.epicParameter);
        }
        if (0 <= userData.currentShoesId && userData.currentShoesId < userData.equipmentSpecificList.Count &&
            userData.equipmentSpecificList[userData.currentShoesId].grade >= 3) {
            selectedItem = itemManager.shoes.Find(w => w.id == userData.equipmentSpecificList[userData.currentShoesId].id);
            InvokeEpicEffect(selectedItem.epicEffect, selectedItem.epicParameter);
        }
        if (0 <= userData.currentCoatId && userData.currentCoatId < userData.equipmentSpecificList.Count &&
            userData.equipmentSpecificList[userData.currentCoatId].grade >= 3) {
            selectedItem = itemManager.coats.Find(w => w.id == userData.equipmentSpecificList[userData.currentCoatId].id);
            InvokeEpicEffect(selectedItem.epicEffect, selectedItem.epicParameter);
        }
    }

    private void InvokeEpicEffect(string epicEffect, float epicParameter)
    {
        playerController.gameObject.SendMessage(epicEffect, epicParameter, SendMessageOptions.DontRequireReceiver);
        weaponController.gameObject.SendMessage(epicEffect, epicParameter, SendMessageOptions.DontRequireReceiver);
        //SendMessage(epicEffect, epicParameter, SendMessageOptions.DontRequireReceiver);
    }
}
