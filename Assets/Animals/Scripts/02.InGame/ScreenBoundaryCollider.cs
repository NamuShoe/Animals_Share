using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenBoundaryCollider : MonoBehaviour {
    [SerializeField] private Camera camera;
    [SerializeField] private Collider2D[] collider2ds;
    
    void Awake()
    {
        SetColliderScreenBoundary();
    }

    void OnRectTransformDimensionsChange()
    {
        SetColliderScreenBoundary();
    }

    void SetColliderScreenBoundary()
    {
        if (SceneManager.GetActiveScene().name != "Game" || camera == null)
            return;
        
        var temp = camera.ViewportToWorldPoint(Vector3.one);
        var width = temp.x;
        var height = temp.y;

        //Top
        collider2ds[0].transform.position = new Vector3(0, height, 0);
        collider2ds[0].transform.localScale = new Vector3(width * 2, 1, 1);
        
        //Bottom
        collider2ds[1].transform.position = new Vector3(0, -height, 0);
        collider2ds[1].transform.localScale = new Vector3(width * 2, 1, 1);
        
        //Left
        collider2ds[2].transform.position = new Vector3(-width, 0, 0);
        collider2ds[2].transform.localScale = new Vector3(1, height * 2, 1);
        
        //Right
        collider2ds[3].transform.position = new Vector3(width, 0, 0);
        collider2ds[3].transform.localScale = new Vector3(1, height * 2, 1);
    }
}
