using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TestMode : MonoBehaviour
{
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) Time.timeScale *= 2; //2배속
            else if (Input.GetKeyDown(KeyCode.DownArrow)) Time.timeScale /= 2; //2감속
            else if (Input.GetKeyDown(KeyCode.Space)) Time.timeScale = 1; //정상
            else if (Input.GetKeyDown(KeyCode.Escape)) GameManager.instance.isWin = true; // 승리
            else if (Input.GetKeyDown(KeyCode.E)) PlayerController.instance.AddEXP(100);
            else if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("점수 반영");
                //for(int i = 1; i < 61; i++)
                var userData = DataManager.instance.userData;
                RankingManager.instance.NewRecord(userData.guestCode, userData.userName, userData.userIcon, 54);
                // DataManager.instance.userData.rankingScore
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                DataManager.instance.SaveUserData();
                LoginManager.instance.WriteUserDB();
                // LoginManager.instance.WriteUserParameter("equipmentSpecificList");
            }
            //else if (Input.GetKeyDown(KeyCode.C)) GetCharacter();
            else if (Input.GetKeyDown(KeyCode.V)) ResetCharacter();
            else if (Input.GetKeyDown(KeyCode.S)) ShoeMeTheMoney();
            else if (Input.GetKeyDown(KeyCode.P)) 
            {
                PassManager.instance.ReceivePass(1);
                MissionManager.instance.MissionClearCheck(15);
            }
        }
    }
    public void ShoeMeTheMoney(){
        //DataManager.instance.userData.currentExp += 100;
        DataManager.instance.userData.Gold += 1000;
        DataManager.instance.userData.Diamond += 100;
        DataManager.instance.userData.Life += 30;

        for(int i = 0; i < DataManager.instance.userData.levelUpMaterial.Count; i++)
            DataManager.instance.userData.levelUpMaterial[i] += 20;

        DataManager.instance.SaveUserData();
    }
    public void GetAll(){
        int itemCount = 0;
        int itemTypeCount = 5;
        GetCharacter(); // 캐릭터, 아래는 무기
        for(int i = 0; i < itemTypeCount; i++)
        {
            itemCount = i switch
            {
                0 => ItemManager.instance.weapons.Count,
                1 => ItemManager.instance.hats.Count,
                2 => ItemManager.instance.accessories.Count,
                3 => ItemManager.instance.shoes.Count,
                4 => ItemManager.instance.coats.Count,
                _ => itemCount
            };
            for(int j = 1; j < itemCount; j++) // 종류별 아이템 개수만큼만 추가
            {
                //var weaponId = ItemManager.instance.weapons[i].id;
                DataManager.instance.AddEquipment(j, i);
            }
        }

        // DataManager.instance.userData.CurrentWeaponId = -1;
        // DataManager.instance.userData.currentHatId = -1;
        // DataManager.instance.userData.currentAccessoryId = -1;
        // DataManager.instance.userData.currentShoesId = -1;
        // DataManager.instance.userData.currentCoatId = -1; // DataManager.instance.userData.equipmentSpecificList.Count

        // InventoryManager.instance.ClearEquipment(DataManager.instance.userData.CurrentWeaponId, 0); // 무기 장착 자동으로 해제됨
        // InventoryManager.instance.ClearEquipment(DataManager.instance.userData.currentHatId, 1); // 무기 장착 자동으로 해제됨
        // InventoryManager.instance.ClearEquipment(DataManager.instance.userData.currentAccessoryId, 2); // 무기 장착 자동으로 해제됨
        // InventoryManager.instance.ClearEquipment(DataManager.instance.userData.currentShoesId, 3); // 신발 장착 자동으로 해제됨
        // InventoryManager.instance.ClearEquipment(DataManager.instance.userData.currentCoatId, 4); // 무기 장착 자동으로 해제됨
        InventoryManager.instance.InventoryOpen();
        DataManager.instance.SaveUserData();
    }
    public void GetCharacter() {
        Debug.Log("캐릭터 획득");
        List<int> characterNum = new List<int>();
        DataManager.instance.userData.characterList.Clear(); // 중복 없도록 클리어
        DataManager.instance.userData.characterInforms.Clear();
        for(int i = 1; i <= 4; i++) {
            characterNum.Add(i);
        }
        foreach (int item in characterNum) {
            DataManager.instance.userData.characterList.Add(item);
            DataManager.instance.userData.characterInforms.Add(new CharacterSpecific(item));
        }
        //InventoryManager.instance.ReadSavedCharacters();
        DataManager.instance.SaveUserData();
        PediaManager.instance.CharacterPossession();
        MainManager.instance.ProfileSet();
    }
    public void ResetCharacter() {
        Debug.Log("캐릭터 초기화");
        DataManager.instance.userData.characterList.Clear();
        DataManager.instance.userData.characterList.Add(1);
        DataManager.instance.SaveUserData();
        //InventoryManager.instance.ReadSavedCharacters();
    }
}
