using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class RankingGameManager : GameManager
{
    [Header("UI")]
    [SerializeField] protected int coinAmount;
    [SerializeField] protected Text coinText;
    
    protected override void Awake()
    {
        instance = this;
        restartButton.onClick.AddListener(() => _SceneManager.instance.GameStart(_SceneManager.stageIdx));
        mainMenuButton.onClick.AddListener(() => _SceneManager.instance.MainMenu());
        Time.timeScale = 1.0f;
        pointText.text = pointAmount.ToString();
        _pointText.text = pointAmount.ToString();

        gameResultRect.transform.localScale = Vector3.zero;
        settingRect.transform.localScale = Vector3.zero;
        resumeButton.onClick.AddListener(() => HidePopup(settingRect));
        homeButton.onClick.AddListener(() => _SceneManager.instance.MainMenu());
        //muteButton.onClick.AddListener();
        
        stageData = StageManager.instance.stageData;
        //DOVirtual.DelayedCall(stageFulltime, () => isWin = true, false);

        var user_Character = DataManager.instance.userData.CurrentCharacterId;
        var characterPrefab = Resources.Load<GameObject>("Prefabs/InGame/Player/" + "c" + user_Character.ToString("D3"));
        Instantiate(characterPrefab, new Vector3(0, -3, 0), new Quaternion(0, 0, 0, 0)).name = "Player";

        PlayerController.instance.maxLevel = 14;
    }

    private void Start()
    {
        StartCoroutine(GetTimePoint());
    }

    IEnumerator GetTimePoint()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            AddPoint((EnemyManager.instance.enemyPatternCount / stageData.stageStep) + 1);
        }
    }

    protected override void SetGame()
    {
        if (isWin) Win();
        else Lose();

        Time.timeScale = 0.0f;
        gameResultRect.DOScale(Vector3.one, 0.2f).SetUpdate(true);
        _pointText.DOCounter(0, pointAmount, 0.5f).SetDelay(0.2f).SetUpdate(true);
    }

    protected override void SetData()
    {
        UserData userData = DataManager.instance.userData;
        RankingManager.instance.NewRecord(userData.guestCode, userData.userName, userData.userIcon, pointAmount);
    }
}
