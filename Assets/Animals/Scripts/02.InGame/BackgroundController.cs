using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
//using Microsoft.Unity.VisualStudio.Editor; // 이거 때매 빌드 안되서 주석 처리 해놨음 1/14-이승원
using Unity.Collections;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    //static public BackgroundController instance;

    //private int stageType = 0;
    [SerializeField] private List<Sprite> BGSprites;
    [SerializeField] private List<GameObject> BGGameObjects;
    
    [SerializeField] private Vector2 offset;
    [SerializeField] private float duration = 5f;

    void Awake()
    {
        //instance = this;
    }

    void Start()
    {
        var stageType = StageManager.instance.stageData.stageType;
        BGSprites = Resources.LoadAll<Sprite>("BackGround/" + (stageType).ToString()).ToList();

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        Vector2 size = new Vector2(1440f, 3000f);
        Vector2 anchor = new Vector2(0.5f, 0f);
        float posY = 2810f;
        float posYOffset = -190f;

        int i = 0;
        var tempGameObject = new GameObject("BG", typeof(Image));
        foreach (var sprite in BGSprites)
        {
            var obj = Instantiate(tempGameObject, transform);
            obj.transform.SetAsFirstSibling();
            obj.GetComponent<Image>().sprite = sprite;
            obj.GetComponent<Image>().raycastTarget = false;
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = new Vector2(0f, (posY * i) + posYOffset);
            
            ScrollBG(rect);
            i++;
        }
        Destroy(tempGameObject);
    }

    void ScrollBG(RectTransform rectTransform)
    {
        if (rectTransform.anchoredPosition.y <= -3000f)
        {
            rectTransform.anchoredPosition = new Vector2(0f, rectTransform.anchoredPosition.y + (2810f * (BGSprites.Count)));
            rectTransform.transform.SetAsFirstSibling();
        }

        rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y - 2810f, duration).SetEase(Ease.Linear)
            .OnComplete(() => ScrollBG(rectTransform));
    }
}
