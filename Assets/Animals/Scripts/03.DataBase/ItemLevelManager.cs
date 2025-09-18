using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class ItemLevelManager : MonoBehaviour
{
    public static ItemLevelManager instance;
    string savePath;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        LoadItemLevelData();
    }

    public void LoadItemLevelData()
    {
        // 필요시 파일 경로를 FileConneter에서 받아옴 (사용 용도에 따라)
        savePath = FileConnecter.GetDataPath(FileConnecter.DATA_TYPE.ItemLevelData);

        // Resources에서 TextAsset 로드
        TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.ItemLevelData);
        if (jsonData != null)
        {
            string jsonText = jsonData.text;
            if (!string.IsNullOrEmpty(jsonText))
            {
                Dictionary<string, ItemLevelData> itemLevelDataDict =
                    JsonConvert.DeserializeObject<Dictionary<string, ItemLevelData>>(jsonText);
                // 이후 로직에서 itemLevelDataDict 사용
            }
        }
    }

    public ItemLevelData ReadItemLevelData(int itemLevelNum)
    {
        TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.ItemLevelData);
        if (jsonData != null)
        {
            string jsonText = jsonData.text;
            if (!string.IsNullOrEmpty(jsonText))
            {
                Dictionary<string, ItemLevelData> itemLevelDataDict =
                    JsonConvert.DeserializeObject<Dictionary<string, ItemLevelData>>(jsonText);
                if (itemLevelDataDict != null && itemLevelDataDict.ContainsKey(itemLevelNum.ToString()))
                {
                    return itemLevelDataDict[itemLevelNum.ToString()];
                }
            }
        }
        return null;
    }
}
