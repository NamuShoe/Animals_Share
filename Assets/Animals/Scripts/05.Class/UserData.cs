using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class CharacterSpecific
{
    public int id; // 캐릭터 id
    public int level; // 캐릭터 레벨
    public int experience; // 캐릭터 경험치
    public List<int> characterSkillList; // 캐릭터별 스킬포인트 정보
    //public List<string> characterSkillNameList; // 스킬 이름
    public CharacterSpecific(int id, int level = 1, int experience = 0, List<int> characterSkillList = null)
    {
        this.id = id;
        this.level = level;
        this.experience = experience;
        this.characterSkillList = characterSkillList ?? new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        //this.characterSkillNameList = characterSkillNameList ?? new List<string> {"","","","","","","","",""};
        //this.characterSkillNameList = PediaManager.instance.characters[id-1].characterSkillNameList; // 캐릭터 번호로 리스트 가져오기
    }
}
[Serializable]
public class EquipmentSpecific
{
    public int id; // 장비 Id
    public int type; // 장비 종류
    public int grade; // 장비 등급
    public int level; // 장비 레벨
    
    public EquipmentSpecific(int equipmentId, int equipmentType, int equipmentGrade, int equipmentLevel = 1)
    {
        this.id = equipmentId;
        this.type = equipmentType;
        this.grade = equipmentGrade;
        this.level = equipmentLevel;
    }
    public EquipmentSpecific(EquipmentSpecific equipment)
    {
        this.id = equipment.id;
        this.type = equipment.type;
        this.grade = equipment.grade;
        this.level = equipment.level;
    }
    
    public static bool operator ==(EquipmentSpecific e1, EquipmentSpecific e2)
    {
        return e1.id == e2.id && e1.type == e2.type && e1.grade == e2.grade && e1.level == e2.level;
    }

    public static bool operator !=(EquipmentSpecific e1, EquipmentSpecific e2)
    {
        return !(e1 == e2);
    }
}

public enum RewardType {
    Gold,
    Diamond,
    Life,
    NormalBox,
    MagicBox,
    EquipLevelUpMaterial
}

public enum ItemType {
    ResurrectionTicket,
    NormalBox,
    MagicBox,
    AdsSmallGold,
    AdsSmallLife,
    AdsTree,
    AdsIron,
    AdsCotton,
    AdsMagicPowder,
    AdsEmerald,
    AdsEquipLevelUpMaterial
}

[Serializable]
public class ConsumeItem {
    [JsonConverter(typeof(StringEnumConverter))]
    public ItemType itemType;
    public int amount;

    public ConsumeItem(ItemType itemType, int amount)
    {
        this.itemType = itemType;
        this.amount = amount;
    }
}
[Serializable]
public class RewardItem{
    [JsonConverter(typeof(StringEnumConverter))]
    public RewardType rewardType;
    public int amount;

    public RewardItem(RewardType rewardType, int amount)
    {
        this.rewardType = rewardType;
        this.amount = amount;
    }
}

[Serializable]
public class UserData
{
// UserStatus
    public string userId; // 유저 아이디
    public string userName; // 유저 이름
    public string guestCode; // 게스트 로그인 코드
    public string endTimeStr; // 시간을 string으로 저장
    public bool isFirstLogin; // 최초 접속 상태 여부
    public bool todayReceive; // 오늘 출석 수령했는지
    public int userIcon; // 프로필 아이콘 번호
    public int loginDate; // 출석 일 수
    public int passPoint; // 배틀패스 포인트
    public int rankingScore; // 랭킹전 최고 점수
    public List<bool> purchaseCheck; // 1회 패키지 상품 구매 여부, 초보 특공대 스타터 패키지 ~ 캐릭터(네로) 패키지까지 5개(PackageData.json)
    public List<int> stageClearCheck; // 스테이지 클리어 여부
    public List<int> enemyClearCheck; // 적 처치 여부
    public List<int> mailRead;

    public List<bool> mainQuestClear;
    // 일일 초기화
    public List<Mission> dailyQuestList; // 일일 퀘스트 번호
    public List<bool> dailyQuestClear; // 클리어 여부
    
// UserGoods
    [SerializeField]private int gold; // 골드
    public int Gold { get => gold; set { gold = value; UserDataManager.instance.SetGold(); GoodsManager.instance.UiRefresh(); } } // 10000 이상 표시를 위해 임시 추가
    [SerializeField]private int diamond; // 보석
    public int Diamond { get => diamond; set { diamond = value; UserDataManager.instance.SetDiamond(); } }
    [SerializeField]private int life; // 라이프
    public int Life { get => life; set { life = 0 > value ? 0 : value; UserDataManager.instance.SetLife(); GoodsManager.instance.UiRefresh(); } }

    /// <summary>
    /// 1:목재 2:강철 3:목화 4:마법 가루 5:보석
    /// </summary>
    public List<int> levelUpMaterial;
    
