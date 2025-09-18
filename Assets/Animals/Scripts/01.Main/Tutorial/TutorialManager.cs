using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    [SerializeField] private bool resetTutorial = false;

    [Header("Tutorial Canvas")]
    [SerializeField] GameObject tutorialGameObject;

    [SerializeField] GameObject backgroundPanel;
    [SerializeField] GameObject highlight;
    [SerializeField] GameObject dialogue;

    [Header("DialogueData")] [SerializeField]
    TutorialData[] dialogDatasets;

    private bool[] isFirstEvent;

    public GameObject GetHighlight()
    {
        return highlight;
    }

    public GameObject GetDialogue()
    {
        return dialogue;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if(resetTutorial)
            DeleteDialogData();
        
        isFirstEvent = new bool[dialogDatasets.Length];
        for (int i = 0; i < dialogDatasets.Length; i++)
        {
            PlayerPrefs.SetInt($"DialogEventSeen_{i}", 1);
            isFirstEvent[i] = PlayerPrefs.GetInt($"DialogEventSeen_{i}", 0) == 0;
        }
        
        tutorialGameObject.SetActive(true);
        StartTutorial(0);
    }

    private void StartTutorial(int dialogIndex)
    {
        // Test
        while (dialogIndex < dialogDatasets.Length)//
        {//
            if (isFirstEvent[dialogIndex])
            {
                StartCoroutine(Tutorial(dialogIndex));
                break;//
            }
            else
            {
                dialogIndex++;
                continue;
            }
        }//
    
        if(dialogIndex >= dialogDatasets.Length)
            DestroyTutorial();
        
    }

    private IEnumerator Tutorial(int index)
    {
        if (dialogDatasets[index].isDialog1st)
        {
            yield return new WaitUntil(() => dialogDatasets[index].UpdateDialog());
            yield return new WaitUntil(() => dialogDatasets[index].UpdateFocus());
        }
        else
        {
            yield return new WaitUntil(() => dialogDatasets[index].UpdateFocus());
            yield return new WaitUntil(() => dialogDatasets[index].UpdateDialog());
        }

        PlayerPrefs.SetInt($"DialogEventSeen_{index}", 1);
        // Test
        StartTutorial(index + 1);//
    }

    private void DeleteDialogData()
    {
#if UNITY_EDITOR
        for (int i = 0; i < dialogDatasets.Length; i++)
        {
            PlayerPrefs.DeleteKey($"DialogEventSeen_{i}");
            Debug.Log("유저 튜토리얼 정보 삭제");
        }
#endif
    }

    public void SetHighlight(RectTransform rect)
    {
        highlight.transform.SetParent(rect.transform);
        highlight.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
        highlight.transform.SetParent(tutorialGameObject.transform);
        highlight.transform.SetAsFirstSibling();
    }

    void DestroyTutorial()
    {
        Destroy(tutorialGameObject.gameObject);
        Destroy(this.gameObject);
    }
}