using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(ScrollRect))]
public class ScrollController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Transform content;
    [SerializeField] private Scrollbar scrollbar;
    [SerializeField] private bool isSnap = false;
    [SerializeField] private int ignoreCount = 0;

    [SerializeField] public ScrollRect parentScrollRect;
    [SerializeField] public ScrollRect myScrollRect;
    private bool isHorizeontal = false;
    
    [Header("Indicator")]
    [SerializeField] private GameObject indicatorLeft;
    [SerializeField] private GameObject indicatorRight;
    private Tween tween;
    private float distance = 0f;
    
    private int selectedNum;
    public int SelectedNum
    {
        get => selectedNum;
        set => SetSelected(value);
    }
    public int previousNum;
    private void SetSelected(int num)
    {
        int clamp = Mathf.Clamp(num, 0, content.childCount - 1 - ignoreCount);
        previousNum = selectedNum;
        selectedNum = clamp;
        float value = 1f / (content.childCount - 1f - ignoreCount);

        if (tween != null && tween.IsActive() && tween.IsPlaying()) {
            tween.Pause();
            tween.Kill();
            tween = null;
        }
        
        if(isSnap)
            tween = DOTween.To(() => scrollbar.value, x => scrollbar.value = x, value * clamp, 0.3f)
            .SetEase(Ease.InCubic).OnComplete(SetActiveIndicator);
        else 
            tween = DOTween.To(() => scrollbar.value, x => scrollbar.value = x, value * clamp, 0.3f)
                .OnComplete(SetActiveIndicator);
    }

    private void SetActiveIndicator()
    {
        if (indicatorLeft == null || indicatorRight == null) return;
        
        if (selectedNum == 0)
        {
            indicatorLeft.SetActive(false);
            indicatorRight.SetActive(true);
        }
        else if (selectedNum == content.childCount - 1 - ignoreCount)
        {
            indicatorLeft.SetActive(true);
            indicatorRight.SetActive(false);
        }
        else
        {
            indicatorLeft.SetActive(true);
            indicatorRight.SetActive(true);
        }
    }

    private void Reset()
    {
        myScrollRect = GetComponent<ScrollRect>();
        content = myScrollRect.content;
        if(myScrollRect.horizontalScrollbar || myScrollRect.verticalScrollbar)
            scrollbar = myScrollRect.horizontalScrollbar ? myScrollRect.horizontalScrollbar : myScrollRect.verticalScrollbar;
        else
            Debug.LogError("ScrollController : There's no ScrollBar");
    }
    
    private void Start()
    {
        if(indicatorLeft != null)
            indicatorLeft.SetActive(false);
        if(indicatorRight != null) 
            indicatorRight.SetActive(true);
        
        if (content.childCount is 0 or 1) {
            if(indicatorLeft != null)
                indicatorLeft.SetActive(false);
            if(indicatorRight != null) 
                indicatorRight.SetActive(false);
        }
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        if (tween != null && tween.IsActive() && tween.IsPlaying()) {
            tween.Pause();
            tween.Kill();
            tween = null;
        }
        
        if (parentScrollRect == null) return;
        
        isHorizeontal = Mathf.Abs(eventData.delta.x) > Mathf.Abs(eventData.delta.y);
        
        if (isHorizeontal == false) {
            parentScrollRect.OnBeginDrag(eventData);
            myScrollRect.horizontal = false;
        }
        else {
            myScrollRect.OnBeginDrag(eventData);
        }
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect == null) return;
        
        if (isHorizeontal == false) {
            parentScrollRect.OnDrag(eventData);
        }
        else {
            myScrollRect.OnDrag(eventData);
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null && isHorizeontal == false) {
            parentScrollRect.OnEndDrag(eventData);
            myScrollRect.horizontal = true;
        }
        
        if (content.childCount is 0 or 1) return;

        if (Mathf.Abs(eventData.delta.x) > 20f)
        {
            if (isSnap)
            {
                StartCoroutine(OnSnap());
                return;
            }
            else
            {
                if (eventData.delta.x > 0)
                    SelectedNum--;
                else
                    SelectedNum++;
                return;
            }
        }
        FindSelectedNum();
    }

    public void FindSelectedNum()
    {
        float value = Mathf.Clamp(scrollbar.value, 0f, 1f);
        
        distance = 1f / (content.childCount - 1);
        for (int i = 0; i < content.childCount; i++)
        {
            if (distance * ((float)i - 0.5f) < value && value < distance * ((float)i + 0.5f))
            {
                SelectedNum = i;
                break;
            }
        }
    }

    private IEnumerator OnSnap()
    {
        ScrollRect scrollRect = GetComponent<ScrollRect>();
        yield return new WaitUntil(() => UpdateSnap(scrollRect));
        FindSelectedNum();
    }

    private bool UpdateSnap(ScrollRect scrollRect)
    {
        if (scrollRect.velocity.magnitude < 800)
            return true;
        return false;
    }
}
