using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TapManager : MonoBehaviour
{
    [Header("Main -> Notice")]
    [SerializeField] private Button mainMissonBtn;
    [SerializeField] private Button subMissonBtn;
    [SerializeField] private Button mailBtn;


    [Header("Pedia -> CharacterInfo")]
    [SerializeField] private Button statBtn;
    [SerializeField] private Button skilBtn;

    private void Start()
    {
        mainMissonBtn.onClick.AddListener(() => TapTool.ShowTap(mainMissonBtn.gameObject));
        subMissonBtn.onClick.AddListener(() => TapTool.ShowTap(subMissonBtn.gameObject));
        mailBtn.onClick.AddListener(() => TapTool.ShowTap(mailBtn.gameObject));
        statBtn.onClick.AddListener(() => TapTool.ShowTap(statBtn.gameObject));
        skilBtn.onClick.AddListener(() => TapTool.ShowTap(skilBtn.gameObject));
    }
    public static class TapTool{
        public static void ShowTap(GameObject TapButton){
            TapButton.transform.parent.SetAsLastSibling();
            SoundManager.instance.PlayOneShot(SoundManager.UI_SFX.Book);
        }
    }
}
