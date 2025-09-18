using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public enum MAIN_BGM
    {
        Morning = 0,
        Night,
        Select
    }

    public enum GAME_BGM {
        Forest,
        Village,
        Desert,
        Wetland,
        Waterfront,
        Boss
    }
    
    [Header("BGM")]
    [SerializeField] private AudioSource mainBGM;
    [SerializeField] private AudioSource subBGM;
    [SerializedDictionary][SerializeField] private SerializedDictionary<MAIN_BGM, AudioClip> Main_BgmClip;
    [SerializedDictionary][SerializeField] private SerializedDictionary<GAME_BGM, AudioClip> Game_BgmClip;
    private MAIN_BGM playingMainBGM;
    private GAME_BGM playingGameBGM;

    public enum UI_SFX
    {
        Button = 0,
        Close,
        Select,
        Book,
        Upgrade,
        Skill,
        Defeat,
        Victory,
    }

    public enum GAME_SFX
    {
        Damage = 0,
        Collision,
        Avoid,
        
        Bow,
        Dart,
        Boomerang,
        BoomerangSpin
    }

    [Header("SFX")] 
    [SerializeField] private AudioSource uiSFX;
    [SerializeField] private AudioSource SFX;
    [SerializedDictionary][SerializeField] private SerializedDictionary<UI_SFX, AudioClip> Ui_SfxClip;
    [SerializedDictionary][SerializeField] private SerializedDictionary<GAME_SFX, AudioClip> Game_SfxClip;

    private void Awake(){
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    } 
    public void InitSound()
    {
        bool bgmTrigger = PlayerPrefs.GetInt("BGMToggle", 1) == 1;
        if(bgmTrigger) EnableBGM();
        else DisableBGM();

        bool sfxTrigger = PlayerPrefs.GetInt("SFXToggle", 1) == 1;
        if(sfxTrigger) EnableSFX();
        else DisableSFX();
    }

    public void ChangeBgmInTime(bool isDay)
    {
        playingMainBGM = isDay ? MAIN_BGM.Morning : MAIN_BGM.Night;
        //PlayBGM(playingMainBGM);
        //mainBGM.Play();
        //mainBGM.Pause();
    }
    
    public void PlayOneShot(UI_SFX n)
    {
        if (uiSFX.enabled) uiSFX.PlayOneShot(Ui_SfxClip[n]);
    }

    public void PlayOneShot(GAME_SFX n)
    {
        if (SFX.enabled) SFX.PlayOneShot(Game_SfxClip[n]);
    }

    public void EnableBGM()
    {
        mainBGM.enabled = true;
        subBGM.enabled = true;
        PlayBGM(playingMainBGM);
    }
    public void DisableBGM()
    {
        mainBGM.enabled = false;
        subBGM.enabled = false;
    }
    public void EnableSFX()
    {
        uiSFX.enabled = true;
        SFX.enabled = true;
    }

    public void DisableSFX()
    {
        uiSFX.enabled = false;
        SFX.enabled = false;
    }

    public void SetBGMVolume(float volume)
    {
        mainBGM.volume = volume;
    }
    
    public void SetSFXVolume(float volume)
    {
        uiSFX.volume = volume;
        SFX.volume = volume;
    }

    public void PlayBGM() => mainBGM.Play();
    public void StopBGM()
    {
        mainBGM.Stop();
        subBGM.Stop();
    }

    public void PlayMainBGM() => PlayBGM(playingMainBGM);

    public void PlayBGM(MAIN_BGM n)
    {
        mainBGM.Stop();
        mainBGM.clip = Main_BgmClip[n];
        mainBGM.Play();
    }

    public void PlayBGM(GAME_BGM n)
    {
        mainBGM.Stop();
        subBGM.Stop();
        mainBGM.clip = Game_BgmClip[n];
        playingGameBGM = n;
        mainBGM.Play();
    }

    public void PlayBGMFade(GAME_BGM n)
    {
        mainBGM.DOFade(0f, 5f);
        
        subBGM.volume = 0f;
        subBGM.clip = Game_BgmClip[n];
        var value = PlayerPrefs.GetFloat("BGMSlider", 0.75f);
        subBGM.Play();
        subBGM.DOFade(value, 5f);
    }

    public void SetSFXPitchByTimeScale()
    {
        SFX.pitch = Time.timeScale;
    }
}
