using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using System;
using UnityEngine.Serialization;

public enum Status {
    Waiting,
    Success,
    Fail
}

public class AdmobManager: MonoBehaviour {
    public static AdmobManager instance; // 싱글톤을 구현합니다.
    private string _adUnitId;
    public bool onEarnedReward = false;
    public bool failedEarnedReward = false;
    
    public Status status = Status.Waiting;

    private void Awake()
    {

        if (instance != null) {
            Destroy(gameObject);
        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        onEarnedReward = false;
        status = Status.Waiting;

        if (Application.platform == RuntimePlatform.Android) {
            _adUnitId = "ca-app-pub-9264896581201928/5541493251";
            //_adUnitId = "ca-app-pub-1041303930548259/5308939443";
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer) {
            _adUnitId = "ca-app-pub-3940256099942544/1712485313";
        }
        else {
            _adUnitId = "ca-app-pub-3940256099942544/5224354917";
        }
    }

    public void Start()
    {
        // Google 모바일 광고 SDK를 초기화합니다.
        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            // 이 콜백은 MobileAds SDK가 초기화되면 호출됩니다.
            LoadRewardedAd();
        });
    }
    // These ad units are configured to always serve test ads.
// #if UNITY_ANDROID
//     private string _adUnitId = "ca-app-pub-3940256099942544/5224354917";
// #elif UNITY_IPHONE
//     private string _adUnitId = "ca-app-pub-3940256099942544/1712485313";
// #else
//     private string _adUnitId = "unused";
// #endif

    private RewardedAd rewardedAd;

    /// <summary>
    /// Loads the rewarded ad.
    /// </summary>
    public void LoadRewardedAd()
    {
        // 새 광고를 로드하기 전에 이전 광고를 정리하십시오.
        if (rewardedAd != null) {
            rewardedAd.Destroy();
            rewardedAd = null;
        }
        Debug.Log("Loading the rewarded ad.");
        // 광고를 로드하는 데 사용되는 요청을 생성합니다.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        RewardedAd.Load(_adUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) => {
                // 오류가 null이 아니면 로드 요청이 실패한 것입니다.
                if (error != null || ad == null) {
                    Debug.LogError("Rewarded ad failed to load an ad " + "with error : " + error);
                    return;
                }

                Debug.Log("Rewarded ad loaded with response : " + ad.GetResponseInfo());
                rewardedAd = ad;

                RegisterEventHandlers(ad);
            });
    }
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // 광고에서 수익이 발생한 것으로 추정될 때 발생합니다.
        ad.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
            Debug.Log("광고에서 수익이 발생한 것으로 추정될 때 발생합니다. " + adValue.Value + " // " + adValue.CurrencyCode);
        };
        // 광고에 대한 노출이 기록될 때 발생합니다.
        ad.OnAdImpressionRecorded += () =>
        {
            //Debug.Log("Rewarded ad recorded an impression.");
            Debug.Log("광고에 대한 노출이 기록될 때 발생합니다.");
        };
        // 광고에 대한 클릭이 기록될 때 발생합니다.
        ad.OnAdClicked += () =>
        {
            //Debug.Log("Rewarded ad was clicked.");
            Debug.Log("광고에 대한 클릭이 기록될 때 발생합니다.");
        };
        // 광고가 전체 화면 콘텐츠를 열 때 발생합니다.
        ad.OnAdFullScreenContentOpened += () =>
        {
            //Debug.Log("Rewarded ad full screen content opened.");
            Debug.Log("광고가 전체 화면 콘텐츠를 열 때 발생합니다.");
        };
        // 광고가 전체 화면 콘텐츠를 닫을 때 발생합니다.
        ad.OnAdFullScreenContentClosed += () =>
        {
            //Debug.Log("Rewarded ad full screen content closed.");
            Debug.Log("광고가 전체 화면 콘텐츠를 닫을 때 발생합니다.");
            // 다음 보상형 광고 미리 로드
            LoadRewardedAd();
        };
        // 광고가 전체 화면 콘텐츠를 열지 못했을 때 발생합니다.
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("Rewarded ad failed to open full screen content " + "with error : " + error);
            status = Status.Fail;
            failedEarnedReward = true;
            // 다음 보상형 광고 미리 로드
            LoadRewardedAd();
        };
    }
    public void ShowRewardedAd()
    {
        const string rewardMsg =
            "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
    
        if (rewardedAd != null && rewardedAd.CanShowAd()) {
            status = Status.Waiting;
            onEarnedReward = false;
            failedEarnedReward = false;
            rewardedAd.Show((Reward reward) =>
            {
                // TODO: Reward the user.
                //GoodsManager.instance.IncreaseGoods(0, 100, 0); // 100골드 상승, 보상 예시
                MissionManager.instance.MissionClearCheck(1); // 광고 보기 미션
                Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                status = Status.Success;
                onEarnedReward = true;
                //GetReward(reward.Type, reward.Amount);
            });
        }
    }
    
    // public bool ShowRewardedAd()
    // {
    //     const string rewardMsg =
    //         "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";
    //
    //     if (rewardedAd != null && rewardedAd.CanShowAd())
    //     {
    //         rewardedAd.Show((Reward reward) =>
    //         {
    //             // TODO: Reward the user.
    //             //GoodsManager.instance.IncreaseGoods(0, 100, 0); // 100골드 상승, 보상 예시
    //             MissionManager.instance.MissionClearCheck(1); // 광고 보기 미션
    //             Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
    //             //GetReward(rewardType, rewardAmount);
    //         });
    //         return true;
    //     }
    //     else {
    //         return false;
    //     }
    // }

    private void GetReward(string type, double amount)
    {
        RewardType rewardType = (RewardType)Enum.Parse(typeof(RewardType), type);
        int rewardAmount = (int)amount;
        
        DataManager.instance.TakeReward(rewardType, rewardAmount);
    }
    
    private void GetReward(RewardType rewardType, int rewardAmount)
    {
        DataManager.instance.TakeReward(rewardType, rewardAmount);
    }
}