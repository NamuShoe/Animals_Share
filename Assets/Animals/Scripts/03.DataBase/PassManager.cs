using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

public class PassManager : MonoBehaviour
{
    public static PassManager instance;
    public PassData passData;

    void Awake()
    {
        if(instance == null)
            instance = this;
        else if(instance != this)
            Destroy(gameObject);
    }

    void Start()
    {
        LoadPassData();
    }

    public void LoadPassData()
    {
        TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.PassData); //GetPassDataAsset();

        if (jsonData != null && !string.IsNullOrEmpty(jsonData.text))
        {
            Dictionary<string, PassData> passDataObject = JsonConvert.DeserializeObject<Dictionary<string, PassData>>(jsonData.text);
        }
    }

    public void ReadPassData(string passNum)
    {
        TextAsset jsonData = FileConnecter.GetDataAsset(FileConnecter.DATA_TYPE.PassData); //GetPassDataAsset();

        if (jsonData != null && !string.IsNullOrEmpty(jsonData.text))
        {
            Dictionary<string, PassData> passDataObject = JsonConvert.DeserializeObject<Dictionary<string, PassData>>(jsonData.text);

            if (passDataObject != null && passDataObject.ContainsKey(passNum))
            {
                passData = passDataObject[passNum]; // 스테이지 데이터에 얻은 키 연결
            }
        }
    }

    public void ReceiveReward()
    {
        // 유료 상품 샀는지 조건 추가해야함

        // 아이템, 재화 지급
        // DataManager.instance.userData.resurrectionTicket += passData.resurrectionTicket;
        // DataManager.instance.userData.normalBox += passData.normalBox;
        // DataManager.instance.userData.magicBox += passData.magicBox;

        DataManager.instance.userData.Diamond += passData.diamond;
        DataManager.instance.userData.Life += passData.life;

        for(int i = 0; i < DataManager.instance.userData.levelUpMaterial.Count; i++)
            DataManager.instance.userData.levelUpMaterial[i] += passData.levelUpMaterial;

        // 캐릭터 지급
        if(passData.characterId != 0)
        {
            DataManager.instance.userData.characterList.Add(passData.characterId);
            DataManager.instance.userData.characterInforms.Insert(passData.characterId, new CharacterSpecific(1, 1, 0, new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 }));
        }
    }

    public void ReceivePass(int passNum)
    {
        ReadPassData(passNum.ToString());
        ReceiveReward();
        Debug.Log
        (
            "수령 된 보상 : \n" + "다이아 : " + passData.diamond + "\n"
            + "일반 상자 : " + passData.normalBox + "\n"
            + "고급 상자 : " + passData.magicBox + "\n"
        );
    }
}
