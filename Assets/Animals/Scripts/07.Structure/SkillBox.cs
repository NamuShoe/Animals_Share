using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkillBox : MonoBehaviour
{
    [SerializeField] private CharacterStatManager characterStatManager;

    [SerializeField] private bool isMainSkill = false;
    [SerializeField] private List<GameObject> parentSkills;
    [SerializeField] private List<GameObject> childSkills;
    [SerializeField] private List<Image> childLines;

    [SerializeField] private Button button;
    [SerializeField] private Transform iconImage;
    public Transform IconImage => iconImage;
    [HideInInspector] public string skillName;
    [HideInInspector] public string skillDescription;

    [SerializeField] private Text skillText;
    [SerializeField] private Image lockImage; // 스킬 잠금 이미지
    
    [Header("SkillInspector")]
    public bool isOnSkill = false;
    [SerializeField] private int skillCount = 0;
    public int SkillCount => skillCount;
    [SerializeField] private int maxSkillCount;
    [SerializeField] private int requestSkillPoint;

    private void Awake()
    {
        button.onClick.AddListener(() =>
        {
            PopupManager.instance.OpenTap((int)PopupType.CharacterSkillConfirm);
            characterStatManager.SetRequestSkillPoint(requestSkillPoint);
            characterStatManager.SetSkillDescription(iconImage.GetComponent<Image>().sprite, skillName, skillDescription);
            characterStatManager.ResetYesButton();
            characterStatManager.YesButton.onClick.AddListener(OnSkill);
            characterStatManager.IsMaxSkillCount(skillCount >= maxSkillCount);
            characterStatManager.IsEnoughSkillPoint(characterStatManager.skillPoint >= requestSkillPoint);
        });
    }

    private void OnEnable()
    {
        if (isMainSkill)
            skillText.text = "패시브";
        else
            skillText.text = skillCount + "/" + maxSkillCount;
    }

    private void OnDisable()
    {
        Init();
    }

    public void Init()
    {
        button.interactable = false;
        iconImage.localScale = Vector3.zero;
        lockImage.gameObject.SetActive(true); // 스킬 잠금 이미지 활성화
        skillCount = 0;
        
        foreach (var line in childLines) {
            line.fillAmount = 0f;
        }

        skillText.gameObject.SetActive(false);
        skillText.text = skillCount + "/" + maxSkillCount;
    }

    public void OnSkill()
    {
        button.interactable = false;
        if (CanSkillPoint() == false) 
            return; 
        button.interactable = true;
        isOnSkill = true;

        if (skillCount <= 0)
        {
            //줄 연결
            foreach (var line in childLines)
            {
                line.DOFillAmount(1f, 0.2f);
            }
            
            //child 스킬 아이콘 표시 및 button 활성화
            foreach (var skill in childSkills)
            {
                DOVirtual.DelayedCall(0.2f,
                    () => skill.GetComponent<SkillBox>().iconImage.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack));
                skill.GetComponent<Button>().interactable = skill.GetComponent<SkillBox>().CanSkillPoint();

                skill.GetComponent<SkillBox>().lockImage.gameObject.SetActive(false);
                skill.GetComponent<SkillBox>().skillText.gameObject.SetActive(true);
            }
            
            //해당 스킬 강조 애니메이션
            Vector3 temp = new Vector3(1.1f, 1.1f, 1f);
            iconImage.DOScale(temp, 0.1f).SetEase(Ease.OutCubic)
                .OnComplete(() => iconImage.DOScale(Vector3.one, 0.1f).SetEase(Ease.InCubic));
        }
        else
        {
            Vector3 temp = new Vector3(1.1f, 1.1f, 1f);
            iconImage.DOScale(temp, 0.1f).SetEase(Ease.OutCubic)
                .OnComplete(() => iconImage.DOScale(Vector3.one, 0.1f).SetEase(Ease.InCubic));
            
        }
        
        skillCount++;
        //if (skillCount >= maxSkillCount) button.interactable = false;
        
        characterStatManager.skillPoint -= requestSkillPoint;
        skillText.text = skillCount + "/" + maxSkillCount;
        characterStatManager.RefreshSkillPointText();
    }

    public bool CanSkillPoint()
    {
        foreach (var skill in parentSkills) //부모 스킬이 활성화 되어 있느냐
            if (skill.GetComponent<SkillBox>().isOnSkill == false)
                return false;
        
        return true;
    }
}
