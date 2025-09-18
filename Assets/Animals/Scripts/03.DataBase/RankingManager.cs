using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using System;
using System.Collections;
using Firebase.Extensions;
using Yejun.UGUI;

public class RankingManager : MonoBehaviour {
    public static RankingManager instance;
    public string dbURL = "https://animalsshooting-7850d-default-rtdb.firebaseio.com/";
    public Sprite[] top3Image;
    public List<UserRanking> rankingData = new List<UserRanking>();
    public Transform myRankingBar;
    [SerializeField] private InfiniteScrollRect infiniteScrollRect;
    public Transform myRankingGameBar;
    [SerializeField] private InfiniteScrollRect infiniteScrollRectRanking;

    public int myRanking;
    public string myName;
    public int myIcon;
    public int myScore;
    public int currentPage = 0; // 현재 페이지
    const int PAGE_SIZE = 30; // 한 페이지에 가져올 데이터 수

    private DatabaseReference dataReference;

    public enum LOADTYPE {
        NONE = 0,
        LOADING,
        LOADED
    }
    [HideInInspector] public LOADTYPE isMyRankingLoad = LOADTYPE.NONE;
    [HideInInspector] public LOADTYPE isMyRankLoad = LOADTYPE.NONE;
    [HideInInspector] public LOADTYPE isRankingLoad = LOADTYPE.NONE;

    // SetRank가 호출된 인덱스를 저장할 HashSet 추가
    private HashSet<int> loadedPageIndices = new HashSet<int>();

    void Awake() {
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(dbURL);
        instance = this;

        infiniteScrollRect.onVerifyIndex += index => {
            // index가 rankingData.Count와 같고, 이 인덱스에 대해 SetRank가 아직 호출되지 않았다면
            if (index == rankingData.Count && !loadedPageIndices.Contains(index)) {
                SetRank(); // SetRank 호출
                loadedPageIndices.Add(index); // 호출된 인덱스를 기록
            }
            return 0 <= index && index < rankingData.Count;
        };
        infiniteScrollRectRanking.onVerifyIndex += index => {
            if (index == rankingData.Count && !loadedPageIndices.Contains(index)) {
                SetRank(); // SetRank 호출
                loadedPageIndices.Add(index); // 호출된 인덱스를 기록
            }
            return 0 <= index && index < rankingData.Count;
        };
    }
    void Start()
    {
        dataReference = FirebaseDatabase.DefaultInstance.GetReference("Ranking/LeaderBoard/userRankings");
    }

