using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Collections;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine.Serialization;

public class MainManager : MonoBehaviour
{
	public static MainManager instance;
	
	[Header("Main")]
    [SerializeField] private GameObject[] menu;
    [SerializeField] private Button[] bottomButton;
    [SerializeField] private Text[] bottomButtonText;
    [SerializeField] private GameObject PlayerIcon;
    [SerializeField] private GameObject GoodsUI; // 재화 UI
    [SerializeField] private MainScrollController scrollController;

    [Header("PlayerProfile")]
    [SerializeField] private Image playerProfileIcon; // 플레이어 아이콘(프로필 팝업)
    [SerializeField] private Image playerProfileIconPopup;
    [SerializeField] private Text playerNickName;
    [SerializeField] private Text playerBestScore;
    [SerializeField] private Transform profileIconsTransform;

    [Space(20f)]
    [SerializeField] private Button GameStartButton;

    [HideInInspector] public int currentMenuNum = 2;

    void  Awake()
    {
	    instance = this;
        InitMenu();
        Time.timeScale = 1f;
        SoundManager.instance.SetSFXPitchByTimeScale();
    }

    // void Update()
    // {
    //     UpdateInput();
    // }

    // private void UpdateInput()
	// {
	// 	#if UNITY_EDITOR
	// 	// 마우스 왼쪽 버튼을 눌렀을 때 1회
	// 	if ( Input.GetMouseButtonDown(0) )
	// 	{
	// 		// 터치 시작 지점 (Swipe 방향 구분)
	// 		startTouch = Input.mousePosition;
	// 	}
	// 	else if ( Input.GetMouseButtonUp(0) )
	// 	{
	// 		// 터치 종료 지점 (Swipe 방향 구분)
	// 		endTouch = Input.mousePosition;

	// 		UpdateSwipe();
	// 	}
	// 	#endif

	// 	#if UNITY_ANDROID
	// 	if ( Input.touchCount == 1 )
	// 	{
	// 		Touch touch = Input.GetTouch(0);

	// 		if ( touch.phase == TouchPhase.Began )
	// 		{
	// 			// 터치 시작 지점 (Swipe 방향 구분)
	// 			startTouch = touch.position;
	// 		}
	// 		else if ( touch.phase == TouchPhase.Ended )
	// 		{
	// 			// 터치 종료 지점 (Swipe 방향 구분)
	// 			endTouch = touch.position;

	// 			UpdateSwipe();
	// 		}
	// 	}
	// 	#endif
	// }

    // private void UpdateSwipe()
    // {
    //     if(Mathf.Abs(startTouch.x - endTouch.x) < swipeDistance ||
    //         Mathf.Abs(startTouch.x - endTouch.x) < Mathf.Abs(startTouch.y - endTouch.y))
    //     {
    //         //원래 페이지로 Swipe해서 돌아간다
    //         return;
    //     }

    //     bool isLeft = startTouch.x < endTouch.x ? true : false;

    //     if(isLeft == true)
    //         previousMenu();
    //     else
    //         nextMenu();
    //     //currentMenuNum 페이지로 Swipe해서 이동
    // }

    // private void SwipeMenu()
    // {

    // }

    /// <summary>
    /// 메뉴 초기화
    /// </summary>
    private void InitMenu(){
        for (int i = 0; i < menu.Length; i++) 
            menu[i].SetActive(true);

        for(int i = 0; i < bottomButton.Length; i++){
            var index = i;
            bottomButton[i].onClick.AddListener(() => ShowMenu(index));
            //bottomButton[i].GetComponent<Image>().color = Color.white;
        }

        // currentMenuNum = 2;
        ShowMenu(2, false);
        // OpenMenu(2);
        
        GameStartButton.onClick.AddListener(() => SoundManager.instance.PlayBGM(SoundManager.MAIN_BGM.Select));
    }
    
	//testMode에서 사용하기 위해 임시적으로 Public
    public void ProfileSet()
    {
	    string path = "CharacterList/CharacterThumbnail";
	    var icons = Resources.LoadAll<Sprite>(path);

	    foreach (var icon in icons)
	    {
            //캐릭터 num은 1부터 시작을 하나 child는 0부터 시작하기에 -1을 추가
		    int num = int.Parse(Regex.Replace(icon.ToString(), @"[^0-9]", "")) - 1;
		    Transform profileIcon = profileIconsTransform.GetChild(num).transform;
		    profileIcon.GetChild(0).gameObject.SetActive(true);
		    profileIcon.GetChild(0).GetComponent<Image>().sprite = icon;
		    profileIcon.GetChild(0).GetComponent<Image>().color = Color.black;
		    
		    profileIcon.GetChild(1).gameObject.SetActive(true);
		    
		    profileIcon.GetComponent<Button>().onClick.RemoveAllListeners();
		    profileIcon.GetComponent<Button>().onClick.AddListener(() => ChangeIcon(num));
		    profileIcon.GetComponent<Button>().interactable = false;
	    }
   
	    for (int i = 0; i < DataManager.instance.userData.characterList.Count; i++)
	    {
		    Transform profileIcon = profileIconsTransform.GetChild(i).transform;
		    profileIcon.GetChild(0).GetComponent<Image>().color = Color.white;
		    profileIcon.GetChild(1).gameObject.SetActive(false);
		    profileIcon.GetComponent<Button>().interactable = true;
	    }
	    
	    ChangeIcon(DataManager.instance.userData.userIcon);
    }

