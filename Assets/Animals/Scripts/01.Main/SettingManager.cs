using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [Header("BGM")] 
    [SerializeField] private Toggle BGMToggle;
    [SerializeField] private Slider BGMSlider;

    [Header("SFX")] 
    [SerializeField] private Toggle SFXToggle;
    [SerializeField] private Slider SFXSlider;

    private void Start()
    {
        BGMToggle.onValueChanged.AddListener(SetBGMToggle);
        BGMSlider.onValueChanged.AddListener(SetBGMSlider);
        
        SFXToggle.onValueChanged.AddListener(SetSFXToggle);
        SFXSlider.onValueChanged.AddListener(SetSFXSlider);
        
        Load();
    }

    private void Load()
    {
        BGMToggle.isOn = PlayerPrefs.GetInt("BGMToggle", 1) == 1;
        BGMToggle.GetComponent<ToggleOnOff>().ToggleChanged(BGMToggle.isOn);
        BGMSlider.value = PlayerPrefs.GetFloat("BGMSlider", 0.75f);
        
        // SetBGMToggle(BGMToggle.isOn);
        SetBGMSlider(BGMSlider.value);

        SFXToggle.isOn = PlayerPrefs.GetInt("SFXToggle", 1) == 1;
        SFXToggle.GetComponent<ToggleOnOff>().ToggleChanged(SFXToggle.isOn);
        SFXSlider.value = PlayerPrefs.GetFloat("SFXSlider", 0.75f);
        
        // SetSFXToggle(SFXToggle.isOn);
        SetSFXSlider(SFXSlider.value);
    }
    
    /*private void Save()
    {
        PlayerPrefs.SetInt("BGMOn", BGMToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("BGMValue", BGMSlider.value);
        
        PlayerPrefs.SetInt("SFXOn", SFXToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("SFXValue", SFXSlider.value);
    }*/

    private void SetBGMToggle(bool isOn) {
        PlayerPrefs.SetInt("BGMToggle", Convert.ToInt32(isOn)); 
        if(isOn) SoundManager.instance.EnableBGM();
        else SoundManager.instance.DisableBGM(); 
    }
    private void SetBGMSlider(float value) { 
        PlayerPrefs.SetFloat("BGMSlider", value);
        SoundManager.instance.SetBGMVolume(value);
    }

    private void SetSFXToggle(bool isOn) {
        PlayerPrefs.SetInt("SFXToggle", Convert.ToInt32(isOn));
        if(isOn) SoundManager.instance.EnableSFX();
        else SoundManager.instance.DisableSFX();
    }

    private void SetSFXSlider(float value)
    {
        PlayerPrefs.SetFloat("SFXSlider", value);
        SoundManager.instance.SetSFXVolume(value);
    }
}
