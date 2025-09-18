using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class EnemyDataManager : MonoBehaviour
{
    public static EnemyDataManager instance;
    public List<EnemyData> enemies;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoadEnemyData();
    }

    private void LoadEnemyData()
    {
        TextAsset jsonFile = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.EnemyData); //GetEnemyDataAsset();
        if (jsonFile != null)
        {
            enemies = JsonConvert.DeserializeObject<List<EnemyData>>(jsonFile.text);
            //System.IO.File.WriteAllText(savePath, jsonFile.text);
        }
        // if (!System.IO.File.Exists(savePath))
        // {
        //     CreateEnemyDataJson();
        // }
        //
        // string jsonFile = System.IO.File.ReadAllText(savePath);
        // enemies = JsonHelper.FromJson<EnemyData>(jsonFile);
    }

    private void CreateEnemyDataJson()
    {
        TextAsset jsonFile = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.EnemyData); //GetEnemyDataAsset();
        if (jsonFile != null)
        {
            enemies = JsonConvert.DeserializeObject<List<EnemyData>>(jsonFile.text);
            //System.IO.File.WriteAllText(savePath, jsonFile.text);
        }
    }
}