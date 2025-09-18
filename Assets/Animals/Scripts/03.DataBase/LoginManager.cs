using Firebase;
using Firebase.Extensions;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Serialization;

public class LoginManager : MonoBehaviour
{
    public string dbURL = "https://animalsshooting-7850d-default-rtdb.firebaseio.com/";
    public string serverTimeURL = "";
    public static LoginManager instance;
    //public static string guestCode; // UploadManager에서 사용
    public string guestCode;
    public int LoadingStatus; // 로딩 현황
    public string userName;
    public static bool loginComplete;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject mainManager;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private GameObject loginCanvas;
    [SerializeField] private GameObject logoPanel;
    [SerializeField] private GameObject tutorialGameObject;
    [SerializeField] private GameObject createUIDBtn;
    [SerializeField] private GameObject LoginUIDBtn;
    [SerializeField] private GameObject gameTitle;
    
    [Header("Main GameObject")]
    [SerializeField] private GameObject main_night_ef;
    
    [Header("Loading GameObject")]
    [SerializeField] private GameObject night_ef;
    
    [Header("userName")]
    [SerializeField] private GameObject userNamePopup;
    [SerializeField] private GameObject nameCheckPopup;
    [SerializeField] private Button nameCheckBtn;
    [SerializeField] private InputField userNameInput;
    [SerializeField] private Text informText;
    [SerializeField] private Text nameCheckText;

    [Header("Login")] 
    [SerializeField] private GameObject loginPopup;
    [SerializeField] private Button loginBtn;
    [SerializeField] private InputField UIDInput;
    
    [Space(20f)]
    [SerializeField] private GameObject checkInternetPopup;
    [SerializeField] private Text loadingText;
    [SerializeField] private Text checkInternetText;
    
    private string savePath;
    private int totalLength;
    [SerializeField] private bool isAdmin; // 관리자 확인
    private DateTime dateNow;
    public DateTime DateNow => dateNow;

    void Awake()
    {
        LoginInit();
    }

    public void LoginInit() // Awake()
    {
        Application.targetFrameRate = 60; // 시작 후 60프레임 설정
        totalLength = 0;
        instance = this;
        SoundManager.instance.StopBGM();
        FolderSettings();
            
        StartCoroutine(ServerTimeCheck());
        if (loginComplete == false)
        {
            loadingBar.gameObject.SetActive(false);
            mainCanvas.SetActive(false);
            mainManager.SetActive(true);
            loginCanvas.SetActive(true);
            logoPanel.SetActive(true);
            transform.gameObject.SetActive(true);
            StartCoroutine(Intro());
        }
        else // 로그인 완료 후의 상태
        {
            loginCanvas.SetActive(false);
            mainManager.SetActive(true);
            mainCanvas.SetActive(false);
            ReadUserJson();
            SoundManager.instance.InitSound();
            DOVirtual.DelayedCall(1.0f, () => {
                mainCanvas.SetActive(true);
                AfterLoad();
            });
            //StartCoroutine(WaitForSingleTon());
        }

        //if(!loginComplete) StartCoroutine(Intro()); // Start()
    }

    #region Time
    private IEnumerator ServerTimeCheck()
    {
        //string url = "https://8xg2frkluf.execute-api.ap-northeast-2.amazonaws.com/default/TestServerTime";
        UnityWebRequest request = UnityWebRequest.Get(serverTimeURL);
        yield return request.SendWebRequest();

        //인터넷 연결 오류 혹은 프로토콜 오류
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogError(request.error);
        }
        else {
            string date = request.GetResponseHeader("date");
            Debug.Log(date);

            // 해당 서버 시간에 맞춰 시간 수정 함수 작성 요망******************************************************
            // // 1. 먼저 UTC로 파싱
            DateTime utcTime = DateTime.Parse(date, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
            Debug.Log("UTC time \t" + utcTime);
            
            dateNow = utcTime.AddHours(9);
            Debug.Log("KST time \t" + dateNow);
        }

        ChangeInTime(TimeCheck());
    }

    void ChangeInTime(bool isDay)
    {
        SoundManager.instance.ChangeBgmInTime(isDay);
        //if (isDay) return;

        main_night_ef.SetActive(!isDay);
        night_ef.SetActive(!isDay);
    }

    bool TimeCheck()
    {
        int hour = dateNow.Hour;
        if (6 < hour && hour < 18)
            return true;
        return false;
    }
    #endregion
    
    private void ReadUserJson()
    {
        string loadJson = File.ReadAllText(savePath);
        DataManager.instance.userData = JsonUtility.FromJson<UserData>(loadJson);
    }
    