    public void NewRecord(string guestCode, string userName, int userIcon, int userScore) {
        dataReference.OrderByChild("guestCode").EqualTo(DataManager.instance.userData.guestCode).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.HasChildren)
                {
                    var data = snapshot.Children.First();
                    
                    if (userScore > -int.Parse(data.Child("userScore").Value.ToString()))
                    {
                        // Firebase에 저장, 점수는 음수로 저장
                        dataReference.Child(guestCode)
                            .SetRawJsonValueAsync(
                                JsonUtility.ToJson(new UserRanking(guestCode, userName, userIcon, -userScore)));
                    }
                }
            }
        });
    }

    // private void WriteRankingDB(UserRanking userRanking) {
    //     // Firebase에 저장
    //     dataReference.Child(userRanking.guestCode).SetRawJsonValueAsync(JsonUtility.ToJson(userRanking));
    // }

    public void ReadMyRankingDB()
    {
        isMyRankingLoad = LOADTYPE.LOADING;
        
        // 내 랭킹 정보
        dataReference.OrderByChild("guestCode").EqualTo(DataManager.instance.userData.guestCode).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.HasChildren)
                {
                    var data = snapshot.Children.First();
                    
                    myName = data.Child("userName").Value.ToString();
                    myScore = -int.Parse(data.Child("userScore").Value.ToString());
                    myIcon = int.Parse(data.Child("userIcon").Value.ToString()); // 음수 복구
                    
                    //내 랭킹 순위 구하기
                    dataReference.OrderByChild("userScore").EndAt(-myScore - 1).GetValueAsync().ContinueWithOnMainThread(task =>
                    {
                        if (task.IsCompleted)
                        {
                            var snapshot = task.Result;
                            myRanking = (int)snapshot.ChildrenCount + 1;
                            isMyRankLoad = LOADTYPE.LOADED;
                        }
                    });
                }
                isMyRankingLoad = LOADTYPE.LOADED;
                StartCoroutine(ShowMyRankInform());
            }
        });
    }
    
    private void ReadRankingDB(int page)
    {
        isRankingLoad = LOADTYPE.LOADING;
        
        // 데이터 초기화
        if (page == 0) rankingData.Clear(); // 첫 페이지일 때만 초기화
        
        // 랭킹 정보 불러우기
        // 페이지 범위 계산
        int startAt = page * PAGE_SIZE; // 시작
        int endAt = startAt + PAGE_SIZE; // 끝

        // 점수 기준 정렬 및 범위 제한 쿼리
        dataReference.OrderByChild("userScore").LimitToFirst(endAt).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsCompleted) {
                DataSnapshot snapshot = task.Result;
                int index = 0;

                foreach (DataSnapshot data in snapshot.Children) {
                    // 필요한 데이터만 추가
                    if (startAt <= index && index < endAt) {
                        if (data.HasChild("guestCode") && data.HasChild("userName") && data.HasChild("userIcon") && data.HasChild("userScore")) {
                            string guestCode = data.Child("guestCode").Value.ToString();
                            string userName = data.Child("userName").Value.ToString();
                            int userIcon = int.Parse(data.Child("userIcon").Value.ToString());
                            int userScore = -int.Parse(data.Child("userScore").Value.ToString()); // 음수 복구

                            rankingData.Add(new UserRanking(guestCode, userName, userIcon, userScore));
                        }
                    }
                    index++;
                }
                isRankingLoad = LOADTYPE.LOADED;
            }
        });
    }

    public void SetRank() {
        if (isMyRankingLoad == LOADTYPE.LOADING) return;
        // 상위 30개 데이터 가져오기
        ReadRankingDB(currentPage);

        currentPage++;
    }
    
    private IEnumerator ShowMyRankInform()
    {
        yield return new WaitUntil(() => isMyRankingLoad == LOADTYPE.LOADED && isMyRankLoad == LOADTYPE.LOADED);
        
        myRankingBar.Find("MyRank_UserName").GetComponent<Text>().text = myName;
        myRankingGameBar.Find("MyRank_UserName").GetComponent<Text>().text = myName;
        
        var iconSprite = Resources.Load<Sprite>("CharacterList/CharacterThumbnail/c" + (myIcon + 1).ToString("D3"));
        myRankingBar.Find("ProfileLayout").GetChild(0).GetComponent<Image>().sprite = iconSprite;
        myRankingGameBar.Find("ProfileLayout").GetChild(0).GetComponent<Image>().sprite = iconSprite;
        
        if(myScore <= 0)
        {
            myRankingBar.Find("MyRank_Ranking").GetChild(0).GetComponent<Text>().text = "-";
            myRankingBar.Find("MyRank_Score").GetComponent<Text>().text = "-";
            
            myRankingGameBar.Find("MyRank_Ranking").GetChild(0).GetComponent<Text>().text = "-";
            myRankingGameBar.Find("MyRank_Score").GetComponent<Text>().text = "-";
        }
        else
        {
            myRankingBar.Find("MyRank_Score").GetComponent<Text>().text = myScore.ToString();
            myRankingGameBar.Find("MyRank_Score").GetComponent<Text>().text = myScore.ToString();
        }
        
        //순위
        if (myRanking > 3 || myRanking <= 0)
        {
            myRankingBar.Find("MyRank_Ranking").GetChild(0).GetComponent<Text>().text = "#" + myRanking;
            myRankingGameBar.Find("MyRank_Ranking").GetChild(0).GetComponent<Text>().text = "#" + myRanking;
        }
        else
        {
            myRankingBar.Find("MyRank_Ranking").GetChild(0).gameObject.SetActive(false);
            myRankingBar.Find("MyRank_Ranking").GetChild(1).gameObject.SetActive(true);
            myRankingBar.Find("MyRank_Ranking").GetChild(1).GetComponent<Image>().sprite = top3Image[myRanking -1];
            
            myRankingGameBar.Find("MyRank_Ranking").GetChild(0).gameObject.SetActive(false);
            myRankingGameBar.Find("MyRank_Ranking").GetChild(1).gameObject.SetActive(true);
            myRankingGameBar.Find("MyRank_Ranking").GetChild(1).GetComponent<Image>().sprite = top3Image[myRanking -1];
        }
    }
}