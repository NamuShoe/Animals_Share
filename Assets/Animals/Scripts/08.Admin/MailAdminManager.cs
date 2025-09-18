using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;

public class MailAdminManager : MonoBehaviour
{
    private string dbURL = "https://animalsshooting-7850d-default-rtdb.firebaseio.com/";

    [SerializeField] private Button mailButton;
    public Mail mail;
    
    void Start()
    {
        
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(dbURL);
        FirebaseAuth auth = FirebaseAuth.DefaultInstance;
        
        mailButton.onClick.AddListener(WriteMail);
    }

    private void WriteMail()
    {
        try {
            var a = DateTime.Parse(mail.timeStart);
            Debug.Log(a);
            var b = DateTime.Parse(mail.timeEnd);
            Debug.Log(b);
        }
        catch (Exception e) {
            Debug.Log(e);
            return;
        }

        DatabaseReference databaseRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Mail");
        
        databaseRef.Child(mail.index.ToString()).Child("index").SetValueAsync(mail.index);
        databaseRef.Child(mail.index.ToString()).Child("timeStart").SetValueAsync(mail.timeStart);
        databaseRef.Child(mail.index.ToString()).Child("timeEnd").SetValueAsync(mail.timeEnd);
        
        databaseRef.Child(mail.index.ToString()).Child("title").SetValueAsync(mail.title);
        databaseRef.Child(mail.index.ToString()).Child("description").SetValueAsync(mail.description);
        
        for (int i = 0; i < mail.rewardItems.Count; i++) {
            databaseRef.Child(mail.index.ToString()).Child("rewardItems").Child(i.ToString()).Child("rewardType").SetValueAsync(mail.rewardItems[i].rewardType.ToString());
            databaseRef.Child(mail.index.ToString()).Child("rewardItems").Child(i.ToString()).Child("amount").SetValueAsync(mail.rewardItems[i].amount);
        }
    }
}
