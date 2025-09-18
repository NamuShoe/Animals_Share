using System;
using UnityEditor;
using UnityEngine;

//[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class NotchHelper : MonoBehaviour
{
    private GameObject canvas;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        canvas = GameObject.Find("Canvas");
        rectTransform = this.GetComponent<RectTransform>();
        Notch();
    }
    
    // OnRectTransformDimensionsChange
    void Notch()
    {
        if (canvas == null) return;
        
#if UNITY_EDITOR
        if (PlayModeWindow.GetViewType() == PlayModeWindow.PlayModeViewTypes.GameView) return;
#endif

        float width = Screen.width;
        float height = Screen.height;
        float safeAreaWidth = Screen.safeArea.width;
        float safeAreaHeight = Screen.safeArea.height;
        
        var topOffset = (Screen.safeArea.position + Screen.safeArea.size).y;
        
        //Full stretch 기준 0, 0, 1, 1
        topOffset /= Screen.height;
        //Debug.LogError($"\t minAnchor {minAnchor}\t maxAnchor {maxAnchor}");

        //rectTransform.anchorMax = maxAnchor;
        
        // 실제 비율과 SafeArea 비율이 같을 경우
        if (!Mathf.Approximately((width / height), (safeAreaWidth / safeAreaHeight))) {
            if (rectTransform.anchorMin.y >= 1f) {
                var min = rectTransform.anchorMin;
                min.y = topOffset;
                rectTransform.anchorMin = min;
            }

            if (rectTransform.anchorMax.y >= 1f) {
                var max = rectTransform.anchorMax;
                max.y = topOffset;
                rectTransform.anchorMax = max;
            }
        }
    }
}