    private void ChangeIcon(int num)
    {
	    DataManager.instance.userData.userIcon = num;
	    string path = "CharacterList/CharacterThumbnail/c";
	    playerProfileIcon.sprite = Resources.Load<Sprite>(path + (num + 1).ToString("D3"));
	    playerProfileIconPopup.sprite = Resources.Load<Sprite>(path + (num + 1).ToString("D3"));
    }

    /// <summary>
    /// 버튼 설정 
    /// </summary>
    private void ShowMenu(int n, bool isSound = true)
    {
	    scrollController.SelectedNum = n; //드래그시 2번 호출
        // 버튼 활성 비활성 처리
        InteractableButton(false);

        // 버튼 색상 및 너비 아이콘 설정
        Sequence Seq = DOTween.Sequence();
        RectTransform previousButtonImageTransform = bottomButton[currentMenuNum].transform.GetComponent<RectTransform>();
        RectTransform buttonImageTransform = bottomButton[n].transform.GetComponent<RectTransform>();

        // bottomButton[currentMenuNum].GetComponent<Image>().color = new Color32(255, 255, 255, 255);
        // bottomButton[n].GetComponent<Image>().color = new Color32(255, 255, 255, 255);

        Seq.Append(DOTween.To(() => bottomButton[currentMenuNum].transform.parent.gameObject.GetComponent<LayoutElement>().flexibleWidth, x => bottomButton[currentMenuNum].transform.parent.gameObject.GetComponent<LayoutElement>().flexibleWidth = x, 1.0f, 0.1f))
        .Join(DOTween.To(() => previousButtonImageTransform.anchoredPosition, x => previousButtonImageTransform.anchoredPosition = x, previousButtonImageTransform.anchoredPosition + new Vector2(0, -30f), 0.1f))
        .Join(DOTween.To(() => bottomButtonText[currentMenuNum].GetComponent<Text>().fontSize, x => bottomButtonText[currentMenuNum].fontSize = x, 58, 0.1f))
        .Join(DOTween.To(() => previousButtonImageTransform.localScale, x => previousButtonImageTransform.localScale = x, new Vector3(1, 1, 1), 0.1f))

        .Join(DOTween.To(() => bottomButton[n].transform.parent.gameObject.GetComponent<LayoutElement>().flexibleWidth, x => bottomButton[n].transform.parent.gameObject.GetComponent<LayoutElement>().flexibleWidth = x, 1.2f, 0.1f))
        .Join(DOTween.To(() => buttonImageTransform.anchoredPosition, x => buttonImageTransform.anchoredPosition = x, buttonImageTransform.anchoredPosition + new Vector2(0, 30f), 0.1f))
        .Join(DOTween.To(() => bottomButtonText[n].GetComponent<Text>().fontSize, x => bottomButtonText[n].fontSize = x, 64, 0.1f))
        .Join(DOTween.To(() => buttonImageTransform.localScale, x => buttonImageTransform.localScale = x, new Vector3(1.2f, 1.2f, 1), 0.1f));

        if (n != 2){
            PlayerIcon.SetActive(false);
        }
        else{
            PlayerIcon.SetActive(true);
        }

        if (n != 3) { // 재화 활성
            GoodsUI.SetActive(true);
        }
        else { // 플레이어 재화 랭킹에서 비활성
            GoodsUI.SetActive(false);
        }

        Seq.AppendCallback(() => { scrollController.previousNum = currentMenuNum; });
        Seq.AppendCallback(() => { currentMenuNum = n; });
        Seq.AppendCallback(() => InteractableButton(true));
        Seq.AppendCallback(() => { bottomButton[n].interactable = false; });
        
        if(isSound)
			SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Button);
    }

    public void OpenMenu(int n)
    {
	    bottomButton[n].onClick.Invoke();
    }
    /*public void previousMenu()
    {
        if(currentMenuNum == 0) 
            return;
        bottomButton[currentMenuNum - 1].onClick.Invoke();
    }
    public void nextMenu()
    {
        if(currentMenuNum == maxMenuNum - 1)
            return;
        bottomButton[currentMenuNum + 1].onClick.Invoke();
    }

    public bool isMinMax(bool isLeft)
    {
        if((currentMenuNum == 0 && isLeft)|| 
            (currentMenuNum == maxMenuNum - 1 && !isLeft)) 
            return true;
        else
            return false;
    }*/
    public void PlayerInformChange() // 플레이어 아이콘 번호
    {
	    ProfileSet();
	    
	    string path = "CharacterList/CharacterThumbnail/c";
	    playerProfileIcon.sprite = Resources.Load<Sprite>(path + (DataManager.instance.userData.userIcon + 1).ToString("D3"));
        playerBestScore.text = DataManager.instance.userData.rankingScore.ToString();
        playerNickName.text = DataManager.instance.userData.userName.ToString();
    }
    private void InteractableButton(bool isInteractable)
    {
        for (int i = 0; i < bottomButton.Length; i++)
            bottomButton[i].interactable = isInteractable;
    }
}
