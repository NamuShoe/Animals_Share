using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataManager: MonoBehaviour {
    public static DataManager instance;
    [SerializeField] private GameObject RewardMSG;
    [SerializeField] private GameObject[] RewardButton;
    [SerializeField] private GameObject BackGroundPanel;
    [SerializeField] private Button GameStartBtn;
    [SerializeField] private Text RewardText;
    [SerializeField] private Text currentUidText;
    [SerializeField] private TextMeshProUGUI currentUidText1;
    [SerializeField] private InputField uidInputField; // 위 2개 하나로 통합하기
    [SerializeField] private GameObject tutorialGameObject;
    [SerializeField] private KeyValuePair<ItemType, int> a;
    string loadJson;
    public int receivedNum; // 클릭한 무기 번호
    public UserData userData;
    public PlayerStats playerStat;
    private string savePath; // userData Path
    private float rewardInterval = 10f;
    const int MAXlife = 30;
    void Awake() {
        if (instance == null) {
            instance = this;
            playerStat = new PlayerStats();
        }
    }
    void Start() {
        FolderSettings(); // 폴더 경로 지정
        //if(File.Exists(savePath)) LoadUserData(); // 데이터 파일 불러오기
        StartCoroutine(DataCheck()); // 최초 접속 여부, 시간 등을 확인
    }
    private void LoadUserData() // json 데이터를 불러오기
    {
        if(LoginManager.loginComplete)
        {
            loadJson = File.ReadAllText(savePath);
            userData = JsonUtility.FromJson<UserData>(loadJson);
        }
    }
    
    public void DailyLoginChecked() // 출석 보상 수령 확인
    {
        Debug.Log(userData.loginDate + "일 차 보상 골드 획득 : " + userData.loginDate * 10);
        //IncreaseGold(userData.loginDate * 10); // 정리 중에 임시 주석 IncreaseGoods로 통일

        MissionManager.instance.MissionClearCheck(8);
        SaveUserData(); // 저장 후
        PopupManager.instance.CloseTap((int) PopupType.DailyReward);
        //RewardMSG.SetActive(false);
        //BackGroundPanel.SetActive(false);
    }

    IEnumerator DataCheck()
    {
        yield return new WaitUntil(() => LoginManager.loginComplete);
        LoadUserData();
        //currentUidText.text = userData.guestCode; // 메인 작업 할때 반영
        currentUidText1.text = userData.guestCode;

        RankingManager.instance.SetRank();
        RankingManager.instance.ReadMyRankingDB();
        DateTime currentTime = LoginManager.instance.DateNow; // 현재 시간
        DateTime endTime;
        if (userData.endTimeStr != "") // 문자열로부터 시간을 변환
            endTime = DateTime.Parse(userData.endTimeStr);
        else {
            endTime = currentTime.AddDays(-1);
            userData.endTimeStr = endTime.ToString();
        }

        TimeSpan timeDiff = currentTime - endTime; // 종료 시간과의 차이 값 계산
        var dayDiff = (new DateTime(currentTime.Year, currentTime.Month, currentTime.Day) -
                      new DateTime(endTime.Year, endTime.Month, endTime.Day)).TotalDays;
        
        if (userData.isFirstLogin) // 최초 접속 시
        {
            //tutorialGameObject.SetActive(true);
            userData.isFirstLogin = false;
        }
        else // 최초 접속이 아니라면
        {
            //Debug.Log("흐른 일 수 : " + (int)timeDiff.Days + "일"); // 지난 일 수 표시
            //Debug.Log("흐른 시간(초) : " + (int)timeDiff.TotalSeconds  + " -> 획득 라이프 : " + (int)(timeDiff.TotalSeconds / rewardInterval)); // 지난 시간을 초 단위로 표시

            userData.Life += (int)(timeDiff.TotalSeconds / rewardInterval); // 초 단위 시간을 10으로 나눠서 라이프로 변환
            userData.Life = Mathf.Clamp(userData.Life, 0, MAXlife);
            if (userData.todayReceive == true) userData.todayReceive = false; // 다음 날 접속 했으면 수령 여부 false
        } 

        // Debug.LogError("userData.loginDate >= 0 \t " + (userData.loginDate >= 0));
        // Debug.LogError("userData.todayReceive == false \t " + (userData.todayReceive == false));
        // Debug.LogError(" dayDiff >=  1" + " \t " +  dayDiff);

        if (userData.loginDate >= 0 && 
            userData.todayReceive == false &&
            dayDiff >= 1) // 접속 확인, 흐른 날짜, 오늘자 수령 여부 확인, 임시로 안되게 막음
        {
            userData.todayReceive = true;
            userData.loginDate += 1;
            PopupManager.instance.OpenTap((int)PopupType.DailyReward, false);

            if (userData.itemList.Exists(x => x.itemType == ItemType.AdsSmallGold) == false)
                userData.itemList.Add(new ConsumeItem(ItemType.AdsSmallGold, 1));
            else {
                userData.itemList.Find(x => x.itemType == ItemType.AdsSmallGold).amount = 1;
            }

            if (userData.itemList.Exists(x => x.itemType == ItemType.AdsSmallLife) == false)
                userData.itemList.Add(new ConsumeItem(ItemType.AdsSmallLife, 1));
            else {
                userData.itemList.Find(x => x.itemType == ItemType.AdsSmallLife).amount = 1;
            }
            //RewardMSG.SetActive(true); // 보상 메시지 활성화
            //BackGroundPanel.SetActive(true);

            // 일일퀘스트
            MakeDailyQuest();
            
            userData.dailyQuestList.Add(new Mission(16));//일일 퀘스트 전체 완료
            userData.dailyQuestClear.Add(false);
            
            //랭킹 모드 플레이 하기 막기*************************************
            var mission = userData.dailyQuestList.Find(x => x.missionNum == 4);
            if (mission != null) {
                mission.missionNum += 1;
            }
            //보스 처치하기 막기*************************************
            mission = userData.dailyQuestList.Find(x => x.missionNum == 15);
            if (mission != null) {
                mission.missionNum += 1;
            }
            
            MissionManager.instance.SetMissionData();

            for (int i = 0; i < RewardButton.Length; i++) {
                RewardButton[i].transform.GetChild(0).GetComponent<Button>().interactable = false;
                RewardButton[i].transform.GetChild(1).GetComponent<Text>().text = (i + 1) + "일차";
            }

            RewardButton[userData.loginDate - 1].transform.GetChild(0).GetComponent<Button>().interactable = true;
            RewardText.text = userData.loginDate + "일차";
        }

        //Main씬 "전환" 시 호출
        GoodsManager.instance.UiRefresh();
        userData.CurrentCharacterId = userData.CurrentCharacterId;
        MainManager.instance.ProfileSet();
        ChangeBasicStats();

        SaveUserData();
        //MissionManager.instance.MissionClearCheck(2, userData.dailyCatchMobs); // 일회성 미션 아닌 것, 회수 검증 진행 중
        //MissionManager.instance.MissionClearCheck(3);
        //MissionManager.instance.MissionClearCheck(4); //랭킹모드X
    }

    private void MakeDailyQuest()
    {
        userData.dailyQuestList.Clear();
        userData.dailyQuestClear.Clear();
        List<int> numbers = new List<int>();
        for (int i = 1; i <= 5; i++) {
            numbers.Add(i);
        }

        // 리스트 셔플
        for (int i = 0; i < numbers.Count; i++) {
            int randomIndex = UnityEngine.Random.Range(0, numbers.Count);
            (numbers[i], numbers[randomIndex]) = (numbers[randomIndex], numbers[i]);
        }

        var tempList = numbers.GetRange(0, 3);
        for (int i = 0; i < tempList.Count; i++) {
            userData.dailyQuestList.Add(new Mission(tempList[i] + (i * 5)));
            userData.dailyQuestClear.Add(false);
            //Debug.Log("섞여나온 숫자 : " + (tempList[i] + (i * 5)));
        }
    }
    
    public void AddEquipment(int itemId, int itemType, int itemGrade = 0) // 무기 추가
    {
        if (itemId == 0) return;

        //미장착 상태라면 미장착 값 재갱신
        if (userData.CurrentWeaponId >= userData.equipmentSpecificList.Count)
            userData.CurrentWeaponId++;
        if (userData.currentHatId >= userData.equipmentSpecificList.Count)
            userData.currentHatId++;
        if (userData.currentAccessoryId >= userData.equipmentSpecificList.Count)
            userData.currentAccessoryId++;
        if (userData.currentShoesId >= userData.equipmentSpecificList.Count)
            userData.currentShoesId++;
        if (userData.currentCoatId >= userData.equipmentSpecificList.Count)
            userData.currentCoatId++;
        
        userData.equipmentSpecificList.Add(new EquipmentSpecific(itemId, itemType, itemGrade, 1));
        InventoryManager.instance.CreateItemFrame(userData.equipmentSpecificList.Count - 1);
        //InventoryManager.instance.AddWeapon(0, itemId, itemGrade); // 현재 의미 없음
        SaveUserData();
    }

    public void TakeReward(RewardType rewardType, int amount)
    {
        switch (rewardType) {
            case RewardType.Gold:
                userData.Gold += amount;
                break;
            case RewardType.Diamond:
                userData.Diamond += amount;
                break;
            case RewardType.Life:
                userData.Life += amount;
                break;
            case RewardType.NormalBox:
                if (userData.itemList.Exists(x => x.itemType == ItemType.NormalBox) == false) {
                    userData.itemList.Add(new ConsumeItem(ItemType.NormalBox, amount));
                }
                else {
                    userData.itemList.Find(x => x.itemType == ItemType.NormalBox).amount += amount;
                }
                break;
            case RewardType.MagicBox:
                if (userData.itemList.Exists(x => x.itemType == ItemType.MagicBox) == false) {
                    userData.itemList.Add(new ConsumeItem(ItemType.NormalBox, amount));
                }
                else {
                    userData.itemList.Find(x => x.itemType == ItemType.MagicBox).amount += amount;
                }
                break;
            case RewardType.EquipLevelUpMaterial:
                for (int j = 0; j < userData.levelUpMaterial.Count; j++)
                    userData.levelUpMaterial[j] += amount;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public void AddMaterial(int materialType)
    {
        userData.levelUpMaterial[materialType]++;
        SaveUserData();
    }
    
    public void ChangeEquipments(EquipmentType itemType, int index) // 아이템 종류와 ID를 통해 바꿈
    {
        switch (itemType) {
            case EquipmentType.Weapon:
                InventoryManager.instance.ClearEquipment(userData.CurrentWeaponId, itemType);
                userData.CurrentWeaponId = index;
                break;
            case EquipmentType.Hat:
                InventoryManager.instance.ClearEquipment(userData.currentHatId, itemType);
                userData.currentHatId = index;
                break;
            case EquipmentType.Accessory:
                InventoryManager.instance.ClearEquipment(userData.currentAccessoryId, itemType);
                userData.currentAccessoryId = index;
                break;
            case EquipmentType.Shoes:
                InventoryManager.instance.ClearEquipment(userData.currentShoesId, itemType);
                userData.currentShoesId = index;
                break;
            case EquipmentType.Coat:
                InventoryManager.instance.ClearEquipment(userData.currentCoatId, itemType);
                userData.currentCoatId = index;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null);
        }

        InventoryManager.instance.EquipPopUpSetting(index);
        SaveUserData();
        ChangeBasicStats();
    }
    
    public void ChangeBasicStats() {
        playerStat.Clear();
        
        if(0 <= userData.CurrentWeaponId
            && userData.CurrentWeaponId < userData.equipmentSpecificList.Count) // 무기
        {
            ItemManager.instance.SetStats(EquipmentType.Weapon);
        }

        if(0 <= userData.currentHatId
           && userData.currentHatId < userData.equipmentSpecificList.Count) // 모자
        {
            ItemManager.instance.SetStats(EquipmentType.Hat);
        }
        
        if(0 <= userData.currentCoatId
           && userData.currentCoatId < userData.equipmentSpecificList.Count) // 상의
        {
            ItemManager.instance.SetStats(EquipmentType.Coat);
        }

        if(0 <= userData.currentAccessoryId
           && userData.currentAccessoryId < userData.equipmentSpecificList.Count) // 악세서리
        {
            ItemManager.instance.SetStats(EquipmentType.Accessory);
        }
        
        if(0 <= userData.currentShoesId
           && userData.currentShoesId < userData.equipmentSpecificList.Count) // 신발
        {
            ItemManager.instance.SetStats(EquipmentType.Shoes);
        }
        
        //캐릭터 스탯 반영
        // playerStat.characterHp += PediaManager.instance.characters[userData.CurrentCharacterId - 1].hp;
        // playerStat.characterAttackPower += PediaManager.instance.characters[userData.CurrentCharacterId - 1].attackPower;
        // playerStat.characterAttackSpeed += PediaManager.instance.characters[userData.CurrentCharacterId - 1].attackSpeed;
        // playerStat.characterMoveSpeed += PediaManager.instance.characters[userData.CurrentCharacterId - 1].moveSpeed;

        var characterSpecific = userData.characterInforms.Find(x => x.id == (userData.CurrentCharacterId));
        playerStat.characterHp += PediaManager.instance.characters[userData.CurrentCharacterId - 1].hp + (characterSpecific.level - 1);
        playerStat.characterAttackPower += PediaManager.instance.characters[userData.CurrentCharacterId - 1].attackPower + (characterSpecific.level - 1);
        playerStat.characterAttackSpeed += PediaManager.instance.characters[userData.CurrentCharacterId - 1].attackSpeed + (characterSpecific.level - 1);
        playerStat.characterMoveSpeed += PediaManager.instance.characters[userData.CurrentCharacterId - 1].moveSpeed;
        
        InventoryManager.instance.UserStatsRefresh();
    }
    public void RecordRanking(int score) {
        if (userData.rankingScore < score) userData.rankingScore = score;
        else return;

        SaveUserData();
    }
    public async void ChangeUID() // 재로그인
    {
        // 유효 UID 검사 과정
        bool isUidValid = await LoginManager.instance.CheckUidDB(uidInputField.text);

        if (!isUidValid)
        {
            uidInputField.placeholder.GetComponent<Text>().text = "UID를 다시 확인해주세요!";
            Debug.LogWarning("유효하지 않은 UID 입력. 절차 중단.");
            uidInputField.SetTextWithoutNotify(""); // 값 초기화 반영
            return;
        }

        // 검사 후
        LoginManager.loginComplete = false;
        userData.guestCode = uidInputField.text; // 로컬 데이터 수정
        Debug.Log("쓰인 유저데이터 : " +  userData.guestCode);

        // ***************************팝업 코드 끝나는대로 닫는 코드 추가 예정***************************
        SaveUserData();
        uidInputField.SetTextWithoutNotify(""); // 값 초기화 반영
        //LoginManager.instance.LoginInit();
        _SceneManager.instance.MainMenu();
    }
    public void LogOut() // 구글 계정 기입을 위한 로그아웃, 임시
    {
        LoginManager.loginComplete = false;
        SaveUserData();
        File.Delete(savePath); // 파일 제거
        PlayerPrefs.DeleteKey("guestCode");
        GoodsManager.instance.StopUpdateData();
        //userData = new UserData("", ""); // 게스트 코드, 닉네임 정보 초기화
        //LoginManager.instance.LoginInit();
        _SceneManager.instance.MainMenu();
    }
    public void CopyUidButton()
    {
        CopyUID(PlayerPrefs.GetString("guestCode"));
    }
    public static void CopyUID(string uid) // UID 복사
    {
        GUIUtility.systemCopyBuffer = uid;
    }
    public void SaveUserData() // 유저 데이터 저장
    { 
        // 현재 EndTime을 문자열로 저장
        //RecordEndTime();

        //userData.SerializeTestList();
        string json = JsonUtility.ToJson(userData, true);
        File.WriteAllText(savePath, json); // 저장과 함께 UI 갱신
    }
    
    public void RecordEndTime()
    {
        userData.endTimeStr = LoginManager.instance.DateNow.ToString();
    }
    private void FolderSettings() { // 플랫폼을 체크하여 경로 지정
        savePath = Path.Combine(Application.dataPath, "Resources/GameData/UserData.json");

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
            savePath = Path.Combine(Application.persistentDataPath + "/UserData.json");
        }
    }
    
    void OnApplicationQuit()
    {
        RecordEndTime();
        SaveUserData(); // 종료 시간 업데이트를 저장
        //LoginManager.instance.WriteUserDB();
        Debug.Log("종료 시간 기록 : " + userData.endTimeStr); // 기록된 종료 시간 표시
        //Application.Quit(); // 종료
    }
}