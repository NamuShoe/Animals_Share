using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(GridLayoutGroup))] 
public class FlexibleGrid : MonoBehaviour
{
    private GridLayoutGroup grid;

    public int row;
    public int column;
    public bool setHeight;

    private void Start()
    {   
        grid = this.GetComponent<GridLayoutGroup>();
    }

    private void Update() 
    {
        float width = GetComponent<RectTransform>().rect.width;
        Vector2 newSize; 
        if (setHeight)
        {
            float height = GetComponent<RectTransform>().rect.height;
            newSize = new Vector2(width / row - (grid.spacing.x), height / column - (grid.spacing.y));
        }
        else
        {
            newSize = new Vector2(width / row - (grid.spacing.x), width / column - (grid.spacing.y));
        }
        
        grid.cellSize = newSize;

        for (int i = 0; i < this.transform.childCount; i++)
        {
            this.transform.GetChild(i).GetComponent<BoxCollider2D>().size = this.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta;
        }
    }

}
