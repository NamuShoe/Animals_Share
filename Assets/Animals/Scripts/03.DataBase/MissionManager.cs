using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;
    public MissionData missionData;
    private Dictionary<string, MissionData> mainMissionDataObject = new Dictionary<string, MissionData>();
    private Dictionary<string, MissionData> submissionDataObject = new Dictionary<string, MissionData>();

    [SerializeField] private GameObject MainMissionPrefab;
    [SerializeField] private GameObject SubMissionPrefab;

    [SerializeField] private Transform MainMissionContainer;
    [SerializeField] private Transform SubMissionContainer;

    void Awake()
    {
        // if(instance == null)
        //     instance = this;
        // else if(instance != this)
        //     Destroy(gameObject);
        
        instance = this;
        LoadMissionData();
    }

    void Start()
    {
        SetMissionData();
    }

    public void LoadMissionData()
    {
        TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.MissionData); //GetMissionDataAsset();

        if (jsonData != null && !string.IsNullOrEmpty(jsonData.text))
        {
            submissionDataObject.Clear();
            submissionDataObject = JsonConvert.DeserializeObject<Dictionary<string, MissionData>>(jsonData.text);
        }
        
        jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.MainMissionData); //GetMissionDataAsset();

        if (jsonData != null && !string.IsNullOrEmpty(jsonData.text))
        {
            mainMissionDataObject.Clear();
            mainMissionDataObject = JsonConvert.DeserializeObject<Dictionary<string, MissionData>>(jsonData.text);
        }
    }

    public MissionData ReadMissionData(string passNum)
    {
        if (submissionDataObject != null && submissionDataObject.ContainsKey(passNum)) {
            return missionData = submissionDataObject[passNum]; // 스테이지 데이터에 얻은 키 연결
        }
        return null;
    }

    public void SetMissionData()
    {
        foreach (Transform child in MainMissionContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < mainMissionDataObject.Count; i++) {
            var missionObject = Instantiate(MainMissionPrefab, MainMissionContainer)
                .GetComponent<NoticeStructure>();
            missionObject.SetMainMission(mainMissionDataObject[(i + 1).ToString()]);
        }
        
        foreach (Transform child in SubMissionContainer)
            Destroy(child.gameObject);
        
        foreach (var dailyQuestNum in DataManager.instance.userData.dailyQuestList) {
            var missionObject = Instantiate(SubMissionPrefab, SubMissionContainer).GetComponent<NoticeStructure>();
            missionObject.SetSubMission(submissionDataObject[dailyQuestNum.missionNum.ToString()]);
        }
    }

    private void RenewSubMissionData()
    {
        var subMissionStructures = SubMissionContainer.GetComponentsInChildren<NoticeStructure>();
        var dailyQuestNum = DataManager.instance.userData.dailyQuestList;
        
        for (int i = 0; i < DataManager.instance.userData.dailyQuestList.Count; i++) {
            subMissionStructures[i]
                .SetSubMission(
                    submissionDataObject[dailyQuestNum[i].missionNum.ToString()]);
        }
    }

    public void MissionClearCheck(int missionNum, int clearNum = 1) // 1회 미션 클리어 확인
    {
        // if(DataManager.instance.userData.dailyQuestList.Contains(missionNum) == false)
        //     return;
        var mission = DataManager.instance.userData.dailyQuestList.Find(x => x.missionNum == missionNum);
        if (mission == null)
            return;
        
        mission.currentNum += clearNum;
        mission.currentNum =
            Mathf.Clamp(mission.currentNum, 0, submissionDataObject[missionNum.ToString()].requiredNum);
        
        
        if(SceneManager.GetActiveScene().name == "Main")
            RenewSubMissionData();
        
        //int clearIndex = DataManager.instance.userData.dailyQuestList.IndexOf(mission);
        //DataManager.instance.userData.dailyQuestList[clearIndex] = 0;
        //DataManager.instance.userData.dailyQuestClear[clearIndex] = true;
        
        //TakeRewards(submissionDataObject[missionNum.ToString()]);
    }

    public void TakeRewards(MissionData missionData)
    {
        for (int i = 0; i < missionData.rewardType.Count; i++) {
            DataManager.instance.TakeReward(missionData.rewardType[i], missionData.rewardAmount[i]);
        }

        DataManager.instance.userData.passPoint += 1;
        Debug.Log("패스 경험치 : " +  DataManager.instance.userData.passPoint);
        
        DataManager.instance.SaveUserData();
    }
}
