using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

public class ExpManager : MonoBehaviour {
    public static ExpManager instance;
    private string savePath;
    public Dictionary<int, ExpData> expDatas = new Dictionary<int, ExpData>();

    void Awake()
    {
        if(instance == null){
            instance = this;
        }
    }
    
    void Start() {
        // FileConneter의 static 메소드로 경로를 받아옴
        JsonLoad();
    }

    public void JsonLoad()
    {
        
        TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.ExpData); //GetMissionDataAsset();

        if (jsonData != null && !string.IsNullOrEmpty(jsonData.text))
        {
            expDatas.Clear();
            expDatas = JsonConvert.DeserializeObject<Dictionary<int, ExpData>>(jsonData.text);
        }
        
        // savePath = FileConnecter.GetDataPath(FileConnecter.DATA_TYPE.ExpData);
        //
        // if (!File.Exists(savePath)) {
        //     //파일이 없으면 fireBase에서 가져오기
        //     //JsonSave();
        // }
        // else {
        //     TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.ExpData);
        //     if (jsonData != null) {
        //         string jsonText = jsonData.text;
        //         if (!string.IsNullOrEmpty(jsonText)) {
        //             expDatas = JsonConvert.DeserializeObject<Dictionary<int, ExpData>>(jsonText);
        //         }
        //     }
        // }
        // string loadJson = File.ReadAllText(savePath);
        // expData = JsonUtility.FromJson<ExpData>(loadJson);
    }

    public ExpData GetExpData(int level)
    {
        try {
            return expDatas[level];
        }
        catch (Exception e) {
            Debug.LogError("exp데이터가 없어영" + e);
            return null;
            throw;
        }
    }
    
    // public void JsonSave() {
    //     int baseExp = 25;
    //     for (int i = 0; i < 5; i++) {
    //         if(i != 0) {
    //             expData.requiredExp.Add(baseExp * i * 3);
    //         }
    //         else {
    //             expData.requiredExp.Add(baseExp);
    //         }
    //     }
    //
    //     string json = JsonUtility.ToJson(expData, true);
    //     File.WriteAllText(savePath, json);
    // }
}
