using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[System.Serializable]
public class MissionData {
    // 미션 번호(레벨)
    public int missionNum;
    public string title;
    public string description;
    public string requirement;
    public int requiredNum;

    [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
    public List<RewardType> rewardType;
    public List<int> rewardAmount;
}

[System.Serializable]
public class Mission {
    public int missionNum;
    public int currentNum;

    public Mission(int mission, int current = 0)
    {
        missionNum = mission;
        currentNum = current;
    }
}