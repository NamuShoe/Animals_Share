using System;
using System.Collections.Generic;
using System.ComponentModel;

public enum EnmeyType
{
    [Description("근거리")]
    Melee,     // 근거리 공격
    [Description("원거리")]
    Ranged,    // 원거리 공격
    [Description("대장")]
    Boss       // 보스 몬스터
}
[Serializable]
public class EnemyData
{
    public EnmeyType enemyType;
    public int id; // 아이디
    public string name; // 적 이름
    public float attackPower;
    public float hp; // 체력
    public float moveSpeed; // 이동 속도
    public float size; // 크기
    public string description;
    public List<float> stageScale; // 스테이지 배율
    public List<float> itemDropRate; // 아이템 드롭률
}

