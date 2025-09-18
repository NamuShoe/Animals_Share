using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GoodsManager : MonoBehaviour
{
    public static GoodsManager instance;
    [SerializeField] private Button GameStartBtn;
    [SerializeField] private Text LifeText;
    [SerializeField] private Text GoldText;
    [SerializeField] private Text DiamondText;
    [SerializeField] private Text PlayerLevelText; // 재화 작업 끝나면 진행
    [SerializeField] private Text ExpText; // 재화 작업 끝나면 진행
    [SerializeField] private Text PlayerNameText;
    [SerializeField] private Text PlayerUidText;
    const int MAXlife = 30;

    void Awake()
    {
        instance = this;
    }
    void Start() {
        if(LoginManager.loginComplete)
            StartCoroutine(Increaselife()); // 라이프 획득 반복 실행
    }
    /// <summary>
    /// 재화 갱신할때 사용
    /// </summary>
    
    public void UiRefresh() 
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            GoldText.text = FormatCurrency(DataManager.instance.userData.Gold);
            LifeText.text = DataManager.instance.userData.Life + "/30";
            DiamondText.text = FormatCurrency(DataManager.instance.userData.Diamond);
        }
    }
    private string FormatCurrency(int value)
    {
        if (value >= 10000) { // 10,000 이상일 때
            return (value / 1000f).ToString("0.#") + "k"; // 10k 형태로 표현
        }
        return value.ToString(); // 10,000 미만일 때는 그대로 표시
    }
    IEnumerator Increaselife() // 라이프 상승
    {
        yield return new WaitUntil(()=>LoginManager.loginComplete);
        float Timer = 10.0f;
        while (true) {
            if (DataManager.instance.userData.Life >= MAXlife) {
                GameStartBtn.interactable = true;
                yield break;
            }
            DataManager.instance.userData.Life += 1; // 라이프 획득
            if (DataManager.instance.userData.Life < 1) { // 기존 수치 5, 현재는 테스트 값
                GameStartBtn.interactable = false; // 기존 false
            } 
            else {
                GameStartBtn.interactable = true;
            }
            Debug.Log("1라이프 증가: " + DataManager.instance.userData.Life);
            DataManager.instance.SaveUserData();
            yield return new WaitForSeconds(Timer);
        }
    }
    public void ConsumeLife() // 라이프 소모 함수
    {   // 테스트 값 0
        #if UNITY_EDITOR
        DataManager.instance.userData.Life -= 0; // 에디터
        #elif UNITY_ANDROID
        DataManager.instance.userData.Life -= 0; // 안드로이드
        #else
        DataManager.instance.userData.Life -= 0;
        #endif

        //DataManager.instance.userData.life -= 5;
        MissionManager.instance.MissionClearCheck(9, 5);
        Debug.Log("5라이프 소모: " + DataManager.instance.userData.Life);
        DataManager.instance.SaveUserData();
    }
    public void IncreaseGoods(int expReward, int goldReward, int diaReward) // 보상 경험치, 골드, 다이아 순
    {
        //DataManager.instance.userData.currentExp += expReward; // 캐릭터 경험치로 변경 예정
        DataManager.instance.userData.Gold += goldReward;
        DataManager.instance.userData.Diamond += diaReward;
        DataManager.instance.SaveUserData();
    }
    public void StopUpdateData()
    {
        Debug.LogError("코루틴 중단");
        StopCoroutine(Increaselife());
    }
}
