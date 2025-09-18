using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Canvas), (typeof(CanvasScaler)))]
public class CanvasScreenMatchControl : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    
    void Awake()
    {
        if (canvas == null)
            canvas = this.GetComponent<Canvas>();
        if (canvasScaler == null)
            canvasScaler = this.GetComponent<CanvasScaler>();
    }

    private void Reset()
    {
        canvas = this.GetComponent<Canvas>();
        canvasScaler = this.GetComponent<CanvasScaler>();
    }

    void OnRectTransformDimensionsChange()
    {
        float mobileRatio = 16.0f / 9.0f; //1.777
        float tabletRatio = 4.0f / 3.0f; //1.333

        var canvasRect = canvas.GetComponent<RectTransform>().rect;
        float ratio = canvasRect.height / canvasRect.width;
        
        if (ratio >= mobileRatio)
            canvasScaler.matchWidthOrHeight = 0f;
        else if (tabletRatio >= ratio)
            canvasScaler.matchWidthOrHeight = 1f;
        else
        {
            float t = (ratio - tabletRatio) / (mobileRatio - tabletRatio);
            canvasScaler.matchWidthOrHeight = Mathf.Lerp(1f, 0f, t);
        }
    }
}
