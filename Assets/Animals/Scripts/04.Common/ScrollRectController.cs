using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler 
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private MainScrollController mainScrollController;

    private bool isHorizeontal = false;

    private void Reset()
    {
        scrollRect = GetComponent<ScrollRect>();
        mainScrollController = FindObjectOfType<MainScrollController>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isHorizeontal = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);

        if (isHorizeontal) {
            mainScrollController.OnBeginDrag(eventData);
            scrollRect.vertical = false;
        }
        // else {
        //     scrollRect.OnBeginDrag(eventData);
        // }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isHorizeontal) {
            mainScrollController.OnDrag(eventData);
        }
        // else {
        //     scrollRect.OnDrag(eventData);
        // }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isHorizeontal) {
            mainScrollController.OnEndDrag(eventData);
            scrollRect.vertical = true;
        }
        // else {
        //     scrollRect.OnEndDrag(eventData);
        // }
    }
}