    // 보유 아이템
    public List<ConsumeItem> itemList;
    
// UserItemInformation
    // 캐릭터 정보
    public List<int> characterList; // 캐릭터 ID 번호 저장
    public List<CharacterSpecific> characterInforms = new List<CharacterSpecific>(); // 캐릭터 정보 저장(레벨, 경험치)
    [SerializeField]private int currentCharacterId; // 현재 선택한 캐릭터
    public int CurrentCharacterId { get => currentCharacterId; set { currentCharacterId = value; UserDataManager.instance.SetCurrentCharacterId(); } }

    // 보유 장비 아이템
    public List<EquipmentSpecific> equipmentSpecificList;

    // 장착 중인 아이템 번호(아이템 번호 인덱스)
    [SerializeField]private int currentWeaponId; // 현재 착용 무기
    public int CurrentWeaponId { get => currentWeaponId; set { currentWeaponId = value; UserDataManager.instance.SetCurrentWeaponId(); } }
    public int currentHatId;
    public int currentAccessoryId;
    public int currentShoesId;
    public int currentCoatId;
    
    // 기본 생성자
    public UserData(string guestCode_)
    {
        // UserStatus
        userId = "User_00001";
        userName = "DefaultName"; //userName_;
        guestCode = guestCode_;
        endTimeStr = "";
        isFirstLogin = true;
        todayReceive = false;
        userIcon = 0;
        loginDate = 0;
        passPoint = 0;
        rankingScore = 0;
        purchaseCheck = new List<bool> { false, false, false, false, false };
        stageClearCheck = new List<int> { 0, 0, 0, 0, 0 };
        enemyClearCheck = new List<int>();
        mailRead = new List<int>();

        mainQuestClear = new List<bool> { false, false, false, false, false };
        // 일일 초기화
        dailyQuestList = new List<Mission>(); //List<int> { 0, 0, 0 };
        dailyQuestClear = new List<bool> { false, false, false, false };

        // UserGoods
        gold = 0;
        diamond = 0;
        life = 0;
        levelUpMaterial = new List<int> { 0, 0, 0, 0, 0 };
        
        // 보유 아이템
        itemList = new List<ConsumeItem>();

        // 캐릭터 정보
        characterList = new List<int> { 1 };
        characterInforms.Add(new CharacterSpecific(1, 1, 0, new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }));
        CurrentCharacterId = 1; // 기본값 설정
        
        // 보유 장비 아이템
        equipmentSpecificList = new List<EquipmentSpecific> { new EquipmentSpecific(1, 0, 0, 1) };
        
        // 장착 중인 아이템 번호(아이템 번호 인덱스)
        CurrentWeaponId = 0;
        currentHatId = -1;
        currentAccessoryId = -1;
        currentShoesId = -1;
        currentCoatId = -1;
    }

    public void InitParameter(string parameterName)
    {
        switch (parameterName) {
            case "userName":
                userName = "DefaultName";
                break;
            case "endTimeStr":
                endTimeStr = "";
                break;
            case "isFirstLogin":
                isFirstLogin = true;
                break;
            case "todayReceive":
                todayReceive = false;
                break;
            case "userIcon":
                userIcon = 0;
                break;
            case "loginDate":
                loginDate = 0;
                break;
            case "passPoint":
                passPoint = 0;
                break;
            case "rankingScore":
                rankingScore = 0;
                break;
            case "purchaseCheck":
                purchaseCheck = new List<bool> { false, false, false, false, false };
                break;
            case "stageClearCheck":
                stageClearCheck = new List<int> { 0, 0, 0, 0, 0 };
                break;
            case "enemyClearCheck":
                enemyClearCheck = new List<int>();
                break;
            case "mailRead":
                mailRead = new List<int>();
                break;
            
            case "mainQuestClear":
                mainQuestClear = new List<bool> { false, false, false, false, false };
                break;
            case "dailyQuestList":
                dailyQuestList = new List<Mission>();
                break;
            case "dailyQuestClear":
                dailyQuestClear = new List<bool> { false, false, false, false };
                break;
            
            case "gold":
                gold = 0;
                break;
            case "diamond":
                diamond = 0;
                break;
            case "life":
                life = 0;
                break;
            case "levelUpMaterial":
                levelUpMaterial = new List<int> { 0, 0, 0, 0, 0 };
                break;
            
            case "itemList":
                itemList = new List<ConsumeItem>();
                break;
            
            case "characterList":
                characterList = new List<int> { 1 };
                break;
            case "characterInforms":
                characterInforms.Add(new CharacterSpecific(1, 1, 0, new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }));
                break;
            case "currentCharacterId":
                currentCharacterId = 1;
                break;
            
            case "equipmentSpecificList":
                equipmentSpecificList = new List<EquipmentSpecific> { new EquipmentSpecific(1, 0, 0, 1) };
                break;
            
            case "currentWeaponId":
                currentWeaponId = 0;
                break;
            case "currentHatId":
                currentHatId = -1;
                break;
            case "currentAccessoryId":
                currentAccessoryId = -1;
                break;
            case "currentShoesId":
                currentShoesId = -1;
                break;
            case "currentCoatId":
                currentCoatId = -1;
                break;
            default:
                Debug.LogError("맞는 변수가 없습니다.");
                break;
        }
    }
}
