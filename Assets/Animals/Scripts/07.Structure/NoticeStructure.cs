using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NoticeStructure : MonoBehaviour {
    [Header("Receive")]
    public CanvasGroup canvasGroup;
    public GameObject completeStamp;
    private Color alreadyColor = new Color(230, 222, 218);
    
    [Header("Common")]
    public Text RequirementText;
    public List<GameObject> Reward;
    public Button ReceiveButton;

    [Header("Main_Mission")] 
    public Text Title;
    public Text Description;

    [Header("Sub_Mission")] 
    public Text ProgressText;

    [Header("Mail")] 
    public Text LeftTimeText;
    
    private void SetMission(MissionData missionData)
    {
        RequirementText.text = missionData.requirement;

        foreach (var reward in Reward) {
            reward.SetActive(false);
        }

        for (int i = 0; i < missionData.rewardType.Count; i++) {
            Reward[i].SetActive(true);
            Reward[i].transform.GetChild(0).GetComponent<Image>().sprite =
                GetSpriteImage(missionData.rewardType[i]);
            Reward[i].transform.GetChild(1).GetComponent<Text>().text = "x" + missionData.rewardAmount[i];
        }
        
        if (ReceiveButton != null) {
            ReceiveButton.onClick.AddListener(() => {
                MissionManager.instance.TakeRewards(missionData);
                ReceiveButton.interactable = false;
            });
        }
    }
    
    public void SetMainMission(MissionData missionData)
    {
        if (ReceiveButton != null) {
            ReceiveButton.onClick.RemoveAllListeners();
            ReceiveButton.onClick.AddListener(() => {
                DataManager.instance.userData.mainQuestClear[missionData.missionNum - 1] = true;
                Receive(true);
            });
            
            if (DataManager.instance.userData.stageClearCheck[missionData.missionNum - 1] > 0)
                ReceiveButton.interactable = true;
            else
                ReceiveButton.interactable = false;
            
            if(DataManager.instance.userData.mainQuestClear[missionData.missionNum - 1] == true)
                Receive(false);
        }
        
        if (Title != null)
            Title.text = missionData.title;

        if (Description != null)
            Description.text = missionData.description;
        
        SetMission(missionData);
    }
    public void SetSubMission(MissionData missionData)
    {
        var dailyQuestMission =
            DataManager.instance.userData.dailyQuestList.Find(x => x.missionNum == missionData.missionNum);
        var clearNum = DataManager.instance.userData.dailyQuestList.IndexOf(dailyQuestMission);

        if (ReceiveButton != null) {
            ReceiveButton.onClick.RemoveAllListeners();
            ReceiveButton.onClick.AddListener(() => {
                DataManager.instance.userData.dailyQuestClear[clearNum] = true;
                MissionManager.instance.MissionClearCheck(16);
                Receive(true);
            });
            
            if (dailyQuestMission.currentNum >= missionData.requiredNum)
                ReceiveButton.interactable = true;
            else
                ReceiveButton.interactable = false;
            
            if(DataManager.instance.userData.dailyQuestClear[clearNum] == true)
                Receive(false);
        }
        
        if (ProgressText != null)
            ProgressText.text = dailyQuestMission.currentNum + "/" + missionData.requiredNum;
        
        SetMission(missionData);
    }

    public void SetMail(Mail mail)
    {
        if(Title != null)
            Title.text = mail.title;

        if (Description != null)
            Description.text = mail.description;

        Reward[0].transform.GetChild(0).GetComponent<Image>().sprite = GetSpriteImage(mail.rewardItems[0].rewardType);
        Reward[0].transform.GetChild(1).GetComponent<Text>().text = "x" + mail.rewardItems[0].amount;

        if (LeftTimeText != null) {
            TimeSpan timeDiff = DateTime.Parse(mail.timeEnd) - LoginManager.instance.DateNow;
            if (timeDiff.Days > 0)
                LeftTimeText.text = timeDiff.Days + "일 남음";
            else
                LeftTimeText.text = timeDiff.Hours + "시간 나음";
        }

        if (ReceiveButton != null) {
            ReceiveButton.onClick.RemoveAllListeners();
            ReceiveButton.interactable = 
                DataManager.instance.userData.mailRead.Exists(x => x == mail.index) == false;
            
            ReceiveButton.onClick.AddListener(() => {
                foreach (var t in mail.rewardItems) {
                    DataManager.instance.TakeReward(t.rewardType, t.amount);
                }
                DataManager.instance.userData.mailRead.Add(mail.index);
                ReceiveButton.interactable = false;
            });
        }
    }

    private Sprite GetSpriteImage(RewardType rewardType)
    {
        string imagePath = "Shop/ShopItem/" + rewardType;
        Sprite image = Resources.Load<Sprite>(imagePath);
        return image;
    }

    private void Receive(bool isAlready = false)
    {
        canvasGroup.alpha = 0.8f;
        canvasGroup.interactable = false;
        completeStamp.SetActive(true);

        if (isAlready) {
            var stampSeq = DOTween.Sequence();
            var stampImage = completeStamp.GetComponent<Image>();
            var stampRect = completeStamp.GetComponent<RectTransform>();

            var color = stampImage.color;
            color.a = 0f;
            stampImage.color = color;
        
            completeStamp.transform.localScale = Vector3.one * 1.1f;
            stampSeq.Append(completeStamp.GetComponent<Image>().DOFade(1.0f, 0.3f))
                .Join(stampRect.DOScale(Vector3.one * 1.2f, 0.3f))
                .Append(stampRect.DOScale(Vector3.one * 0.9f, 0.1f))
                .Append(stampRect.DOScale(Vector3.one, 0.05f))
                .OnComplete(() => SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Select));
            //SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Select);
        }
    }
}