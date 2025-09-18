using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleOnOff : MonoBehaviour
{
    private Toggle toggle;
    [SerializeField] private Image onImage;
    [SerializeField] private Image offImage;

    private void Awake()
    {
        toggle = this.GetComponent<Toggle>();
        
        if(onImage && toggle.isOn)
            onImage.gameObject.SetActive(true);
        if(offImage && !toggle.isOn)
            offImage.gameObject.SetActive(true);
        
        toggle.onValueChanged.AddListener(ToggleChanged);
    }

    public void ToggleChanged(bool boolean)
    {
        if(onImage)
            onImage.gameObject.SetActive(boolean);
        if(offImage)
            offImage.gameObject.SetActive(!boolean);
    }
}
