using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SquirrelController : PlayerController
{
    [SerializeField] private GameObject grenadePrefab;
    private GameObject grenade;

    protected override void Awake()
    {
        base.Awake();
        grenade = Instantiate(grenadePrefab);
        grenade.SetActive(false);
    }

    private IEnumerator ThrowGrenade()
    {
        var fireCountWaitWhile = new WaitWhile(() => weaponController.FireCount % 5 == 0);
        var fireCountWaitUntil = new WaitUntil(() => weaponController.FireCount % 5 == 0);
        yield return new WaitWhile(() => weaponController.FireCount == 0);
        
        while (true)
        {
            yield return fireCountWaitWhile;
            yield return fireCountWaitUntil;
            grenade.transform.position = transform.position;
            grenade.SetActive(true);
        }
    }

    private void IncreaseGrenadeDrainAmount(float percent)
    {
        grenade.GetComponent<Grenade>().IncreaseDrainAmount(percent);
    }
    
    //도토리 투척
    protected override void BasicSkill()
    {
        grenade.GetComponent<Grenade>().SetDamageAndRange(weaponController.NormalAttackPower * 1.25f, 3f);
        StartCoroutine(ThrowGrenade());
    }
    protected override void A1Skill() { ExpGained(5); }
    protected override void B1Skill() { weaponController.IncreaseAttackSpeed(3); }
    protected override void A2Skill() { weaponController.IncreaseCriticalPercent(4); }
    protected override void B2_1Skill() { IncreaseMoveSpeed(3); }
    protected override void B2_2Skill() { IncreaseAvoid(2); }
    protected override void A3Skill() { IncreaseGrenadeDrainAmount(3); }
    protected override void B3Skill() { weaponController.IncreaseCriticalPercent(2); }
    //파편 도토리 투척
    protected override void LastSkill()
    {
        grenade.GetComponent<Grenade>().SetDamageAndRange(weaponController.NormalAttackPower * 1.5f, 6f);
    }
}
