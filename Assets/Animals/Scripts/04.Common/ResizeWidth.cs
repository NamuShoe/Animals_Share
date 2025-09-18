using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(LayoutGroup))]
public class ResizeWidth : UIBehaviour
{
    private GameObject canvas;
    private GridLayoutGroup gridLayoutGroup;
    private List<RectTransform> rectTransforms;

    protected override void Awake()
    {
        canvas = GameObject.Find("Canvas");
        rectTransforms = new List<RectTransform>();

        if(gridLayoutGroup == null)
            gridLayoutGroup = this.GetComponent<GridLayoutGroup>();
        
        if (GetComponent<HorizontalLayoutGroup>() != null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                rectTransforms.Add(transform.GetChild(i).GetComponent<RectTransform>());
            }
        }
    }

    new void OnRectTransformDimensionsChange()
    {
        if (canvas == null) return;
        float width = canvas.GetComponent<RectTransform>().rect.width;
        
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = new Vector2(width, gridLayoutGroup.cellSize.y);
            return;
        }

        if (this.GetComponent<HorizontalLayoutGroup>() != null)
        {
            foreach (var rectTransform in rectTransforms)
            {
                rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
            }
        }
    }
}
