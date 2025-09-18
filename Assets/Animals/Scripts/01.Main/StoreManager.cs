using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    private RectTransform scrollRectTransform;
    private RectTransform viewPortRectTransform;

    private void Awake()
    {
        if (scrollRect == null) return;
        scrollRectTransform = scrollRect.transform as RectTransform;
        viewPortRectTransform = scrollRect.viewport;
    }

    public void GotoItem(int childIndex)
    {
        if (MainManager.instance.currentMenuNum != 1)
        {
            MainManager.instance.OpenMenu(1);
            DOVirtual.DelayedCall(0.2f, () => CenterOnItem(childIndex), true);
        }
        else
        {
            CenterOnItem(childIndex) ;
        }
    }
    
    public void GotoItemReverse(int childIndex)
    {
        if (MainManager.instance.currentMenuNum != 1)
        {
            MainManager.instance.OpenMenu(1);
            DOVirtual.DelayedCall(0.2f, () => CenterOnItem(childIndex, false), true);
        }
        else
        {
            CenterOnItem(childIndex, false) ;
        }
    }

    private void CenterOnItem(int childIndex, bool isReverse = true)
    {
        if(isReverse == false)
            childIndex = scrollRect.content.childCount - 1 - childIndex;
        
        RectTransform target = scrollRect.content.GetChild(childIndex).transform as RectTransform;
        
        var itemCenterPositionInScroll = GetWorldPointInWidget(scrollRectTransform, GetWidgetWorldPoint(target));
        var targetPositionInScroll = GetWorldPointInWidget(scrollRectTransform, GetWidgetWorldPoint(viewPortRectTransform));
        var difference = targetPositionInScroll - itemCenterPositionInScroll;
        difference.z = 0f;
        
        //clear axis data that is not enabled in the scrollrect
        if (!scrollRect.horizontal)
            difference.x = 0f;
        if (!scrollRect.vertical)
            difference.y = 0f;
        
        var normalizedDifference = new Vector2(
            difference.x / (scrollRect.content.rect.size.x - scrollRectTransform.rect.size.x),
            difference.y / (scrollRect.content.rect.size.y - scrollRectTransform.rect.size.y));
 
        var newNormalizedPosition = scrollRect.normalizedPosition - normalizedDifference;
        if (scrollRect.movementType != ScrollRect.MovementType.Unrestricted)
        {
            newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
            newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
        }
 
        scrollRect.DONormalizedPos(newNormalizedPosition, 0.2f);
        DOVirtual.DelayedCall(0.2f, () => HighlightItem(target));
    }

    private void HighlightItem(RectTransform target)
    {
        target.DOShakePosition(1.0f, 10f);
    }
    
    private Vector3 GetWidgetWorldPoint(RectTransform target)
    {
        //pivot position + item size has to be included
        var pivotOffset = new Vector3(
            (0.5f - target.pivot.x) * target.rect.size.x,
            (0.5f - target.pivot.y) * target.rect.size.y,
            0f);
        var localPosition = target.localPosition + pivotOffset;
        return target.parent.TransformPoint(localPosition);
    }
    private Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
    {
        return target.InverseTransformPoint(worldPoint);
    }
}
