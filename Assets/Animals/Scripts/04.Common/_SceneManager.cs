using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class _SceneManager : MonoBehaviour
{
    public static _SceneManager instance;
    public static int stageIdx;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    public void GameStart(int _stageIdx)
    {
        GoodsManager.instance.ConsumeLife();
        DataManager.instance.RecordEndTime();
        DOTween.KillAll();
        
        if(GoodsManager.instance != null)
            GoodsManager.instance.StopCoroutine("Increaselife");
        //RankingManager.instance.StopCoroutine("SetRank");
        SoundManager.instance.StopBGM();
        SceneManager.LoadScene("Game");
        stageIdx = _stageIdx;
        
        DataManager.instance.SaveUserData();
    }

    public void RankingGameStart(int _stageIdx)
    {
        DOTween.KillAll();
        SoundManager.instance.StopBGM();
        SceneManager.LoadScene("Ranking");
        stageIdx = _stageIdx;
    }

    public void MainMenu()
    {
        DOTween.KillAll();
        SoundManager.instance.StopBGM();
        SceneManager.LoadScene("Main");
    }
}
