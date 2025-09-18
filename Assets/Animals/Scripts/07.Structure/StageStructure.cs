using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageStructure : MonoBehaviour {
    [HideInInspector]public StageData data;
    
    [SerializeField] private Text StageNameText;
    [SerializeField] private Image StageImage;
    [SerializeField] private GameObject[] StageDifficultyStar;
    [SerializeField] private Button indicatorLeft;
    [SerializeField] private Button indicatorRight;
    [SerializeField] private Transform StageEnemy;
    [SerializeField] private Transform StageReward;
    
    public int stageDifficultyIndex = 0;

    public void SetStageStructure(StageData stageData, string stageNum = null)
    {
        data = stageData;
        StageNameText.text = "STAGE " + stageNum + " : " + stageData.stageType.GetNameKr();
        StageImage.sprite = 
            Resources.Load<Sprite>("BackGround/Thumbnail/" + (stageData.stageType));

        foreach (Transform child in StageEnemy) 
            child.gameObject.SetActive(false);
        foreach (Transform child in StageReward) 
            child.gameObject.SetActive(false);
                
        for (int i = 0; i < Mathf.Clamp(stageData.enemies.Count, 0, 4); i++)
        {
            var stageEnemy = StageEnemy.GetChild(i);
            stageEnemy.gameObject.SetActive(true);
            stageEnemy.GetComponent<Image>().sprite =
                Resources.Load<Sprite>("CharacterList/EnemyThumbnail/e" +
                                       stageData.enemies[i].ToString("D3"));
        }
        
        var dropItemTypes = stageData.dropItemType
            .Where(x => x != DropItemType.EquipmentAll && x != DropItemType.MaterialAll).ToList();
        for (int i = 0; i < Mathf.Clamp(dropItemTypes.Count, 0, 4); i++) {
            if (dropItemTypes[i] == DropItemType.EquipmentAll ||
                dropItemTypes[i] == DropItemType.MaterialAll)
                continue;
            
            var stageDropItem = StageReward.GetChild(i);
            stageDropItem.gameObject.SetActive(true);
            stageDropItem.GetComponent<Image>().sprite =
                Resources.Load<Sprite>("Item/" + dropItemTypes[i].ToString());
        }
        
        foreach (var star in StageDifficultyStar) 
            star.SetActive(false);
        StageDifficultyStar[0].SetActive(true);
        
        indicatorLeft.onClick.AddListener(IndicatorLeftOnClick);
        indicatorRight.onClick.AddListener(IndicatorRightOnClick);
    }

    private void IndicatorLeftOnClick()
    {
        if (0 < stageDifficultyIndex) {
            //StageDifficultyStar[stageDifficulty].SetActive(false);
            stageDifficultyIndex--;
            
            foreach (var star in StageDifficultyStar) {
                star.SetActive(false);
            }
            
            for (int i = 0; i < stageDifficultyIndex + 1; i++) {
                StageDifficultyStar[i].SetActive(true);
            }
        }
    }
    
    private void IndicatorRightOnClick()
    {
        if (stageDifficultyIndex < (data.stageMultiple.Count - 1)
            && DataManager.instance.userData.stageClearCheck[data.stageId - 1] >= (stageDifficultyIndex + 1)) {
            stageDifficultyIndex++;
            //StageDifficultyStar[stageDifficulty].SetActive(true);
            
            foreach (var star in StageDifficultyStar) {
                star.SetActive(false);
            }
            
            for (int i = 0; i < stageDifficultyIndex + 1; i++) {
                StageDifficultyStar[i].SetActive(true);
            }
        }
    }
}
