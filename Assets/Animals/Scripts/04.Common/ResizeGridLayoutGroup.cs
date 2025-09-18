using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup), (typeof(RectTransform)))]
public class ResizeGridLayoutGroup : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;
    [SerializeField] private bool isPedia = false;

    void Start()
    {
        if(rectTransform == null) rectTransform = this.GetComponent<RectTransform>();
        if(gridLayoutGroup == null) gridLayoutGroup = this.GetComponent<GridLayoutGroup>();
    }

    void Reset()
    {
        rectTransform = this.GetComponent<RectTransform>();
        gridLayoutGroup = this.GetComponent<GridLayoutGroup>();
    }

    void OnRectTransformDimensionsChange()
    {
        //cell size
        ResizeCellSize();
        //if(isPedia) PediaManager.instance.ResizeAnimalKindAll();
    }

    void ResizeCellSize()
    {
        //Canvas.ForceUpdateCanvases();
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        Vector2 spacing = gridLayoutGroup.spacing;
        int constraintCount = gridLayoutGroup.constraintCount;

        width = width - (spacing.x * (constraintCount - 1)) - gridLayoutGroup.padding.left - gridLayoutGroup.padding.right;
        float widthSize = (width / constraintCount);
        height = height - (spacing.y * (constraintCount - 1)) - gridLayoutGroup.padding.top - gridLayoutGroup.padding.bottom;
        float heightSize = (height / constraintCount);

        if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
            gridLayoutGroup.cellSize = new Vector2(widthSize, widthSize);
        else if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
            gridLayoutGroup.cellSize = new Vector2(heightSize, heightSize);

        if (isPedia) ResizeForPedia();
    }

    void ResizeForPedia()
    {
        int count = (transform.childCount / (4 + 1)) + 1;
        
        var delta = transform.parent.GetComponent<RectTransform>().sizeDelta;
        delta.y = (gridLayoutGroup.cellSize.y * count) + (gridLayoutGroup.spacing.y * (count - 1)) +
                  gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;
        transform.parent.GetComponent<RectTransform>().sizeDelta = delta;
    }
}
