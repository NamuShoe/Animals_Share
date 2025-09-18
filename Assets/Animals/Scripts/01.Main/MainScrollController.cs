using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MainScrollController : ScrollController
{
    [SerializeField] MainManager mainManager;
    public ScrollRect scrollRect;

    public override void OnBeginDrag(PointerEventData eventData)
    {
        scrollRect.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        scrollRect.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        if (previousNum == SelectedNum) return;
        mainManager.OpenMenu(SelectedNum);
    }
}
