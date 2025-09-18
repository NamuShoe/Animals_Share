using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;


[Serializable]
public class Mail {
    public int index;
    public string timeStart;
    public string timeEnd;
    
    public string title;
    public string description;

    public List<RewardItem> rewardItems = new List<RewardItem>();
}

public class MailBoxManager : MonoBehaviour {
    [SerializeField] private Transform mailContainer;
    [SerializeField] private GameObject mailPrefab;
    
    [SerializeField] private List<Mail> mailList = new List<Mail>();
    
    private void Start()
    {
        StartCoroutine(ReadMailDB());
    }

    private IEnumerator ReadMailDB()
    {
        DatabaseReference userReference = FirebaseDatabase.DefaultInstance.GetReference("Mail");

        var mails = new List<Mail>();
        var task = userReference.GetValueAsync().ContinueWithOnMainThread(task => { 
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
                mails.Clear();
                
                foreach (DataSnapshot child in snapshot.Children) {
                    var mail = new Mail {
                        index = int.Parse(child.Child("index").Value.ToString()),
                        timeStart = child.Child("timeStart").Value.ToString(),
                        timeEnd = child.Child("timeEnd").Value.ToString(),
                        
                        title = child.Child("title").Value.ToString(),
                        description = child.Child("description").Value.ToString()
                    };
                    
                    foreach (var rewardItem in child.Child("rewardItems").Children) {
                        RewardType itemType = (RewardType)Enum.Parse(typeof(RewardType), rewardItem.Child("rewardType").Value.ToString());
                        int amount = int.Parse(rewardItem.Child("amount").Value.ToString());
                        mail.rewardItems.Add(new RewardItem(itemType, amount));
                    }
                    mails.Add(mail);
                }
            } else {
                Debug.LogError("데이터를 불러오는 데 실패했습니다.");
            }
        });
        yield return new WaitUntil(() => task.IsCompleted);
        SetupMailList(mails);
    }

    private void SetupMailList(List<Mail> mails)
    {
        mailList.Clear();
        
        foreach (var mail in mails) {
            if (DateTime.Parse(mail.timeStart) < LoginManager.instance.DateNow &&
                LoginManager.instance.DateNow < DateTime.Parse(mail.timeEnd)) {
                mailList.Add(mail);
            }
        }
        
        SetMailList();
    }

    private void SetMailList()
    {
        foreach (Transform child in mailContainer) {
            Destroy(child.gameObject);
        }

        foreach (var mail in mailList) {
            var mailObject = Instantiate(mailPrefab, mailContainer).GetComponent<NoticeStructure>();
            mailObject.SetMail(mail);
        }
    }
}
