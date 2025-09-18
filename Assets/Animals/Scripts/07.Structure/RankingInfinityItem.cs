using UnityEngine;
using UnityEngine.UI;
using Yejun.UGUI;

public class RankingInfinityItem : MonoBehaviour, IContent {
    [SerializeField] private Text rankingText;
    [SerializeField] private Image rankingImg;
    [SerializeField] private Image icon;
    [SerializeField] private Text userName;
    [SerializeField] private Text userScore;
    
    // Start is called before the first frame update
    void Reset()
    {
        rankingText = transform.Find("RankingNumber/RankingTxt").GetComponent<Text>();
        rankingImg = transform.Find("RankingNumber/RankingImg").GetComponent<Image>();
        icon = transform.Find("ProfileLayout").GetChild(0).GetComponent<Image>();
        userName = transform.Find("UserName").GetComponent<Text>();
        userScore = transform.Find("UserScore").GetComponent<Text>();
    }

    // public void Reload(InfinityScrollView infinity, int _index)
    // {
    //     // 랭킹 데이터를 동적으로 갱신
    //     if(_index < 3) { // 3위 내는 이미지
    //         rankingImg.gameObject.SetActive(true);
    //         rankingText.gameObject.SetActive(false);
    //         rankingImg.GetComponent<Image>().sprite = RankingManager.instance.top3Image[_index];
    //     }
    //     else { // 3위 이후 텍스트
    //         rankingImg.gameObject.SetActive(false);
    //         rankingText.gameObject.SetActive(true);
    //         rankingText.GetComponent<Text>().text = "#" + (_index + 1).ToString();
    //     }
    //
    //     var rankingData = RankingManager.instance.rankingData[_index];
    //     var iconSprite = Resources.Load<Sprite>("CharacterList/CharacterThumbnail/c" + (rankingData.userIcon + 1).ToString("D3"));
    //     icon.sprite = iconSprite;
    //     userName.text = rankingData.userName; //rankingData.userRankings[indexReload].userName;
    //     userScore.text = rankingData.userScore.ToString();
    // }

    bool IContent.Update(int index)
    {
        // 랭킹 데이터를 동적으로 갱신
        if(index < 3) { // 3위 내는 이미지
            rankingImg.gameObject.SetActive(true);
            rankingText.gameObject.SetActive(false);
            rankingImg.GetComponent<Image>().sprite = RankingManager.instance.top3Image[index];
        }
        else { // 3위 이후 텍스트
            rankingImg.gameObject.SetActive(false);
            rankingText.gameObject.SetActive(true);
            rankingText.GetComponent<Text>().text = "#" + (index + 1).ToString();
        }

        var rankingData = RankingManager.instance.rankingData[index];
        var iconSprite = Resources.Load<Sprite>("CharacterList/CharacterThumbnail/c" + (rankingData.userIcon + 1).ToString("D3"));
        icon.sprite = iconSprite;
        userName.text = rankingData.userName; //rankingData.userRankings[indexReload].userName;
        userScore.text = rankingData.userScore.ToString();

        return true;
    }
}
