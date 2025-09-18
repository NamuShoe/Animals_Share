using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public struct Speaker
{
    public Text textName;
    public Text textDialogue;
}

[System.Serializable]
public struct DialogData
{
    public string name;
    [TextArea(3, 5)]
    public string dialogue;
}

public class TutorialData : MonoBehaviour
{
    [SerializeField] private Speaker speaker;
    [SerializeField] private DialogData[] dialogs;
    [SerializeField] private RectTransform[] focusObjects;
    public bool isDialog1st = true;
    
    private bool doDialog = true;
    private int currentDialogIndex = -1;
    private bool isTyping = false;
    
    private bool doFocus = true;
    private int currentFocusObjectIndex = -1;
    private bool isHighlight = false;

    [SerializeField] private GameObject canvas;
    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    private EventSystem eventSystem;

    private void Start()
    {
        graphicRaycaster = canvas.GetComponent<GraphicRaycaster>();
        eventSystem = GetComponent<EventSystem>();
    }

    private void SetupDialog()
    {
        TutorialManager.instance.GetDialogue().SetActive(true);
    }

    private void UnSetupDialog()
    {
        TutorialManager.instance.GetDialogue().SetActive(false);
    }
    
    public bool UpdateDialog()
    {
        if (dialogs.Length == 0) return true;
        if (doDialog == true)
        {
            DOVirtual.DelayedCall(0.2f, SetupDialog);
            NextDialog();
            doDialog = false;
        }
        
        if (Input.GetMouseButtonDown(0) && isTyping == false)
        {
            if (dialogs.Length > currentDialogIndex + 1)
                NextDialog();
            else
            {
                UnSetupDialog();
                return true;
            }
        }

        return false;
    }

    public void NextDialog()
    {
        currentDialogIndex++;
        
        isTyping = true;
        speaker.textName.text = dialogs[currentDialogIndex].name;
        speaker.textDialogue.text = "";
        float duration = dialogs[currentDialogIndex].dialogue.Length * 0.05f;
        speaker.textDialogue.DOText(dialogs[currentDialogIndex].dialogue, duration)
            .OnComplete(() => isTyping = false);
    }

    public bool UpdateFocus()
    {
        if (focusObjects.Length == 0) return true;
        if (doFocus == true)
        {
            TutorialManager.instance.GetHighlight().SetActive(false);
            DOVirtual.DelayedCall(0.2f, NextFocus);
            doFocus = false;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            pointerEventData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            
            graphicRaycaster.Raycast(pointerEventData, results);

            isHighlight = false;
            foreach (var result in results)
            {
                if (isHighlight == false && result.gameObject.name == "Highlight")
                {
                    isHighlight = true;
                    continue;
                }
                
                var btn = result.gameObject.GetComponent<Button>();
                if (isHighlight == true && btn != null &&
                    focusObjects[currentFocusObjectIndex].name == btn.name)
                {
                    btn.onClick.Invoke();
                    
                    if (focusObjects.Length > currentFocusObjectIndex + 1)
                    {
                        TutorialManager.instance.GetHighlight().SetActive(false);
                        DOVirtual.DelayedCall(0.2f, NextFocus);
                        break;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            
            /*if (TutorialManager.instance.GetHighlight().GetComponent<Collider2D>().OverlapPoint(mousePos))
            {
                focusObjects[currentFocusObjectIndex].GetComponent<Button>().onClick.Invoke();
                
                if (focusObjects.Length > currentFocusObjectIndex + 1)
                {
                    TutorialManager.instance.GetHighlight().SetActive(false);
                    DOVirtual.DelayedCall(0.2f, NextFocus);
                }
                else
                {
                    return true;
                }
            }*/
        }

        return false;
    }

    public void NextFocus()
    {
        currentFocusObjectIndex++;

        TutorialManager.instance.SetHighlight(focusObjects[currentFocusObjectIndex]);
        TutorialManager.instance.GetHighlight().SetActive(true);
    }
}
