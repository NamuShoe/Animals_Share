using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObjectManager : MonoBehaviour {
    
    public static LoadingObjectManager instance;
    
    [SerializeField] private GameObject LoadingLeft;
    [SerializeField] private GameObject LoadingRight;
    [SerializeField] private Animator animator;
    
    private void Awake(){
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }
    
    
}
