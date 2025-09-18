using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.U2D.Animation;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteLibrary), 
    typeof(Rigidbody2D), 
    typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    protected WeaponController weaponController;

    /**********************************************************************/
    public Text shower; //발매 시 지우기
    /**********************************************************************/

    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private FloatingJoystick joystick;
    [SerializeField] private Animator animator;
    private Material material;
    [SerializeField] private SpriteRenderer[] weaponSpriteRenderers = new SpriteRenderer[2];
    private Slider HPSlider;
    private Slider EXPSlider;

    [Header("Status")]
    private int level = 1;
    public int maxLevel = 11;
    private float hp = 0f;
    [SerializeField] private float currentHp = 0f;
    private float CurrentHp
    {
        get => currentHp;
        set
        {
            currentHp = value;
            //HPSlider.DOValue(value, 0.2f);
            HPSlider.value = currentHp;
        }
    }
    [SerializeField] private float hpPercent = 1f;
    private float HpPercent
    {
        get => hpPercent;
        set
        {
            float temp = MaxHp;
            hpPercent = value;
            CurrentHp += MaxHp - temp;
            HPSlider.maxValue = MaxHp;
        }
    }
    [SerializeField] private float damagedHp = 0f;
    public float DamagedHp
    {
        get => damagedHp;
        set
        {
            damagedHp = value;
            if (MaxHp <= CurrentHp) CurrentHp = MaxHp;
            HPSlider.maxValue = MaxHp;
        }
    }
    private float MaxHp => hp * hpPercent - damagedHp;
    [SerializeField] public float moveSpeed = 5.0f; // 추후 함수로 수정
    [SerializeField] public float moveSpeedPercent = 1f;
    private float currentMoveSpeed => moveSpeed * moveSpeedPercent;
    [SerializeField] private float avoidPercent = 0f;
    [SerializeField] private float avoidHealPercent = 0f;
    [SerializeField] private bool isInvulnerability = false;
    [SerializeField] private float exp = 0f;
    private float expPercent = 1f;
    [SerializeField] private float drainPercent = 0f;

    [Header("Skill Point")]
    [SerializeField]
    private int bugSprayCount = 0;
    [SerializeField] private bool isArmoring = false;
    [SerializeField] private bool isSecondChance = false;
    
    private Camera _camera;
    private Tween _shakeTween;

    public virtual void AddEXP(int _exp)
    {
        if (level >= maxLevel) {
            EXPSlider.value = 1f;
            return;
        }
        
        exp += _exp * expPercent;
        if(exp >= 100) {
            GameManager.instance.LevelUp();
            level++;
            exp -= 100;
        }
        EXPSlider.value = exp / 100;
    }
    
    protected virtual void Awake()
    {
        _camera = Camera.main;
        instance = this;
        var tempMaterial = GetComponentInChildren<SpriteRenderer>().material;
        material = new Material(tempMaterial);

        foreach (var spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.material = material;
        }
        
        HPSlider = GameObject.Find("HPSlider").GetComponent<Slider>();
        EXPSlider = GameObject.Find("EXPSlider").GetComponent<Slider>();
        EXPSlider.value = exp;
        
        /**********************************************************************/
        //shower = GameObject.Find("PlayerStat").GetComponent<Text>(); // 차후 삭제
        /**********************************************************************/
        
        Instantiate(Resources.Load<GameObject>("Prefabs/InGame/" + "w001"), transform).name = "Weapon"; //임시적으로 w001
        
        PlayerSetting();
        SkillSetting();
        DOVirtual.DelayedCall(1f, () => enabled = true);
        enabled = false;
    }

    private void PlayerSetting()
    {
        var dataManager = DataManager.instance;
        var playerStat = dataManager.playerStat;
        
        hp = playerStat.Hp;
        CurrentHp = MaxHp;
        HPSlider.maxValue = MaxHp;
        HPSlider.value = CurrentHp;
        moveSpeed = 4f + playerStat.MoveSpeed; //stat이 1일 경우, 실제 값은 5배로 한다.
        avoidPercent = playerStat.equipmentAvoidPercent / 100f;
        
        var user_Weapon = dataManager.userData.equipmentSpecificList[dataManager.userData.CurrentWeaponId];
        Weapon weapon = ItemManager.instance.weapons.Find(w => w.id == user_Weapon.id);
        
        animator.runtimeAnimatorController =
            Resources.Load<RuntimeAnimatorController>("Character/Animation/" + "w" + weapon.weaponCode.ToString("D3"));
        animator.speed = 0f;
        
        string path = "Character/Projectile/" + "pp" + weapon.weaponCode.ToString("D3");
        if (user_Weapon.grade == 3)
            path += "_Epic";
            
        //Weapon_R
        weaponSpriteRenderers[0].sprite = Resources.Load<Sprite>(path);
        
        //Weapon_L
        Sprite weaponSprite = Resources.Load<Sprite>(path + "-1");
        if (weaponSprite != null) weaponSpriteRenderers[1].sprite = weaponSprite;
        else weaponSpriteRenderers[1].gameObject.SetActive(false);
        
        //material
        material.SetFloat("_FlickerFreq", 0f);
    }

    protected virtual void Start()
    {
        Reset();
        weaponController = WeaponController.instance;
        animator.speed = 1.0f;
        CharacterSkillSetting();
    }

    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        joystick = GameObject.Find("Floating Joystick").GetComponent<FloatingJoystick>();
        animator = gameObject.GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        Move();

        /**********************************************************************/
        // shower.text = "체력 : " + DataManager.instance.playerStat.Hp + " * " + hpPercent + " - " + damagedHp + " = " +
        //               MaxHp + "\n" +
        //               "현재 체력 : " + CurrentHp + "\n" +
        //               "이동속도 : " + moveSpeed + " * " + moveSpeedPercent + " = " + currentMoveSpeed + "\n" +
        //               "회피율 : " + (avoidPercent * 100) + "%\n" +
        //               "경험치 : " + exp + "\n" +
        //               "경험치% : " + (expPercent * 100) + "%\n" + 
        //               "퇴치제 : " + bugSprayCount;
        /**********************************************************************/
    }

    private void Move()
    {
        Vector2 moveVector = new Vector2(joystick.Horizontal, joystick.Vertical) * (currentMoveSpeed * Time.fixedDeltaTime); //moveSpeed 7% 합연산
        if (moveVector == Vector2.zero) return;
        
        Vector3 viewPosition = _camera.WorldToViewportPoint(transform.position);
        
        //화면 밖으로 나갈 경우 해당 방향의 Vector를 0으로 한다
        if ((viewPosition.x <= 0 && moveVector.x < 0) || (1 <= viewPosition.x && 0 < moveVector.x))
        {
            viewPosition.x = Mathf.Clamp(viewPosition.x, 0f, 1f);
            moveVector.x = 0;
        }
        
        if ((viewPosition.y <= 0 && moveVector.y < 0) || (1 <= viewPosition.y && 0 < moveVector.y))
        {
            viewPosition.y = Mathf.Clamp(viewPosition.y, 0f, 1f);
            moveVector.y = 0;
        }
        
        Vector2 pos = _camera.ViewportToWorldPoint(viewPosition);
        rigidbody.MovePosition(pos + moveVector);
        
        // Vector2 moveVector = new Vector2(joystick.Horizontal, joystick.Vertical) * Time.fixedDeltaTime;
        // if (moveVector == Vector2.zero) rigidbody.velocity = Vector2.zero;
        //
        // rigidbody.velocity = moveVector * (currentMoveSpeed * 15f);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Enemy")) return;
        
        if (other.gameObject.GetComponent<EnemyBase>().isDead == true) return;
        
        var collision = TakeDamage(other.GetComponent<EnemyBase>().CurrentAttackPower);
        if (collision && other.GetComponent<EnemyController>() != null)
        {
            other.GetComponent<EnemyController>().gameObject.SetActive(false);
            SoundManager.instance.PlayOneShot(SoundManager.GAME_SFX.Collision);
        }
    }
    
    /// <param name="damageAmount">If collide, take damage parameter</param>
    /// <returns>If collide return true</returns>
    public bool TakeDamage(float damageAmount)
    {
        if (isInvulnerability) // 무적상태
            return false;
        
        if (Random.value < avoidPercent) // 회피
        {
            Avoid();
            SoundManager.instance.PlayOneShot(SoundManager.GAME_SFX.Avoid);
            return false;
        }
        
        if (isArmoring) // 갑옷
        {
            isArmoring = false;
            return true;
        }
        
        Damaged(damageAmount);
        return true;
    }

    protected virtual void Damaged(float damageAmount = 1f)
    {
        CurrentHp -= damageAmount;
        CameraShake();
        InvulnerabilityPeriod(2.0f);
        if (CurrentHp <= 0)
        {
            if (isSecondChance)
            {
                isSecondChance = false;
                CurrentHp = 0.5f;
                return;
            }
            GameManager.instance.isWin = false;
        }
    }

    protected virtual void Avoid()
    {
        AvoidHealHp();
    }

    private void CameraShake()
    {
        _shakeTween.Complete();
        _shakeTween = _camera.DOShakePosition(0.5f, Vector3.one / 4, 25);
    }

    private void InvulnerabilityPeriod(float time)
    {
        isInvulnerability = true;
        //material.EnableKeyword("FLICKER_ON");
        material.SetFloat("_FlickerFreq", 1f);
        DOVirtual.DelayedCall(time, () =>
        {
            isInvulnerability = false;
            //material.DisableKeyword("FLICKER_ON");
            material.SetFloat("_FlickerFreq", 0f);
        }, 
            false);
    }

    private void CharacterSkillSetting()
    {
        var characterId = DataManager.instance.userData.CurrentCharacterId;
        var characterSpecific = DataManager.instance.userData.characterInforms.Find(x => x.id == characterId);
        var characterSkillList = characterSpecific.characterSkillList;
        
        Action[] characterSkills = new Action[9];
        characterSkills[0] += BasicSkill;
        characterSkills[1] += A1Skill;
        characterSkills[2] += B1Skill;
        characterSkills[3] += A2Skill;
        characterSkills[4] += B2_1Skill;
        characterSkills[5] += B2_2Skill;
        characterSkills[6] += A3Skill;
        characterSkills[7] += B3Skill;
        characterSkills[8] += LastSkill;
        
        for (int i = 0; i < characterSkillList.Count; i++) {
            for (int j = 0; j < characterSkillList[i]; j++) {
                characterSkills[i]();
            }
        }
    }
    
    protected virtual void BasicSkill() { Debug.LogError("BasicSkill"); }
    protected virtual void A1Skill() { Debug.LogError("A1Skill"); }
    protected virtual void B1Skill() { Debug.LogError("B1Skill"); }
    protected virtual void A2Skill() { Debug.LogError("A2Skill"); }
    protected virtual void B2_1Skill() { Debug.LogError("B2_1Skill"); }
    protected virtual void B2_2Skill() { Debug.LogError("B2_2Skill"); }
    protected virtual void A3Skill() { Debug.LogError("A3Skill"); }
    protected virtual void B3Skill() { Debug.LogError("B3Skill"); }
    protected virtual void LastSkill() { Debug.LogError("LastSkill"); }

    protected virtual void SkillSetting()
    {
        SkillManager skillManager = SkillManager.instance;
        
        skillManager.AddSkill("common_IncreaseHp", () => IncreaseHp(20), 100000);
        skillManager.AddSkill("common_IncreaseAvoid", () => IncreaseAvoid(5), 2);
        skillManager.AddSkill("common_BugSpray", BugSpray, 2);
        skillManager.AddSkill("common_AdditionalArmor", AdditionalArmor, 1);
        skillManager.AddSkill("common_SecondChance", SecondChance, 1);
    }
    
    #region Skill Function
    
    public void IncreaseHp(float percent) { HpPercent += percent / 100f; }
    public void IncreaseAvoid(float percent) { avoidPercent += percent / 100f; }
    public void BugSpray() { bugSprayCount++; if(bugSprayCount == 1) StartCoroutine(BugSpraying()); }

    private IEnumerator BugSpraying()
    {
        var bugSprayWaitSec = new WaitForSeconds(0.25f);
        while (true)
        {
            EnemyBase[] enemies = GameObject.FindObjectsOfType<EnemyBase>();
        
            List<EnemyBase> nearbyEnemies = enemies
                .Where(enemy => Vector3.Distance(transform.position, enemy.transform.position) <= bugSprayCount + 1)
                .ToList();

            foreach (var enemy in nearbyEnemies)
            {
                enemy.TakeDamage(weaponController.NormalAttackPower * 0.1f);
            }

            yield return bugSprayWaitSec;
        }
    }
    
    public void AdditionalArmor() { isArmoring = true; StartCoroutine(Armoring()); }

    private IEnumerator Armoring()
    {
        var armorWaitWhile = new WaitWhile(() => isArmoring);
        var armorWaitSec = new WaitForSeconds(30.0f);
        while (true)
        {
            yield return armorWaitWhile;
            yield return armorWaitSec;
            isArmoring = true;
        }
    }
    
    public void SecondChance() { isSecondChance = true; }
    
    #endregion

    #region Character Skill Function
    
    public void IncreaseMoveSpeed(float percent) { moveSpeedPercent += percent / 100f; }
    public void HealHp(float percent) { CurrentHp += MaxHp * (percent / 100f); CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp); }
    public void ExpGained(float percent) { expPercent += percent / 100f; }
    public void IncreaseDrainPercent(float percent) { drainPercent += percent / 100f; }
    public void DrainHp() { CurrentHp += MaxHp * (drainPercent / 100f); CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp); }
    public void IncreaseAvoidHealPercent(float percent) { avoidHealPercent += percent / 100f; }
    public void AvoidHealHp() { CurrentHp += MaxHp * (avoidHealPercent / 100f); CurrentHp = Mathf.Clamp(CurrentHp, 0, MaxHp); }
    public void AddProbabilityIncreaseAttackSpeed(float amount) { SkillManager.instance.AddProbability("민첩함 증가", 0.2f); }
    public void AddProbabilityIncreaseAttackPower(float amount) { SkillManager.instance.AddProbability("힘 증가", 0.2f); }
    
    #endregion
}
