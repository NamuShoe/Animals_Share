using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[System.Serializable]
public class Skill
{
    [SerializeField] public string skillName;
    [SerializeField] public UnityEvent skillEvent;
    [SerializeField] public int count;
    [SerializeField] public float probability = 1;
    [SerializeField] public List<string> relatedSkills;
    
    public Skill(string _skillName, UnityAction _skillAction, int _count, List<string> _relatedSkills = null)
    {
        skillName = _skillName;
        skillEvent = new UnityEvent();
        skillEvent.AddListener(_skillAction);
        count = _count;
        relatedSkills = _relatedSkills ?? new List<string>(); // null일 경우 빈 리스트로 초기화
    }

    public void InvokeSkillEvent()
    {
        skillEvent.Invoke(); 
        if(count != -1) 
            count--;
    }
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager instance;

    private List<Skill> skills = new List<Skill>();
    public List<Skill> Skills => skills;
    public List<SkillData> skillDatas = new List<SkillData>();

    void Awake()
    {
        instance = this;
        skills.Clear();
        skillDatas.Clear();
        SetSkillData();
    }

    private void SetSkillData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("GameData/SkillData");
        if (jsonFile != null)
        {
            skillDatas = JsonConvert.DeserializeObject<List<SkillData>>(jsonFile.text);
        }
    }
    
    public void AddSkill(string skillName, UnityAction action, int count, List<string> relatedSkills = null)
    {
        Skill skill = new Skill(skillName, action, count, relatedSkills);
        skills.Add(skill);
    }
    
    public Skill GetSkill(string skillName)
    {
        Skill skill = skills.Find(s => s.skillName == skillName);
        return skill;
    }

    public SkillData GetSkillData(string id)
    {
        SkillData skillData = skillDatas.Find(s => s.id == id);
        return skillData;
    }

    public void RemoveSkill(string skillName)
    {
        Skill skill = skills.Find(s => s.skillName == skillName);
        skills.Remove(skill);
    }

    public void AddProbability(string skillName, float amount)
    {
        Skill skill = skills.Find(x => x.skillName == skillName);
        skill.probability += amount;
    }

    public void ConfirmSkill(int num)
    {
        Skill skillToCheck = skills.Find(skill => skill.skillName == skills[num].skillName);
        if (skillToCheck != null)
        {
            RemoveRelatedSkill(skillToCheck.relatedSkills);
            if (skillToCheck.count == 0)
                skills.Remove(skillToCheck);
        }
    }

    public void ConfirmSkill(string skillName)
    {
        Skill skillToCheck = skills.Find(skill => skill.skillName == skillName);
        if (skillToCheck != null)
        {
            RemoveRelatedSkill(skillToCheck.relatedSkills);
            if (skillToCheck.count == 0)
                skills.Remove(skillToCheck);
        }
    }
    
    private void RemoveRelatedSkill(List<string> relatedSkillNames)
    {
        foreach (var relatedSkillName in relatedSkillNames)
        {
            if (!string.IsNullOrEmpty(relatedSkillName))
            {
                Skill relatedSkill = skills.Find(s => s.skillName == relatedSkillName);
                if (relatedSkill != null)
                {
                    skills.Remove(relatedSkill);
                }
            }
        }
    }
}