    public void Login()
    {
        if(PlayerPrefs.HasKey("guestCode"))
        {
            FirebaseInit(isAdmin);
        }
        else
        {
            createUIDBtn.gameObject.SetActive(true);
            createUIDBtn.GetComponent<Button>().onClick.AddListener(ShowUserNamePopup);
            LoginUIDBtn.gameObject.SetActive(true);
            LoginUIDBtn.GetComponent<Button>().onClick.AddListener(ShowLoginPopup);
            // 구글 로그인 버튼으로 이벤트 추가
        }
    }
    bool CheckInternetConnection() // 인터넷 연결 확인
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("인터넷 연결이 없습니다.");
            return false;
        }
        else
        {
            Debug.Log("인터넷 연결이 확인되었습니다.");
            return true;
        }
    }
    
    public void FirebaseInit(bool account_)
    {
        StartCoroutine(FirebaseLoginCoroutine());
        isAdmin = account_;
    }
    IEnumerator QuitApp() // 3초 후 앱 자동 종료
    {
        int count = 5;
        while(count >= 0)
        {
            checkInternetText.text = "인터넷 미연결 감지.\n 인터넷 연결을 확인해주세요!\n\n" + count + "초 후 자동종료";
            count--;
            yield return new WaitForSeconds(1.0f);
        }
        Application.Quit();
    }
    IEnumerator Intro()
    {
        logoPanel.transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        logoPanel.transform.GetChild(0).gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence();
        Image logoImage = logoPanel.transform.GetChild(0).GetComponent<Image>();

        logoImage.color = new Color(1f, 1f, 1f, 0f);
        seq.Append(DOTween.ToAlpha(() => logoImage.color, x => logoImage.color = x, 1f, 1.5f));
        seq.AppendInterval(1.0f);
        seq.Append(DOTween.ToAlpha(() => logoImage.color, x => logoImage.color = x, 0, 0.5f));

        logoPanel.transform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);
        logoPanel.transform.GetChild(0).DOScale(1.05f, 3.0f).SetEase(Ease.Linear);
        
        if(CheckInternetConnection() == true) // 인터넷 연결이 되어 있다면 true
        {
                loadingBar.SetActive(true);
                loadingBar.GetComponent<Slider>().value = 0;
                
                if(!loginComplete) {
                    FolderSettings();
                    FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(dbURL);
                    Login(); // 유저
                    //FirebaseInit(true); // 관리자
                }
        }
        else {
            checkInternetPopup.SetActive(true);
            StartCoroutine(QuitApp());
        }

        yield return new WaitForSeconds(3.5f);
        logoPanel.gameObject.SetActive(false);

        // 게임 제목 서서히 나타나는 부분
        Image gameTitleImage = gameTitle.GetComponent<Image>();
        gameTitleImage.color = new Color(1f, 1f, 1f, 0f);
        DOTween.ToAlpha(() => gameTitleImage.color, x => gameTitleImage.color = x, 1f, 2f).SetEase(Ease.InCubic);

    }

    IEnumerator FirebaseLoginCoroutine()
    {
        yield return null;
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        Task authTask;
        
        // if (PlayerPrefs.HasKey("guestCode")) {
        //     authTask = auth.SignInWithEmailAndPasswordAsync(
        //             PlayerPrefs.GetString("guestCode") + "@forfristu.com", "111111")
        //         .ContinueWithOnMainThread(task => {
        //             if (task.IsCanceled) {
        //                 Debug.LogError("SignInWithCustomTokenAsync was canceled.");
        //                 return;
        //             }
        //
        //             if (task.IsFaulted) {
        //                 Debug.LogError("SignInWithCustomTokenAsync encountered an error: " + task.Exception);
        //                 return;
        //             }
        //
        //             if (task.IsCompletedSuccessfully) {
        //                 DataManager.instance.userData.guestCode = PlayerPrefs.GetString("guestCode");
        //             }
        //         });
        // }
        // else {
        //     authTask = auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
        //         if (task.IsCanceled) {
        //             Debug.LogError("SignInAnonymouslyAsync was canceled.");
        //             return;
        //         }
        //
        //         if (task.IsFaulted) {
        //             Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
        //             return;
        //         }
        //         if (task.IsCompletedSuccessfully) {
        //             string tempGuestCode = task.Result.User.UserId;
        //             auth.CreateUserWithEmailAndPasswordAsync(tempGuestCode + "@forfristu.com", "111111");
        //             PlayerPrefs.SetString("guestCode", tempGuestCode);
        //             ResetData(tempGuestCode);
        //             DataManager.instance.userData.userName = PlayerPrefs.GetString("userName");
        //             PlayerPrefs.DeleteKey("userName");
        //             DataManager.instance.SaveUserData();
        //             WriteUserDB();
        //             guestCode = tempGuestCode;
        //         }
        //     });
        // }
        
        
        authTask = auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
        
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
        
            if (task.IsCompletedSuccessfully) {
                if (isAdmin) {
                    guestCode = "7uIHIcKvbXg9KMWvAZ2p9gFEdrg1"; // 관리자 UID
                    Debug.Log("불러온 관리자 UID : " + guestCode);
                    ResetData(guestCode);
                }
                else {
                    string tempGuestCode;
                    if (PlayerPrefs.HasKey("guestCode")) {
                        tempGuestCode = PlayerPrefs.GetString("guestCode");
                        DataManager.instance.userData.guestCode = tempGuestCode;
                        //ReadUserDB();
                        guestCode = tempGuestCode;
                    }
                    else {
                        tempGuestCode = task.Result.User.UserId;
                        PlayerPrefs.SetString("guestCode", tempGuestCode);
                        ResetData(tempGuestCode);
                        DataManager.instance.userData.userName = PlayerPrefs.GetString("userName");
                        PlayerPrefs.DeleteKey("userName");
                        // string json = JsonUtility.ToJson(DataManager.instance.userData, true);
                        // File.WriteAllText(savePath, json);
                        DataManager.instance.SaveUserData();
                        WriteUserDB();
                        guestCode = tempGuestCode;
                    }
        
                    // if (tempGuestCode == null) {
                    //     AuthResult authResult = task.Result;
                    //     FirebaseUser newUser = authResult.User;
                    //     guestCode = newUser.UserId.ToString();
                    //
                    //     ResetData(guestCode);
                    //     string json = JsonUtility.ToJson(DataManager.instance.userData, true);
                    //     File.WriteAllText(savePath, json);
                    //     WriteUserDB();
                    // }
                    // else {
                    //     DataManager.instance.userData.guestCode = tempGuestCode;
                    //     ReadUserDB();
                    // }
                    //
                    // if (!File.Exists(savePath)) // 로컬 유저 정보 없이 처음 접속
                    // {
                    //     AuthResult authResult = task.Result;
                    //     FirebaseUser newUser = authResult.User;
                    //     guestCode = newUser.UserId.ToString();
                    //
                    //     ResetData(guestCode);
                    //     string json = JsonUtility.ToJson(DataManager.instance.userData, true);
                    //     File.WriteAllText(savePath, json);
                    //     WriteUserDB();
                    // }
                    // else {
                    //     // 기존 로컬 정보 있는 상태로 접속
                    //     string loadJson = File.ReadAllText(savePath);
                    //     DataManager.instance.userData.guestCode = JsonUtility.FromJson<UserData>(loadJson).guestCode;
                    //     //DataManager.instance.userData.userName = JsonUtility.FromJson<UserData>(loadJson).userName;
                    //     guestCode = DataManager.instance.userData.guestCode;
                    //     userName = DataManager.instance.userData.userName;
                    //     ResetData(guestCode, userName);
                    //     Debug.Log("읽어온 유저 UID : " + guestCode);
                    // }
                }
            }
        });
        
        createUIDBtn.gameObject.SetActive(false);
        LoginUIDBtn.gameObject.SetActive(false);
        
        Slider loadingSlider = loadingBar.GetComponent<Slider>();
        loadingText.gameObject.SetActive(true);
        
        loadingText.text = "UID 불러오는 중 ( 1 / 3 )";
        loadingSlider.value = 0.33f;
        yield return new WaitForSeconds(4.0f); // 로고 화면 시간
        yield return new WaitUntil(() => authTask.IsCompletedSuccessfully);
        StartCoroutine(ReadUserDB());
        //ReadUserDB();
        
        loadingText.text = "데이터 동기화 ( 2 / 3 )";
        loadingSlider.value = 0.66f;
        yield return new WaitUntil(() => loginComplete);
        string json = JsonUtility.ToJson(DataManager.instance.userData, true);
        File.WriteAllText(savePath, json);
        
        loadingText.text = "로딩 완료 및 화면 구성 ( 3 / 3 )";
        loadingSlider.value = 1.0f; // 로딩 정상 완료 시점
        yield return new WaitForSeconds(4.0f); // 최소 로딩 시간(더 줄여도 호출 됨)
        
        ChangeScene();
        GoodsManager.instance.UiRefresh();
        MainManager.instance.PlayerInformChange(); // 프로필 정보
        SoundManager.instance.InitSound();
    }
    private void ChangeScene()
    {
        loginCanvas.SetActive(false);
        mainCanvas.SetActive(true);
        UserDataManager.instance.SetCurrentCharacterId();
        if(tutorialGameObject != null) tutorialGameObject.SetActive(true);
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 유저 데이터 초기화
    /// </summary>
    private void ResetData(string guestCode)
    {
        Debug.Log("데이터 초기화 작업 후" + guestCode);
        DataManager.instance.userData = new UserData(guestCode);
    }
    private IEnumerator ReadUserDB() // 유저 데이터 읽어오기
    {
        string userCode = DataManager.instance.userData.guestCode;
        DatabaseReference userReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB/" + userCode);

        // 플레이어, 메인 정보
        var userRefTask = userReference.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                foreach(DataSnapshot data in snapshot.Children) {
                    // 각 데이터에 접근
                    string key = data.Key;
                    string value = data.Value.ToString();

                    // 디버깅 로그 추가
                    //Debug.Log($"키: {key}, 값: {value}");
                    try {
                        if (key == "loginDate") DataManager.instance.userData.loginDate = int.Parse(value);
                        else if (key == "rankingScore") DataManager.instance.userData.rankingScore = int.Parse(value);
                        else if (key == "userIcon") DataManager.instance.userData.userIcon = int.Parse(value);
                        else if (key == "userId") DataManager.instance.userData.userId = value;
                        else if (key == "userName") DataManager.instance.userData.userName = value;
                        else if (key == "guestCode") DataManager.instance.userData.guestCode = value;
                        else if (key == "endTimeStr") DataManager.instance.userData.endTimeStr = value;
                    } catch (Exception e) {
                        Debug.LogError($"에러난 파싱키 {key}: {e.Message}");
                        DataManager.instance.userData.InitParameter(key);
                    }
                }
                Debug.Log("0");
            }
        });
        yield return new WaitUntil(() => userRefTask.IsCompleted);
        userRefTask.Dispose();
        
        // 장비 리스트
        DatabaseReference equipmentReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB/" + userCode + "/equipmentSpecificList");
        userRefTask = equipmentReference.GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataManager.instance.userData.equipmentSpecificList.Clear();
                DataSnapshot snapshot = task.Result;
                
                EquipmentSpecific[] equipSpecList = new EquipmentSpecific[snapshot.ChildrenCount];
                foreach (DataSnapshot equipmentData in snapshot.Children) {
                    int id = int.Parse(equipmentData.Child("id").Value.ToString());
                    int type = int.Parse(equipmentData.Child("type").Value.ToString());
                    int grade = int.Parse(equipmentData.Child("grade").Value.ToString());
                    int level = int.Parse(equipmentData.Child("level").Value.ToString());
                    
                    EquipmentSpecific equipmentSpecific = new EquipmentSpecific(id, type, grade, level); // 새로운 EquipmentSpecific 객체 생성
                    equipSpecList[int.Parse(equipmentData.Key)] = equipmentSpecific;
                    //DataManager.instance.userData.equipmentSpecificList.Add(equipmentSpecific); // UserData의 equipmentSpecificList 리스트에 추가
                }
                DataManager.instance.userData.equipmentSpecificList = equipSpecList.ToList();
                Debug.Log("1");
            } else {
                Debug.LogError("데이터를 불러오는 데 실패했습니다.");
            }
        });
        yield return new WaitUntil(() => userRefTask.IsCompleted);
        userRefTask.Dispose();
        
        // 유저 재화, 보유 현황 등
        userRefTask = userReference.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                foreach(DataSnapshot data in snapshot.Children) {
                    // 각 데이터에 접근
                    string key = data.Key;
                    string value = data.Value.ToString();
                    try {
                        if (key == "gold") DataManager.instance.userData.Gold = int.Parse(value);
                        else if (key == "diamond") DataManager.instance.userData.Diamond = int.Parse(value);
                        else if (key == "life") DataManager.instance.userData.Life = int.Parse(value);
                        else if (key == "currentCharacterId") DataManager.instance.userData.CurrentCharacterId = int.Parse(value);
                        else if (key == "currentWeaponId") DataManager.instance.userData.CurrentWeaponId = int.Parse(value);
                        else if (key == "currentShoesId") DataManager.instance.userData.currentShoesId = int.Parse(value);
                        else if (key == "currentAccessoryId") DataManager.instance.userData.currentAccessoryId = int.Parse(value);
                        else if (key == "currentHatId") DataManager.instance.userData.currentHatId = int.Parse(value);
                        else if (key == "currentCoatId") DataManager.instance.userData.currentCoatId = int.Parse(value);
                    } catch (Exception e) {
                        Debug.LogError($"에러난 파싱키 {key}: {e.Message}");
                        DataManager.instance.userData.InitParameter(key);
                    }
                }
                Debug.Log("2");
            }
        });
        yield return new WaitUntil(() => userRefTask.IsCompleted);
        userRefTask.Dispose();
        
        // 캐릭터, 아이템 보유 정보
        userRefTask = userReference.GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                foreach(DataSnapshot data in snapshot.Children) {
                    // 각 데이터에 접근
                    string key = data.Key;
                    //string value = data.Value.ToString();
                    try {
                        if (key == "characterList") {
                            DataManager.instance.userData.characterList.Clear();
                            foreach(DataSnapshot characterData in data.Children) {
                                int characterId = int.Parse(characterData.Value.ToString());
                                DataManager.instance.userData.characterList.Add(characterId);
                            }
                        }
                        else if (key == "mainQuestClear") {
                            DataManager.instance.userData.mainQuestClear.Clear();
                            foreach(DataSnapshot itemData in data.Children) {
                                bool itemId = Convert.ToBoolean(itemData.Value);
                                DataManager.instance.userData.mainQuestClear.Add(itemId);
                            }
                        }
                        else if (key == "dailyQuestList") { // 일일 미션 목록 확인(25.02.18)
                            DataManager.instance.userData.dailyQuestList.Clear();
                            foreach(DataSnapshot itemData in data.Children) {
                                // int itemId = int.Parse(itemData.Value.ToString());
                                // DataManager.instance.userData.dailyQuestList.Add(itemId);
                                int mission;
                                int current;
                                if (int.TryParse(itemData.Value.ToString(), out mission)) {
                                    current = 0;
                                }
                                else {
                                    mission = int.Parse(itemData.Child("missionNum").Value.ToString());
                                    current = int.Parse(itemData.Child("currentNum").Value.ToString());
                                }
                                
                                DataManager.instance.userData.dailyQuestList.Add(
                                    new Mission(mission, current));
                            }
                        }
                        else if (key == "dailyQuestClear") {
                            DataManager.instance.userData.dailyQuestClear.Clear();
                            foreach(DataSnapshot itemData in data.Children) {
                                //int itemId = int.Parse(itemData.Value.ToString());
                                bool itemId = Convert.ToBoolean(itemData.Value);
                                DataManager.instance.userData.dailyQuestClear.Add(itemId);
                            }
                        }
                        else if (key == "purchaseCheck") {
                            DataManager.instance.userData.purchaseCheck.Clear();
                            foreach(DataSnapshot itemData in data.Children) {
                                //int itemId = int.Parse(itemData.Value.ToString());
                                bool itemId = Convert.ToBoolean(itemData.Value);
                                DataManager.instance.userData.purchaseCheck.Add(itemId);
                            }
                        }
                        else if (key == "stageClearCheck") {
                            DataManager.instance.userData.stageClearCheck.Clear();
                            foreach(DataSnapshot itemData in data.Children) {
                                //기존 bool을 int로 바꾸면서 생기는 문제를 대응
                                //bool itemId = Convert.ToBoolean(itemData.Value);
                                //int itemId = int.Parse(itemData.Value.ToString());
                                int itemId;
                                if (int.TryParse(itemData.Value.ToString(), out itemId)) {
                                    itemId = int.Parse(itemData.Value.ToString());
                                }
                                else {
                                    itemId = Convert.ToBoolean(itemData.Value) ? 1 : 0;
                                }
                                DataManager.instance.userData.stageClearCheck.Add(itemId);
                            }
                        }
                        else if (key == "enemyClearCheck") {
                            DataManager.instance.userData.enemyClearCheck.Clear();
                            foreach (DataSnapshot itemData in data.Children) {
                                var itemId = int.Parse(itemData.Value.ToString());
                                DataManager.instance.userData.enemyClearCheck.Add(itemId);
                            }
                        }
                        else if (key == "mailRead") {
                            DataManager.instance.userData.mailRead.Clear();
                            foreach (DataSnapshot itemData in data.Children) {
                                var itemId = int.Parse(itemData.Value.ToString());
                                DataManager.instance.userData.mailRead.Add(itemId);
                            }
                        }
                        else if (key == "levelUpMaterial") {
                            DataManager.instance.userData.levelUpMaterial.Clear();
                            for (int i = 0; i < 5; i++) {
                                DataManager.instance.userData.levelUpMaterial.Add(0);
                            }
                            foreach(DataSnapshot itemData in data.Children) {
                                int index = int.Parse(itemData.Key);
                                if (index >= 5) continue; // levelUpMaterial은 5개
                                int itemId = int.Parse(itemData.Value.ToString());
                                DataManager.instance.userData.levelUpMaterial[index] = itemId;
                            }
                        }
                    } catch (Exception e) {
                        Debug.LogError($"에러난 파싱키 {key}: {e.Message}");
                        DataManager.instance.userData.InitParameter(key);
                    }
                }
                
                //새로운 변수가 추가되었을 때
                if (snapshot.HasChild("mainQuestClear") == false) {
                    DataManager.instance.userData.InitParameter("mainQuestClear");
                }

                if (snapshot.HasChild("levelUpMaterial") == false) {
                    DataManager.instance.userData.InitParameter("levelUpMaterial");
                }
                Debug.Log("3");
            }
        });
        yield return new WaitUntil(() => userRefTask.IsCompleted);
        userRefTask.Dispose();
        
        // 캐릭터 정보 리스트
        DatabaseReference characterReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB/" + userCode + "/characterInforms");
        userRefTask = characterReference.GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataManager.instance.userData.characterInforms.Clear();
                DataSnapshot snapshot = task.Result;
                foreach (DataSnapshot characterData in snapshot.Children) {
                    // "level"과 "experience" 값을 각각 가져오기
                    int id = int.Parse(characterData.Child("id").Value.ToString());
                    int level = int.Parse(characterData.Child("level").Value.ToString());
                    int experience = int.Parse(characterData.Child("experience").Value.ToString());

                    // "characterSkillList"에 접근
                    DataSnapshot characterSkillList = characterData.Child("characterSkillList");

                    // 리스트 형태의 데이터를 처리
                    List<int> characterSkill = new List<int>();
                    foreach (DataSnapshot listItem in characterSkillList.Children) {
                        int value = int.Parse(listItem.Value.ToString());
                        characterSkill.Add(value);
                    }
                    // 새로운 CharacterSpecific 객체 생성
                    CharacterSpecific characterSpecific = new CharacterSpecific(id, level, experience, characterSkill);

                    // UserData의 characterInforms 리스트에 추가
                    DataManager.instance.userData.characterInforms.Add(characterSpecific);
                }
                Debug.Log("4");
            } else {
                Debug.LogError("데이터를 불러오는 데 실패했습니다.");
            }
        });
        yield return new WaitUntil(() => userRefTask.IsCompleted);
        userRefTask.Dispose();
        
        /*
        // 장비 리스트
        DatabaseReference equipmentReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB/" + userCode + "/equipmentSpecificList");
        task = equipmentReference.GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataManager.instance.userData.equipmentSpecificList.Clear();
                DataSnapshot snapshot = task.Result;
                
                EquipmentSpecific[] equipSpecList = new EquipmentSpecific[snapshot.ChildrenCount];
                foreach (DataSnapshot equipmentData in snapshot.Children) {
                    int id = int.Parse(equipmentData.Child("id").Value.ToString());
                    int type = int.Parse(equipmentData.Child("type").Value.ToString());
                    int grade = int.Parse(equipmentData.Child("grade").Value.ToString());
                    int level = int.Parse(equipmentData.Child("level").Value.ToString());
                    
                    EquipmentSpecific equipmentSpecific = new EquipmentSpecific(id, type, grade, level); // 새로운 EquipmentSpecific 객체 생성
                    equipSpecList[int.Parse(equipmentData.Key)] = equipmentSpecific;
                    //DataManager.instance.userData.equipmentSpecificList.Add(equipmentSpecific); // UserData의 equipmentSpecificList 리스트에 추가
                }
                DataManager.instance.userData.equipmentSpecificList = equipSpecList.ToList();
                isLoadList[4] = true;
                Debug.Log("4");
            } else {
                Debug.LogError("데이터를 불러오는 데 실패했습니다.");
            }
        });
        yield return new WaitUntil(() => task.IsCompleted);
        */
        
        // 아이템 리스트
        DatabaseReference itemReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB/" + userCode + "/itemList");
        userRefTask = itemReference.GetValueAsync().ContinueWithOnMainThread(task => { 
            if (task.IsCanceled) {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }
            if (task.IsCompleted) {
                DataManager.instance.userData.itemList.Clear();
                DataSnapshot snapshot = task.Result;

                List<ConsumeItem> itemList = new List<ConsumeItem>();
                foreach (DataSnapshot itemData in snapshot.Children) {
                    ItemType itemType = (ItemType)Enum.Parse(typeof(ItemType), itemData.Child("itemType").Value.ToString());
                    int amount = int.Parse(itemData.Child("amount").Value.ToString());
                    itemList.Add(new ConsumeItem(itemType, amount));
                }
                DataManager.instance.userData.itemList = itemList.ToList();
                Debug.Log("5");
            } else {
                Debug.LogError("데이터를 불러오는 데 실패했습니다.");
            }
        });
        yield return new WaitUntil(() => userRefTask.IsCompleted);
        userRefTask.Dispose();
        
        // 백업파일과 비교 후 일치할 경우 완료 처리 추가
        //yield return null; // 왜 여기에 이걸 넣으면 itemList가 제때 안 들어가는지 이유를 모르겠다.
        loginComplete = true;
        yield return null; // 왜 여기에 이걸 넣어야 itemList가 제때 들어가는지 이유를 모르겠다.
        AfterLoad();
        //StartCoroutine(UpdateAllData());
    }

    /// <summary>
    /// 데이터가 모두 로드 되었을 때 호출
    /// </summary>
    private void AfterLoad()
    {
        var components = mainManager.GetComponentsInChildren<MonoBehaviour>();
        foreach (var component in components) {
            component.enabled = true;
        }
        
        InventoryManager.instance.InventoryOpen();
        
        PediaManager.instance.CharacterPossession();
        PediaManager.instance.CharacterData(DataManager.instance.userData.CurrentCharacterId); // 현재 장착 된 캐릭터로 불러오기
        PediaManager.instance.SelectCharacter(DataManager.instance.userData.CurrentCharacterId, false);

        PediaManager.instance.EnemyPossession();
    }
    
    public void WriteUserDB() { // 유저 데이터 전체
        DatabaseReference userDB = FirebaseDatabase.DefaultInstance.RootReference.Child("UserDB")
            .Child(DataManager.instance.userData.guestCode);
        string jsonUserData = File.ReadAllText(savePath);
        Debug.Log("유저 데이터 쓰기 실행");
        userDB.SetRawJsonValueAsync(jsonUserData);
        userDB.Child("TimeStamp").SetValueAsync(ServerValue.Timestamp);

        DatabaseReference databaseRef;
        // equipmentSpecificList
        databaseRef = userDB.Child("equipmentSpecificList");
        var equipmentSpecificList = DataManager.instance.userData.equipmentSpecificList;
        for (int i = 0; i < equipmentSpecificList.Count; i++)
        {
            databaseRef.Child(i.ToString()).Child("id").SetValueAsync(equipmentSpecificList[i].id);
            databaseRef.Child(i.ToString()).Child("type").SetValueAsync(equipmentSpecificList[i].type);
            databaseRef.Child(i.ToString()).Child("grade").SetValueAsync(equipmentSpecificList[i].grade);
            databaseRef.Child(i.ToString()).Child("level").SetValueAsync(equipmentSpecificList[i].level);
        }
        
        // itemList
        // databaseRef = userDB.Child("itemList");
        // var itemList = DataManager.instance.userData.itemList;
        // for (int i = 0; i < itemList.Count; i++) {
        //     databaseRef.Child(i.ToString()).Child("itemType").SetValueAsync(itemList[i].itemType.ToString());
        //     databaseRef.Child(i.ToString()).Child("amount").SetValueAsync(itemList[i].amount);
        // }
    }

    public void WriteUserParameter(string parameterName)
    {
        DatabaseReference userDB = FirebaseDatabase.DefaultInstance.RootReference.Child("UserDB")
            .Child(DataManager.instance.userData.guestCode);
        DatabaseReference databaseRef;
        Dictionary<string, object> updates = new Dictionary<string, object>();
        
        switch (parameterName) {
            case "userName":
                updates["userName"] = DataManager.instance.userData.userName;
                break;
            case "endTimeStr":
                updates["endTimeStr"] = DataManager.instance.userData.endTimeStr;
                break;
            case "isFirstLogin":
                updates["isFirstLogin"] = DataManager.instance.userData.isFirstLogin;
                break;
            case "todayReceive":
                updates["todayReceive"] = DataManager.instance.userData.todayReceive;
                break;
            case "userIcon":
                updates["userIcon"] = DataManager.instance.userData.userIcon;
                break;
            case "loginDate":
                updates["loginDate"] = DataManager.instance.userData.loginDate;
                break;
            case "passPoint":
                updates["passPoint"] = DataManager.instance.userData.passPoint;
                break;
            case "rankingScore":
                updates["rankingScore"] = DataManager.instance.userData.rankingScore;
                break;
            case "purchaseCheck":
                var purchaseCheck = DataManager.instance.userData.purchaseCheck;
                for (int i = 0; i < purchaseCheck.Count; i++) {
                    updates["purchaseCheck/" + i] = purchaseCheck[i];
                }
                break;
            case "stageClearCheck":
                var stageClearCheck = DataManager.instance.userData.stageClearCheck;
                for (int i = 0; i < stageClearCheck.Count; i++) {
                    updates["stageClearCheck/" + i] = stageClearCheck[i];
                }
                break;
            case "enemyClearCheck":
                var enemyClearCheck = DataManager.instance.userData.enemyClearCheck;
                for (int i = 0; i < enemyClearCheck.Count; i++) {
                    updates["enemyClearCheck/" + i] = enemyClearCheck[i];
                }
                break;
            case "mailRead":
                var mailRead = DataManager.instance.userData.mailRead;
                for (int i = 0; i < mailRead.Count; i++) {
                    updates["mailRead/" + i] = mailRead[i];
                }
                break;
            
            case "mainQuestClear":
                var mainQuestClear = DataManager.instance.userData.mainQuestClear;
                for (int i = 0; i < mainQuestClear.Count; i++) {
                    updates["mainQuestClear/" + i] = mainQuestClear[i];
                }
                break;
            case "dailyQuestList":
                var dailyQuestList = DataManager.instance.userData.dailyQuestList;
                for (int i = 0; i < dailyQuestList.Count; i++) {
                    updates["dailyQuestList/" + i] = dailyQuestList[i];
                }
                break;
            case "dailyQuestClear":
                var dailyQuestClear = DataManager.instance.userData.dailyQuestClear;
                for (int i = 0; i < dailyQuestClear.Count; i++) {
                    updates["dailyQuestClear/" + i] = dailyQuestClear[i];
                }
                break;
            
            case "gold":
                updates["gold"] = DataManager.instance.userData.Gold;
                break;
            case "diamond":
                updates["diamond"] = DataManager.instance.userData.Diamond;
                break;
            case "life":
                updates["life"] = DataManager.instance.userData.Life;
                break;
            case "levelUpMaterial":
                var levelUpMaterial = DataManager.instance.userData.levelUpMaterial;
                for (int i = 0; i < levelUpMaterial.Count; i++) {
                    updates["levelUpMaterial/" + i] = levelUpMaterial[i];
                }
                break;
            
            case "itemList":
                var itemList = DataManager.instance.userData.itemList;
                for (int i = 0; i < itemList.Count; i++) {
                    updates["itemList/" + i + "/itemType"] = itemList[i].itemType;
                    updates["itemList/" + i + "/amount"] = itemList[i].amount;
                }
                break;
            
            case "characterList":
                var characterList = DataManager.instance.userData.characterList;
                for (int i = 0; i < characterList.Count; i++) {
                    updates["characterList/" + i] = characterList[i];
                }
                break;
            case "characterInforms":
                var characterInforms = DataManager.instance.userData.characterInforms;
                for (int i = 0; i < characterInforms.Count; i++) {
                    updates["characterInforms/" + i + "/id"] = characterInforms[i].id;
                    updates["characterInforms/" + i + "/level"] = characterInforms[i].level;
                    updates["characterInforms/" + i + "/experience"] = characterInforms[i].experience;
                    for (int j = 0; j < characterInforms[i].characterSkillList.Count; j++) {
                        updates["characterInforms/" + i + "/characterSkillList/" + j] =
                            characterInforms[i].characterSkillList[j];
                    }
                }
                break;
            case "currentCharacterId":
                updates["currentCharacterId"] = DataManager.instance.userData.CurrentCharacterId;
                break;
            
            case "equipmentSpecificList":
                databaseRef = userDB.Child("equipmentSpecificList");
                databaseRef.RemoveValueAsync();
                var equipmentSpecificList = DataManager.instance.userData.equipmentSpecificList;
                for (int i = 0; i < equipmentSpecificList.Count; i++) {
                    updates["equipmentSpecificList/" + i + "/id"] = equipmentSpecificList[i].id;
                    updates["equipmentSpecificList/" + i + "/type"] = equipmentSpecificList[i].type;
                    updates["equipmentSpecificList/" + i + "/grade"] = equipmentSpecificList[i].grade;
                    updates["equipmentSpecificList/" + i + "/level"] = equipmentSpecificList[i].level;
                }
                break;
            
            case "currentWeaponId":
                updates["currentWeaponId"] = DataManager.instance.userData.CurrentWeaponId;
                break;
            case "currentHatId":
                updates["currentHatId"] = DataManager.instance.userData.currentHatId;
                break;
            case "currentAccessoryId":
                updates["currentAccessoryId"] = DataManager.instance.userData.currentAccessoryId;
                break;
            case "currentShoesId":
                updates["currentShoesId"] = DataManager.instance.userData.currentShoesId;
                break;
            case "currentCoatId":
                updates["currentCoatId"] = DataManager.instance.userData.currentCoatId;
                break;
            default:
                Debug.LogError(parameterName + "에 맞는 변수가 없습니다.");
                break;
        }
        
        userDB.UpdateChildrenAsync(updates);
    }
    
    public void WriteUserParameters(string[] parameterNames)
    {
        DatabaseReference userDB = FirebaseDatabase.DefaultInstance.RootReference.Child("UserDB")
            .Child(DataManager.instance.userData.guestCode);
        DatabaseReference databaseRef;
        Dictionary<string, object> updates = new Dictionary<string, object>();
        
        foreach (var parameterName in parameterNames) {
            switch (parameterName) {
                case "userName":
                    updates["userName"] = DataManager.instance.userData.userName;
                    break;
                case "endTimeStr":
                    updates["endTimeStr"] = DataManager.instance.userData.endTimeStr;
                    break;
                case "isFirstLogin":
                    updates["isFirstLogin"] = DataManager.instance.userData.isFirstLogin;
                    break;
                case "todayReceive":
                    updates["todayReceive"] = DataManager.instance.userData.todayReceive;
                    break;
                case "userIcon":
                    updates["userIcon"] = DataManager.instance.userData.userIcon;
                    break;
                case "loginDate":
                    updates["loginDate"] = DataManager.instance.userData.loginDate;
                    break;
                case "passPoint":
                    updates["passPoint"] = DataManager.instance.userData.passPoint;
                    break;
                case "rankingScore":
                    updates["rankingScore"] = DataManager.instance.userData.rankingScore;
                    break;
                case "purchaseCheck":
                    var purchaseCheck = DataManager.instance.userData.purchaseCheck;
                    for (int i = 0; i < purchaseCheck.Count; i++) {
                        updates["purchaseCheck/" + i] = purchaseCheck[i];
                    }

                    break;
                case "stageClearCheck":
                    var stageClearCheck = DataManager.instance.userData.stageClearCheck;
                    for (int i = 0; i < stageClearCheck.Count; i++) {
                        updates["stageClearCheck/" + i] = stageClearCheck[i];
                    }

                    break;
                case "enemyClearCheck":
                    var enemyClearCheck = DataManager.instance.userData.enemyClearCheck;
                    for (int i = 0; i < enemyClearCheck.Count; i++) {
                        updates["enemyClearCheck/" + i] = enemyClearCheck[i];
                    }

                    break;
                case "mailRead":
                    var mailRead = DataManager.instance.userData.mailRead;
                    for (int i = 0; i < mailRead.Count; i++) {
                        updates["mailRead/" + i] = mailRead[i];
                    }

                    break;

                case "mainQuestClear":
                    var mainQuestClear = DataManager.instance.userData.mainQuestClear;
                    for (int i = 0; i < mainQuestClear.Count; i++) {
                        updates["mainQuestClear/" + i] = mainQuestClear[i];
                    }

                    break;
                case "dailyQuestList":
                    var dailyQuestList = DataManager.instance.userData.dailyQuestList;
                    for (int i = 0; i < dailyQuestList.Count; i++) {
                        updates["dailyQuestList/" + i] = dailyQuestList[i];
                    }

                    break;
                case "dailyQuestClear":
                    var dailyQuestClear = DataManager.instance.userData.dailyQuestClear;
                    for (int i = 0; i < dailyQuestClear.Count; i++) {
                        updates["dailyQuestClear/" + i] = dailyQuestClear[i];
                    }

                    break;

                case "gold":
                    updates["gold"] = DataManager.instance.userData.Gold;
                    break;
                case "diamond":
                    updates["diamond"] = DataManager.instance.userData.Diamond;
                    break;
                case "life":
                    updates["life"] = DataManager.instance.userData.Life;
                    break;
                case "levelUpMaterial":
                    var levelUpMaterial = DataManager.instance.userData.levelUpMaterial;
                    for (int i = 0; i < levelUpMaterial.Count; i++) {
                        updates["levelUpMaterial/" + i] = levelUpMaterial[i];
                    }

                    break;

                case "itemList":
                    var itemList = DataManager.instance.userData.itemList;
                    for (int i = 0; i < itemList.Count; i++) {
                        updates["itemList/" + i + "/itemType"] = itemList[i].itemType;
                        updates["itemList/" + i + "/amount"] = itemList[i].amount;
                    }

                    break;

                case "characterList":
                    var characterList = DataManager.instance.userData.characterList;
                    for (int i = 0; i < characterList.Count; i++) {
                        updates["characterList/" + i] = characterList[i];
                    }

                    break;
                case "characterInforms":
                    var characterInforms = DataManager.instance.userData.characterInforms;
                    for (int i = 0; i < characterInforms.Count; i++) {
                        updates["characterInforms/" + i + "/id"] = characterInforms[i].id;
                        updates["characterInforms/" + i + "/level"] = characterInforms[i].level;
                        updates["characterInforms/" + i + "/experience"] = characterInforms[i].experience;
                        for (int j = 0; j < characterInforms[i].characterSkillList.Count; j++) {
                            updates["characterInforms/" + i + "/characterSkillList/" + j] =
                                characterInforms[i].characterSkillList[j];
                        }
                    }

                    break;
                case "currentCharacterId":
                    updates["currentCharacterId"] = DataManager.instance.userData.CurrentCharacterId;
                    break;

                case "equipmentSpecificList":
                    databaseRef = userDB.Child("equipmentSpecificList");
                    databaseRef.RemoveValueAsync();
                    var equipmentSpecificList = DataManager.instance.userData.equipmentSpecificList;
                    for (int i = 0; i < equipmentSpecificList.Count; i++) {
                        updates["equipmentSpecificList/" + i + "/id"] = equipmentSpecificList[i].id;
                        updates["equipmentSpecificList/" + i + "/type"] = equipmentSpecificList[i].type;
                        updates["equipmentSpecificList/" + i + "/grade"] = equipmentSpecificList[i].grade;
                        updates["equipmentSpecificList/" + i + "/level"] = equipmentSpecificList[i].level;
                    }

                    break;

                case "currentWeaponId":
                    updates["currentWeaponId"] = DataManager.instance.userData.CurrentWeaponId;
                    break;
                case "currentHatId":
                    updates["currentHatId"] = DataManager.instance.userData.currentHatId;
                    break;
                case "currentAccessoryId":
                    updates["currentAccessoryId"] = DataManager.instance.userData.currentAccessoryId;
                    break;
                case "currentShoesId":
                    updates["currentShoesId"] = DataManager.instance.userData.currentShoesId;
                    break;
                case "currentCoatId":
                    updates["currentCoatId"] = DataManager.instance.userData.currentCoatId;
                    break;
                default:
                    Debug.LogError(parameterName + "에 맞는 변수가 없습니다.");
                    break;
            }
        }

        userDB.UpdateChildrenAsync(updates);
    }
    private void FolderSettings() { // 플랫폼을 체크하여 경로 지정
        savePath = Path.Combine(Application.dataPath, "Resources/GameData/UserData.json");

        if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
            savePath = Path.Combine(Application.persistentDataPath + "/UserData.json");
        }
    }

    #region UID
    
    private void ShowUserNamePopup()
    {
        userNamePopup.gameObject.SetActive(true);
        //createUIDBtn.gameObject.SetActive(false);
    }
    public void CloseUserNamePopup()
    {
        userNamePopup.gameObject.SetActive(false);
        nameCheckPopup.gameObject.SetActive(false);
    }
    
    public void CreateUserName() // 1
    {
        StartCoroutine(IsValidInput(userNameInput.text));
    }
    IEnumerator IsValidInput(string input) // 2
    {
        totalLength = 0;
        //userName = input;
        informText.text = "확인 중입니다!";
        nameCheckBtn.interactable = false;
        bool isOverlap = CheckUserNameDB(input); // 중복되는 이름인지도 확인하기
        yield return new WaitForSeconds(2.0f);
        foreach (char c in input)
        {
            if ((c >= '가' && c <= '힣'))
            {
                    totalLength += 2; // 한글은 2글자로 계산
            }
            else
            {
                totalLength += 1; // 영문 및 기타 문자는 1글자로 계산
            }
        }
        if (totalLength > 2 && totalLength <= 16 && !isOverlap) {
            nameCheckPopup.SetActive(true);
            informText.text = "사용 가능한 이름입니다.";
            nameCheckText.text = input;
        }
        else {
            Debug.Log("사용 할 수 없는 이름 : " + input);
            if(!(totalLength > 2 && totalLength <= 16)) informText.text = "글자 수를 확인해주세요!";
            else if(isOverlap) informText.text = "이미 사용 중인 이름입니다!";
            nameCheckBtn.interactable = true;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns>겹칠 시 true 반환</returns>
    private bool CheckUserNameDB(string input) // 3
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("UserNameList/Names");
        dataReference.GetValueAsync().ContinueWithOnMainThread(task => 
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // UserNameList에 대한 모든 데이터를 가져온 후
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    // 각 데이터에 접근
                    string key = childSnapshot.Key;
                    string value = childSnapshot.Value.ToString();
                    // 예시로 0번째 데이터의 key와 value를 출력
                    if (value == input)
                    {
                        Debug.Log("겹침 감지");
                        return true;
                    }
                }
            }
            return false;
        });
        return false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns>UID가 존재할 시 true 반환</returns>
    private bool CheckUID(string input)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB");
        dataReference.GetValueAsync().ContinueWithOnMainThread(task => 
        {
            if(task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                // UserDB의 대한 모든 데이터를 가져온 후
                foreach (DataSnapshot childSnapshot in snapshot.Children)
                {
                    // 각 데이터에 접근
                    string key = childSnapshot.Key;
                    string value = childSnapshot.Value.ToString();
                    // 예시로 0번째 데이터의 key와 value를 출력
                    if (value == input)
                    {
                        Debug.Log("UID 존재");
                        return true;
                    }
                }
            }
            return false;
        });
        return false;
    }
    public void UIDCancelBtn() // 취소 버튼
    {
        nameCheckPopup.SetActive(false);
        nameCheckBtn.interactable = true;
    }
    public void UIDCompleteBtn() // 결정 버튼
    {
        //임시로 저장
        PlayerPrefs.SetString("userName", userNameInput.text);
        DataManager.instance.userData.userName = userNameInput.text;
        FirebaseInit(isAdmin); // false: 유저, true: 관리자
        WriteUserNameDB(); // 네임리스트에 쓰기
        userNamePopup.SetActive(false);
    }
    private void WriteUserNameDB() { // 유저 이름 리스트
        DatabaseReference userNameDB =
            FirebaseDatabase.DefaultInstance.RootReference.Child("UserNameList").Child("Names");
        string jsonUserName = DataManager.instance.userData.userName;
        Debug.Log("유저네임 등록 실행");
        userNameDB.Push().SetValueAsync(jsonUserName);
    }

    #endregion

    #region Login

    public async Task<bool> CheckUidDB(string uid) // UID 유효성 검사
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB");

        try
        {
            DataSnapshot snapshot = await dataReference.GetValueAsync(); // Firebase 데이터 조회
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string key = childSnapshot.Key;
                
                if (key == uid)
                {
                    Debug.Log("존재하는 UID 확인");
                    return true; // UID 존재
                }
            }

            Debug.Log("존재하지 않는 UID");
            return false; // UID 없음
        }
        catch (Exception e)
        {
            Debug.LogError("UID 확인 중 오류 발생: " + e.Message);
            return false; // 에러 발생 시 UID 없음으로 처리
        }
    }

    public async Task<string> LoadUserNameByUID(string uid)
    {
        DatabaseReference dataReference = FirebaseDatabase.DefaultInstance.GetReference("UserDB/" + uid + "/userName");

        try
        {
            DataSnapshot snapshot = await dataReference.GetValueAsync(); // Firebase 데이터 조회

            if (snapshot.Exists) {
                Debug.Log("존재하는 UID 확인");
                return snapshot.Value.ToString();
            }
            else {
                Debug.Log("존재하지 않는 UID");
                return null; // UID 없음
            }
        }
        catch (Exception e)
        {
            Debug.LogError("UID 확인 중 오류 발생: " + e.Message);
            return null; // 에러 발생 시 UID 없음으로 처리
        }
    }

    private void ShowLoginPopup()
    {
        loginPopup.SetActive(true);
    }

    public void CloseLoginPopup()
    {
        loginPopup.SetActive(false);
    }

    public async void LoginUID()
    {
        bool isUidValid = await CheckUidDB(UIDInput.text);
        
        if (!isUidValid)
        {
            UIDInput.placeholder.GetComponent<Text>().text = "UID를 다시 확인해주세요!";
            Debug.LogWarning("유효하지 않은 UID 입력. 절차 중단.");
            UIDInput.SetTextWithoutNotify(""); // 값 초기화 반영
            return;
        }

        loginComplete = false;
        PlayerPrefs.SetString("guestCode", UIDInput.text);
        
        ResetData(UIDInput.text);

        DataManager.instance.userData.userName = await LoadUserNameByUID(UIDInput.text);
        DataManager.instance.SaveUserData();
        WriteUserDB();
        
        UIDInput.SetTextWithoutNotify("");
        //LoginInit();
        _SceneManager.instance.MainMenu();
    }

    #endregion
}