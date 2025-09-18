using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

public enum AnimalTypes
{
    [Description("강아지")]
    Dog,
    [Description("고양이")]
    Cat,
    [Description("햄스터")]
    Rodents
}

[Serializable]
public class CharacterData
{
    public AnimalTypes animalTypes; // 동물종
    public int id; // 아이디
    public float hp;
    public string name; // 캐릭터 이름
    public float attackPower; // 공격력
    public float attackSpeed; // 공격 속도
    public float moveSpeed; // 이동 속도
    public string description; // 캐릭터 설명
    public List<string> characterSkillNameList; // 전체 스킬 이름
    public List<string> characterSkillVariableList; // 전체 스킬 이름
}

/// <summary>
/// 애니멀 enum 이름 어트리뷰트 참조하여 한글 이름 출력
/// </summary>
public static class EnumNameExtensions {
    public static string GetNameKr(this Enum value)
    {
        FieldInfo field = value.GetType().GetField(value.ToString());
        DescriptionAttribute attribute =
            (DescriptionAttribute)Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute));
        return attribute == null ? value.ToString() : attribute.Description;
    }
}
