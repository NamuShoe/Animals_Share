using UnityEngine;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

public enum StageType
{
    [Description("숲속")]
    Forest,
    [Description("마을")]
    Village,
    [Description("늪지")]
    Desert,
    [Description("사막")]
    Wetland,
    [Description("해안가")]
    Waterfront
}

[System.Serializable]
public class StageData {
    public int stageId;
    [JsonConverter(typeof(StringEnumConverter))]
    public StageType stageType;
    public List<float> stageMultiple;
    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<DropItemType> dropItemType;
    public List<float> dropItemRate;
    public int rewardGold;
    public int rewardExp;

    public int stageStep;
    public List<int> stageKillCount;
    public List<int> enemies;
    public List<float> enemyMultiple;
    public List<float> frequencies;
    public List<List<float>> enemyFrequencies;
}

public class StageManager : MonoBehaviour
{
    public static StageManager instance;
    public StageData stageData;
    public int stageMultipleIndex = 0;
    public float StageMultiple => stageData.stageMultiple[stageMultipleIndex];
    private Dictionary<string, StageData> stageDataDic;
    public List<StageData> stageDatas;
    
    [SerializeField] private GameObject stagePrefab;
    [SerializeField] private Transform stageContent;
    [SerializeField] private Button stageGameStart;
    [SerializeField] private ScrollController scrollController;
    [SerializeField] private Button rankingBtn;

    void Awake()
    {
        if(instance == null)
            instance = this;
        else if(instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        LoadStageData();
    }

    private void LoadStageData()
    {
        stageDataDic = LoadStageDataFromFile(FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.StageData));

        if (stageDataDic != null)
        {
            foreach (Transform child in stageContent)
            {
                Destroy(child.gameObject);
            }

            if (stageDataDic.Count > DataManager.instance.userData.stageClearCheck.Count) {
                var diffCount = stageDataDic.Count - DataManager.instance.userData.stageClearCheck.Count;
                for (int i = 0; i < diffCount; i++) {
                    DataManager.instance.userData.stageClearCheck.Add(0);
                }
            }

            foreach (var stageDataPair in stageDataDic)
            {
                if(stageDataPair.Value.stageId != 1)
                    if(DataManager.instance.userData.stageClearCheck[stageDataPair.Value.stageId - 1 - 1] <= 0)
                        continue;
                
                
                //**************************************************
                if (stageDataPair.Value.stageId >= 3)
                    continue;
                //**************************************************
                
                var stage = Instantiate(stagePrefab, stageContent).GetComponent<StageStructure>();
                stage.SetStageStructure(stageDataPair.Value, stageDataPair.Key);
                //stageDatas.Add(stageDataPair.Value);
            }
        }
        
        //scrollController.SetActiveIndicator();

        stageGameStart.onClick.RemoveAllListeners();
        stageGameStart.onClick.AddListener(GameStartOnClick);

        // Dictionary<string, StageData> rankingStageDataObject = LoadStageDataFromFile(FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.RankingStageData));
        //
        // if (rankingStageDataObject != null)
        // {
        //     foreach (var item in rankingStageDataObject)
        //     {
        //         rankingBtn.onClick.RemoveAllListeners();
        //         rankingBtn.onClick.AddListener(() => ReadStageData(item.Key, true));
        //         rankingBtn.onClick.AddListener(() => _SceneManager.instance.RankingGameStart(int.Parse(item.Key)));
        //     }
        // }
    }

    private void GameStartOnClick()
    {
        var num = scrollController.SelectedNum;
        
        var stageStructure = stageContent.GetChild(num).GetComponent<StageStructure>();
        stageData = stageStructure.data;
        stageMultipleIndex = stageStructure.stageDifficultyIndex;
        //ReadStageData(stageDataDic.Keys.ElementAt(num), false);
        _SceneManager.instance.GameStart(num);
    }

    private void ReadStageData(string stageNum, bool isRanking)
    {
        TextAsset jsonData = isRanking
            ? FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.RankingStageData)
            : FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.StageData);
        stageDataDic = LoadStageDataFromFile(jsonData);

        if (stageDataDic != null && stageDataDic.ContainsKey(stageNum))
        {
            stageData = stageDataDic[stageNum]; // 스테이지 데이터 로드
        }
    }

    private Dictionary<string, StageData> LoadStageDataFromFile(TextAsset jsonData)
    {
        if (jsonData != null && !string.IsNullOrEmpty(jsonData.text))
        {
            return JsonConvert.DeserializeObject<Dictionary<string, StageData>>(jsonData.text);
        }
        return null;
    }
}
