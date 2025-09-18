using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Redcode.Pools;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    public static WeaponController instance;
    
    /**********************************************************************/
    public Text shower; // 발매 시 지우기
    /**********************************************************************/

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileContainer;
    [SerializeField] private Animator playerAnimator;

    [SerializeField] private float fireAngle = 60f;
    [SerializeField] private float projectileInterval = 0.2f;
    public int FireCount = 0;
    
    [Space(20f)]
    [SerializeField] private float attackPower;
    [SerializeField] private float attackPowerPercent = 1f;
    public float bossAttackPowerPercent = 1f;
    
    [SerializeField] private float criticalPercent = 0.05f;
    [SerializeField] private float criticalMultipleMin = 1.7f; //임시
    [SerializeField] private float criticalMultipleMax = 2.0f; //임시
    [SerializeField] private float attackSpeed;
    [SerializeField] private float attackSpeedPercent = 1f;
    [SerializeField] private float AttackSpeed => 1 / (attackSpeed * attackSpeedPercent);
    [SerializeField] private float projectileSpeed = 1.5f;
    [SerializeField] private float projectileSpeedPercent = 1.0f;
    public float ProjectileSpeed => projectileSpeed * projectileSpeedPercent;

    public float AttackPower
    {
        get
        {
            if (UnityEngine.Random.Range(0f, 1f) < criticalPercent)
            {
                float criticalMultiple = (float)Math.Round(UnityEngine.Random.Range(criticalMultipleMin, criticalMultipleMax), 2);
                return attackPower * attackPowerPercent * criticalMultiple;
            }
            return attackPower * attackPowerPercent;
        }
    }
    public float NormalAttackPower => attackPower * attackPowerPercent;

    private IEnumerator fireEnumerator;

    [Header("Skill Point")]
    [SerializeField] private int multiShotCount = 0;
    [SerializeField] private int doubleShotCount = 0;
    [SerializeField] private int tripleBustCount = 0;
    [SerializeField] private int increaseStrengthCount = 0;
    [SerializeField] private int piercingCount = 0;
    public int PiercingCount => piercingCount;
    public bool isEpic = false;
    [SerializeField] private int senseOfSmellCount = 0;
    //속성
    [SerializeField] private bool isFire = false;
    public bool IsFire => isFire;
    public float firePercent = 1.0f;
    [SerializeField] private bool isWater = false;
    public bool IsWater => isWater;
    public float waterPercent = 0.05f;
    [SerializeField] private bool isLightning = false;
    public bool IsLightning => isLightning;
    public float lightningPercent = 0.125f;
    [SerializeField] private bool isPoison = false;
    public bool IsPoison => isPoison;
    public float poisonPercent = 0.8f;

    private Pool<Projectile> projectilePool;
    private int weaponSFXnum = 0;

    private void Awake()
    {
        instance = this;
        WeaponSetting();
        playerAnimator = PlayerController.instance.GetComponent<Animator>();
        
        /**********************************************************************/
        //shower = GameObject.Find("WeaponStat").GetComponent<Text>(); // 차후 삭제
        /**********************************************************************/

        DOVirtual.DelayedCall(1f, () => enabled = true);
        enabled = false;
    }

    private void WeaponSetting()
    {
        var playerStat = DataManager.instance.playerStat;
        var user_Weapon = DataManager.instance.userData.equipmentSpecificList[DataManager.instance.userData.CurrentWeaponId].id;
        
        Weapon weapon = ItemManager.instance.weapons.Find(w => w.id == user_Weapon);
        weaponSFXnum = GetWeaponSFX(weapon.weaponCode);

        attackPower = playerStat.AttackPower;
        attackSpeed = 1.0f + (playerStat.AttackSpeed / 10f);
        criticalPercent += (playerStat.CriticalPercent / 100);
        criticalMultipleMin += (playerStat.equipmentCriticalMultipleMin / 100f);
        criticalMultipleMax += (playerStat.equipmentCriticalMultipleMax / 100f);
        piercingCount += (int)playerStat.secret_equipmentPiercing;
        projectileSpeedPercent += playerStat.secret_equipmentProjectileSpeed;

        projectilePrefab = Resources.Load<GameObject>("Prefabs/InGame/Projectile/" + "p" + weapon.weaponCode.ToString("D3"));
        projectileContainer = GameObject.Find("ProjectileContainer").transform;

        projectilePool = Pool.Create<Projectile>(projectilePrefab.GetComponent<Projectile>(), 5, projectileContainer).NonLazy();
        SkillSetting();
    }

    private void Start()
    {
        fireEnumerator = Fire();
        StartCoroutine(fireEnumerator);
    }
    
    /**********************************************************************/
    // private void FixedUpdate()
    // {
    //     shower.text = "공격력 : " + attackPower + " * " + attackPowerPercent + " = " + attackPower * attackPowerPercent + "\n" +
    //                   "공격속도 : " + attackSpeed + " * " + attackSpeedPercent + " = " + 1 / AttackSpeed + "\n" +
    //                   "크리티컬 확률 : " + (criticalPercent * 100) + "%\n" +
    //                   "크리티컬 최소 배수 : " + criticalMultipleMin + "\n" +
    //                   "크리티컬 최대 배수 : " + criticalMultipleMax + "\n" +
    //                   "투사체 속도 : " + projectileSpeed + " * " + projectileSpeedPercent + " = " + ProjectileSpeed + "\n" +
    //                   "관통력 : " + piercingCount + "\n" +
    //                   "더블샷 : " + doubleShotCount + "\n" +
    //                   "트리플 버스트 : " + tripleBustCount + "\n" +
    //                   "불 속성 : " + isFire + "\n" +
    //                   "물 속성 : " + isWater + "\n" +
    //                   "번개 속성 : " + isLightning + "\n" +
    //                   "독 속성 : " + isPoison + "\n";
    // }
    /**********************************************************************/

    private void OnDestroy()
    {
        StopCoroutine(fireEnumerator);
    }

    private int GetWeaponSFX(int num)
    {
        return num switch {
            1 => (int)SoundManager.GAME_SFX.Bow, // 활
            2 => (int)SoundManager.GAME_SFX.Dart, // 다트
            3 => (int)SoundManager.GAME_SFX.Boomerang, // 부메랑
            _ => -1
        };
    }

    public void TakeProjectile(Projectile _projectile)
    {
        projectilePool.Take(_projectile);
    }

    private IEnumerator Fire()
    {
        float currentAttackSpeed = AttackSpeed;
        var attackSpeedWaitSec = new WaitForSeconds(AttackSpeed);
        while (true)
        {
            playerAnimator.SetFloat("AttackSpeed", 1 / AttackSpeed); // 배속
            playerAnimator.Play("Attack", -1, 0);
            if (Math.Abs(currentAttackSpeed - AttackSpeed) > 0.001f) // currentAttackSpeed != AttackSpeed
            {
                currentAttackSpeed = AttackSpeed;
                attackSpeedWaitSec = new WaitForSeconds(currentAttackSpeed); 
            }
            yield return attackSpeedWaitSec;; // 임시
            for (int i = 0; i < doubleShotCount + 1; i++)
                Invoke(nameof(Shot), i * 0.2f);
            FireCount++;
        }
    }

    public void ReloadFire()
    {
        StopCoroutine(fireEnumerator);
        StartCoroutine(fireEnumerator);
    }

    private void Shot()
    {
        var position = transform.position;
        var rotation = transform.rotation;
        float angle = fireAngle/((tripleBustCount * 2));
        float pos = multiShotCount * projectileInterval / 2;

        for (int i = 0; i < (tripleBustCount * 2) + 1; i++)
        {
            if (((tripleBustCount * 2) + 1) / 2 == i)
                continue;
            Quaternion spawnRotation = Quaternion.Euler(rotation.x, rotation.y, angle * i - (fireAngle / 2));
            
            Vector3 spawnPosition = new Vector3(position.x, position.y, position.z);
            projectilePool.Get(spawnPosition, spawnRotation);
        }

        for (int i = 0; i < multiShotCount + 1; i++)
        {
            float interval = (i * projectileInterval) - pos;
            Vector3 spawnPosition = new Vector3(position.x + interval, position.y - Mathf.Abs(interval), position.z);
            projectilePool.Get(spawnPosition, rotation);
        }
        
        SoundManager.instance.PlayOneShot(((SoundManager.GAME_SFX)weaponSFXnum));
    }

    private void SkillSetting()
    {
        SkillManager skillManager = SkillManager.instance;

        skillManager.AddSkill("common_MultiShot", PlusMultiShotCount, 2);
        skillManager.AddSkill("common_DoubleShot", PlusDoubleShotCount, 2);
        skillManager.AddSkill("common_TripleBust", PlusTripleBustCount, 1);
        
        skillManager.AddSkill("common_IncreaseAttackPower", IncreaseAttackPower, 3);
        skillManager.AddSkill("common_IncreaseAttackSpeed", () => IncreaseAttackSpeed(5), 2);
        skillManager.AddSkill("common_PlusPiercingCount", PlusPiercingCount, 2);
        skillManager.AddSkill("common_IncreaseCritical", () => IncreaseCritical(5, 5, 0), 2);
        //skillDataManager.AddSkillData("common_PlusSenseOfSmellCount", PlusSenseOfSmellCount, 2);
        
        skillManager.AddSkill("common_EnchantFire", EnchantFire, 1, new List<string> { "common_EnchantWater", "common_EnchantLightning", "common_EnchantPoison" });
        skillManager.AddSkill("common_EnchantWater", EnchantWater, 1, new List<string> { "common_EnchantFire", "common_EnchantLightning", "common_EnchantPoison" });
        skillManager.AddSkill("common_EnchantLightning", EnchantLightning, 1, new List<string> { "common_EnchantFire", "common_EnchantWater", "common_EnchantPoison" });
        skillManager.AddSkill("common_EnchantPoison", EnchantPoison, 1, new List<string> { "common_EnchantFire", "common_EnchantWater", "common_EnchantLightning" });
        
        skillManager.AddSkill("common_EnergyDrink", EnergyDrink, 1);
        skillManager.AddSkill("common_HeavyProjectile", HeavyProjectile, 2, new List<string> { "common_LightProjectile"});
        skillManager.AddSkill("common_LightProjectile", LightProjectile, 2, new List<string> { "common_HeavyProjectile"});
    }

    #region Skill Function
    
    public void PlusMultiShotCount() { multiShotCount++; attackPowerPercent -= 0.05f; projectilePool.SetCount(projectilePool.Count * 2).NonLazy(); }
    public void PlusDoubleShotCount() { doubleShotCount++; attackPowerPercent -= 0.05f; projectilePool.SetCount(projectilePool.Count * 2).NonLazy(); }
    public void PlusTripleBustCount() { tripleBustCount++; projectilePool.SetCount(projectilePool.Count * 3).NonLazy(); }

    public void IncreaseAttackPower()
    {
        increaseStrengthCount++; 
        float num = increaseStrengthCount switch
        {
            1 => 0.2f,
            2 => 0.15f,
            3 => 0.15f,
            _ => 0
        };
        attackPowerPercent += num;
    }
    public void IncreaseAttackSpeed(float percent) { attackSpeedPercent += percent / 100f; }
    public void PlusPiercingCount() { piercingCount++; }
    public void IncreaseCritical(float percent = 0f, float multipleMin = 0f, float multipleMax = 0f)
    {
        criticalPercent += percent / 100f; 
        criticalMultipleMin += multipleMin / 100f;
        criticalMultipleMax += multipleMax / 100f;
    }
    public void IncreaseCriticalPercent(float percent) { criticalPercent += percent / 100f; }
    public void IncreaseCriticalMultipleMin(float percent) { criticalMultipleMin += percent / 100f; }
    public void IncreaseCriticalMultipleMax(float percent) { criticalMultipleMax += percent / 100f; }
    public void PlusSenseOfSmellCount() { senseOfSmellCount++; } //차후 구현
    
    public void EnchantFire() { isFire = true; }
    public void EnchantWater() { isWater = true; }
    public void EnchantLightning() { isLightning = true; }
    public void EnchantPoison() { isPoison = true; }

    public void EnergyDrink()
    {
        attackPowerPercent += 0.2f; 
        attackSpeedPercent += 0.1f;
        PlayerController.instance.DamagedHp += 1f;
    }
    public void HeavyProjectile()
    {
        attackPowerPercent += 0.25f; 
        attackSpeedPercent -= 0.15f;
        projectileSpeedPercent -= 0.1f;
    }

    public void LightProjectile()
    {
        attackPowerPercent -= 0.25f; 
        attackSpeedPercent += 0.2f;
        projectileSpeedPercent += 0.1f;
    }
    
    #endregion
    
    
    #region Character Skill Function
    
    //public void IncreaseAttackPowerPoint(float point) { attackPower += point; }
    public void IncreaseAttackPower(float percent) { attackPowerPercent += percent / 100f; }
    public void IncreaseProjectileSpeed(float percent) { projectileSpeedPercent += percent / 100f; }
    
    #endregion
    
    #region Item Epic Effect Function
    
    public void IncreaseBossAttackPower(float percent) { bossAttackPowerPercent += percent / 100f; }
    public void IncreaseFirePercent(float percent) { firePercent += percent / 100f; }
    public void IncreaseWaterPercent(float percent) { waterPercent += percent / 100f; }
    public void IncreaseLightningPercent(float percent) { lightningPercent += percent / 100f; }
    public void IncreasePoisonPercent(float percent) { poisonPercent += percent / 100f; }
    public void SetEpic(float value) { isEpic = value > 0 ? true : false; }
    
    #endregion
}
