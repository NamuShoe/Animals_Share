using System.IO;
using UnityEngine;

public static class FileConnecter
{
    public enum DATA_TYPE {
        ExpData,
        ItemLevelData,
        CharacterData,
        EnemyData,
        MainMissionData,
        MissionData,
        PassData,
        StageData,
        RankingStageData
    }
    
    // 플랫폼에 따라 기본 경로를 선택 후 상대 경로를 결합
    private static string GetFilePath(string relativePath = "")
    {
        string basePath = (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            ? Application.persistentDataPath
            : Application.dataPath;
        return Path.Combine(basePath, relativePath);
    }

    /// <summary>
    /// 파일 여부 확인용
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static string GetDataPath(DATA_TYPE dataType)
    {
        string basePath = GetFilePath();
        string path = "Resources/GameData/" + dataType + ".json";
        return Path.Combine(basePath, path);
    }
    
    public static TextAsset GetDataAsset(DATA_TYPE dataType)
    {
        string path = "GameData/" + dataType;
        return Resources.Load<TextAsset>(path);;
    }

    // EquipmentData
    public static TextAsset GetEquipmentDataAsset(EquipmentType equipmentType)
    {
        switch (equipmentType)
        {
            case EquipmentType.Weapon:
                return Resources.Load<TextAsset>("GameData/WeaponData");
            case EquipmentType.Hat:
                return Resources.Load<TextAsset>("GameData/HatData");
            case EquipmentType.Accessory:
                return Resources.Load<TextAsset>("GameData/AccessoryData");
            case EquipmentType.Shoes:
                return Resources.Load<TextAsset>("GameData/ShoesData");
            case EquipmentType.Coat:
                return Resources.Load<TextAsset>("GameData/CoatData");
            default:
                return null;
        }
    }
}
