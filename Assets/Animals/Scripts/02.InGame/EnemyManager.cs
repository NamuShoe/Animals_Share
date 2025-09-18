using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Redcode.Pools;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    [SerializeField] private RectTransform spawnRect;

    [Range(2, 50)]
    [SerializeField] private int spawnPositionCount; //스폰 위치 세분화
    [HideInInspector] public List<RectTransform> spawnPosition; //실제 스폰 위치
    public List<RectTransform> rangedSpawnPosition;
    public List<bool> isRangedSpawnPositionUsing;
    [SerializeField] private GameObject enemyContainer;

    [Header("StageData")]
    private StageData stageData;
    [SerializeField] private float spawnDuration = 10f; // 스폰 쿨타임
    [SerializeField] private int spawnCount; // 스폰 시, 한번에 나오는 횟수
    public float spawnCountGoal = 0;

    [FormerlySerializedAs("killCount")]
    [Header("StageParameter")] 
    [SerializeField] private int killCount = 0;
    [SerializeField] private int totalKillCount = 0;
    public int TotalKillCount => totalKillCount;
    public int killCountGoal = 0; // 확인용
    [SerializeField] public int enemyPatternCount = 0; // 적이 나오는 패턴

    private int EnemyPattern
    {
        get => (int)Mathf.Repeat(enemyPatternCount, stageData.stageStep);
        set { 
            enemyPatternCount = value;
            bBossOn = false;
            if (enemyPatternCount >= stageData.stageStep && GameManager.instance is RankingGameManager == false)
                GameManager.instance.isWin = true;
            //RemoveAllEnemies();
            StartCoroutine(WaitKillCount());
            if (Mathf.Log(GameManager.instance.enemyTimes, 2) == (enemyPatternCount / stageData.stageStep) - 1)
                GameManager.instance.enemyTimes *= 2;
        }
    }
    
    [Header("PoolManager")]
    public PoolManager enemyPoolManager;
    public PoolManager enemyProjectilePoolManager;
    private List<int> rangeIndex;
    private List<int> bossIndex;
    private bool bBossOn = false;
    
    void Awake()
    {
        instance = this;

        SetSpawnPositions();
        
        // isRangedSpawnPositionUsing 초기화
        isRangedSpawnPositionUsing = new List<bool>();
        for (int i = 0; i < rangedSpawnPosition.Count; i++)
            isRangedSpawnPositionUsing.Add(false);
        
        enemyPoolManager = GameObject.Find("EnemyPoolManager").GetComponent<PoolManager>();

        spawnCount = 20;
    }

    void Start()
    {
        stageData = StageManager.instance.stageData;
        
        rangeIndex = stageData.enemies
            .Select((value, index) => new { value, index }) // 값과 인덱스를 함께 선택
            .Where(x => 100 < x.value && x.value < 200) // 값이 100을 초과하는 것만 필터링
            .Select(x => x.index)                       // 인덱스만 추출
            .ToList();
        
        bossIndex = stageData.enemies
            .Select((value, index) => new { value, index }) // 값과 인덱스를 함께 선택
            .Where(x => 200 < x.value)                  // 값이 200을 초과하는 것만 필터링
            .Select(x => x.index)                       // 인덱스만 추출
            .ToList();
        
        StartCoroutine(Spawn());
        EnemyPattern = 0;
    }

    void SetSpawnPositions()
    {
        float spawnXPos = -spawnRect.rect.width / 2;
        float spawnPosSpace = spawnRect.rect.width / (spawnPositionCount - 1);

        for (int i = 0; i < spawnPositionCount; i++)
        {
            //스폰 위치 지정용 오브젝트 생성
            GameObject tempObj = new GameObject("Spawn Position(" + (i + 1) + ")");
            tempObj.transform.SetParent(spawnRect);
            tempObj.AddComponent<RectTransform>();
            RectTransform tempRect = tempObj.GetComponent<RectTransform>();

            tempRect.anchoredPosition = new Vector3(spawnXPos + (spawnPosSpace * i), spawnRect.rect.y, 0f);
            spawnPosition.Add(tempRect);
        }
    }

    IEnumerator Spawn()
    {
        List<float> enemyList = new List<float>();
        float spawnDurationUnit = 0f;
        WaitForSeconds WFS = new WaitForSeconds(10f);
        
        yield return new WaitForSeconds(3.0f);
        while (true)
        {
            enemyList.Clear();
            
            enemyList = stageData.enemyFrequencies[EnemyPattern].ToList(); // List 깊은 복사
            
            // 보스 여부 확인 및 생성
            if (bossIndex.Count != 0)
                foreach (var t in bossIndex)
                    if (bBossOn == false && enemyList[t] != 0)
                    {
                        string name = "e" + stageData.enemies[t].ToString("D3");
                        enemyPoolManager.GetFromPool<EnemyBase>(name);
                        bBossOn = true;
                    }
            
            // 보스을 제외
            foreach (var t in bossIndex)
                enemyList[t] = 0;

            spawnCountGoal = spawnCount * stageData.frequencies[EnemyPattern];
            
            if (Math.Abs(spawnDurationUnit - spawnDuration / (int)(spawnCount * stageData.frequencies[EnemyPattern])) > 0.001f) {
                spawnDurationUnit = spawnDuration / (int)(spawnCount * stageData.frequencies[EnemyPattern]);
                WFS = new WaitForSeconds(spawnDurationUnit);
            }
            
            List<int> temp = CalculateProbability(enemyList, (int)(spawnCount * stageData.frequencies[EnemyPattern]));

            for (int i = 0; i < temp.Count; i++) {
                for (int j = 0; j < temp[i]; j++) {
                    yield return WFS;
                    SpawnEnemy(stageData.enemies[i]);
                }
            }
        }
    }

    private void SpawnEnemy(int id)
    {
        string name = "e" + id.ToString("D3");

        if (id < 100) // 근거리
        {
            enemyPoolManager.GetFromPool<MeleeEnemyController>(name);
        }
        else // 원거리
        {
            for (int i = 0; i < isRangedSpawnPositionUsing.Count; i++)
            {
                if (isRangedSpawnPositionUsing[i] == true) continue;
                
                var enemy = enemyPoolManager.GetFromPool<RangedEnemyController>(name);
                enemy.transform.position = new Vector3(rangedSpawnPosition[i].position.x, rangedSpawnPosition[i].position.y, 0f) + Vector3.up;
                enemy.positionNum = i;
                isRangedSpawnPositionUsing[i] = true;
                break;
            }
        }
    }
    
    // public void SpawnEnemy(int _enemyId, int _amount)
    // {
    //     string name = "e" + _enemyId.ToString("D3");
    //     for (int j = 0; j < _amount; j++)
    //         poolManager.GetFromPool<EnemyBase>(name);
    // }

    // void RemoveAllEnemies()
    // {
    //     var enemies = enemyContainer.transform.GetComponentsInChildren<EnemyBase>();
    //     foreach (var enemy in enemies)
    //         enemy.gameObject.SetActive(false);
    //         //poolManager.TakeToPool<EnemyBase>(idName, enemy);
    // }

    private IEnumerator WaitKillCount()
    {
        foreach (var t in bossIndex)
        {
            if (stageData.enemyFrequencies[EnemyPattern][t] != 0)
                yield break;
        }

        killCountGoal = stageData.stageKillCount[EnemyPattern];
        yield return new WaitUntil(() => stageData.stageKillCount[EnemyPattern] <= killCount);
        killCount = 0;
        EnemyPattern = enemyPatternCount + 1;
    }

    public void PlusKillCount()
    {
        killCount++;
        totalKillCount++;
    }

    public void NextEnemyPattern() => EnemyPattern = enemyPatternCount + 1;

    //해당 오브젝트를 비활성화
    public void TakeEnemyToPool(string idName, EnemyBase clone)
    {
        enemyPoolManager.TakeToPool<EnemyBase>(idName, clone);
    }


    #region EnemyProjectile

    public EnemyProjectile GetEnemyProjectileFromPool(string idName, Vector3 position, Quaternion rotation)
    {
        return enemyProjectilePoolManager.GetFromPool<EnemyProjectile>(idName, position, rotation);
    }
    
    public void TakeEnemyProjectileToPool(string idName, EnemyProjectile clone)
    {
        enemyProjectilePoolManager.TakeToPool<EnemyProjectile>(idName, clone);
    }

    #endregion
    

    private List<int> CalculateProbability(List<float> inputList, int value)
    {
        List<int> resultList = new List<int>();
        
        float total = inputList.Sum();

        if (total == 0)
            for (int i = 0; i < inputList.Count; i++)
                resultList.Add(0);
        else
        {
            for (int i = 0; i < inputList.Count; i++)
                resultList.Add(0);

            int rangeEnemyAmount = isRangedSpawnPositionUsing.Count(t => t == false); // Spawn가능한 원거리적 자릿수

            for (int i = 0; i < value; i++)
            {
                float randomValue = Random.Range(0f, total);

                float cumulativeProbability = 0f; // 누적확률
                for (int j = 0; j < inputList.Count; j++)
                {
                    cumulativeProbability += inputList[j];
                    if (randomValue <= cumulativeProbability)
                    {
                        //원거리 적일 경우
                        if (rangeIndex.Any(t => t == j))
                        {
                            if (rangeEnemyAmount <= 0)
                            {
                                i--;
                                break;
                            }

                            rangeEnemyAmount--;
                        }
                        
                        resultList[j]++;
                        break;
                    }
                }
            }
        }
        return resultList;
    }
}
