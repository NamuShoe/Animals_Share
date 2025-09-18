using System;

[Serializable]
public class PlayerStats
{
    public float Hp => characterHp + equipmentHp; //차후 아이템 관련으로 구현
    public float MoveSpeed => characterMoveSpeed + equipmentMoveSpeed;
    public float AttackPower => characterAttackPower + equipmentAttackPower;
    public float AttackSpeed => characterAttackSpeed + equipmentAttackSpeed;
    public float CriticalPercent => equipmentCriticalPercent + secret_equipmentCriticalPercent;
     
    
    
    // 캐릭터
    public float characterHp = 0;
    public float characterMoveSpeed = 0;
    
    public float characterAttackPower = 0;
    public float characterAttackSpeed = 0;
    
    // 장비
    public float equipmentHp = 0;
    public float equipmentMoveSpeed = 0;
    public float equipmentAvoidPercent = 0;
    
    public float equipmentAttackPower = 0;
    public float equipmentAttackSpeed = 0;
    public float equipmentCriticalPercent = 0;
    public float equipmentCriticalMultipleMin = 0;
    public float equipmentCriticalMultipleMax = 0;
    
    public float secret_equipmentCriticalPercent = 0;
    public float secret_equipmentPiercing = 0;
    public float secret_equipmentProjectileSpeed = 0;

    public void Clear()
    {
        // 캐릭터
        characterHp = 0;
        characterMoveSpeed = 0;
        
        characterAttackPower = 0;
        characterAttackSpeed = 0;
        
        // 장비
        equipmentHp = 0;
        equipmentMoveSpeed = 0;
        equipmentAvoidPercent = 0;
        
        equipmentAttackPower = 0;
        equipmentAttackSpeed = 0;
        equipmentCriticalPercent = 0;
        equipmentCriticalMultipleMin = 0;
        equipmentCriticalMultipleMax = 0;

        secret_equipmentCriticalPercent = 0;
        secret_equipmentPiercing = 0;
        secret_equipmentProjectileSpeed = 0;
    }
}
