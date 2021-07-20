using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerUiController : MonoBehaviour
{
    public static PlayerUiController instance;

    public Canvas canvas;

    [Header("Cursor")]
    public Image selectedObjectIcon;
    public Image selectedObjectIconBackground;

    [Header("Actions UI")] 
    public Transform actionsParent;
    Vector3 actionsParentInitLocalPos = new Vector3(1000, 0, 0);
    public List<UiActionText> actionTextListUi;
    public int selectedAction = 0;
    public int selectedActionTextFontSize = 72;
    public int unselectedActionTextFontSize = 42;
    public int selectedActionBackgroundHeight = 100;
    public int unselectedActionBackgroundHeight = 50;
    public Vector3 putObjectPromtPosition;

    [Header("Time")]
    public float timeToAnimate = 0.25f;
    public float timeToSelect = 0.1f;    
    public float selectNewActionCooldown = 0.5f;    
    float selectNewActionCooldownCurrent = 0.5f;


    [Header("Items Wheel")] 
    public List<Image> itemIcons;

    private InteractiveObject currentSelectedObject;
    private bool selectedObject = false;
    private Vector3 selectedPosition;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        actionsParent.transform.localPosition = actionsParentInitLocalPos;

        foreach (var item in itemIcons)
        {
            item.transform.localScale = Vector3.zero;
        }
    }

    public InteractiveObject GetSelectedObject()
    {
        return currentSelectedObject;
    }

    public void ResetSelectedObject()
    {
        selectedObject = false;
    }
    
    private Vector3 screenPos;
    private Vector3 uiFeedbackPosition;
    private GameObject lastSelectedGameObject = null;

    private Coroutine animatePointerCoroutine;
    private Coroutine movePointerCoroutine;
    public void SelectedInteractableObject(GameObject newSelectedGameObject, Vector3 newPos)
    {
        if (newSelectedGameObject != lastSelectedGameObject)
        {
            currentSelectedObject = newSelectedGameObject.GetComponent<InteractiveObject>();
            lastSelectedGameObject = newSelectedGameObject;
        }
        
        if (!selectedObject)
        {
            selectedObject = true;
            
            if (animatePointerCoroutine != null)
                StopCoroutine(animatePointerCoroutine);
            if (movePointerCoroutine != null)
                StopCoroutine(movePointerCoroutine);
            
            animatePointerCoroutine = StartCoroutine(AnimatePointer());
            movePointerCoroutine = StartCoroutine(MovePointer());
        }
        selectedPosition = newPos;
    }

    public void NoSelectedObject()
    {
        if (!currentSelectedObject && !lastSelectedGameObject && !selectedObject)
            return;
        
        currentSelectedObject = null;
        lastSelectedGameObject = null;
        selectedObject = false;
        StopCoroutine(movePointerCoroutine);
    }

    IEnumerator AnimatePointer()
    {
        float t = 0;
        Vector3 actionsUiTempPosition;
        actionsParentInitLocalPos.y = selectedObjectIcon.transform.localPosition.y;

        if (PlayerInteractionController.instance.draggingObject)
        {
            for (int i = 0; i < actionTextListUi.Count; i++)
            {
                if (i > 0)
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = false;
                    actionTextListUi[i].uiText.enabled = false;
                }
                else
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = true;
                    actionTextListUi[i].uiText.enabled = true;
                    actionTextListUi[i].uiText.text = currentSelectedObject.putAction.displayedName[GameManager.instance.gameLanguage].ToUpper();
                }
            }   
        }
        else
        {
            for (int i = 0; i < actionTextListUi.Count; i++)
            {
                if (i >= currentSelectedObject.actionList.Count)
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = false;
                    actionTextListUi[i].uiText.enabled = false;
                }
                else
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = true;
                    actionTextListUi[i].uiText.enabled = true;
                    actionTextListUi[i].uiText.text = currentSelectedObject.actionList[i].displayedName[GameManager.instance.gameLanguage].ToUpper();
                }
            }   
        }
        
        while (t < timeToAnimate)
        {
            t += Time.deltaTime;

            if (PlayerInteractionController.instance.draggingObject)
            {
                actionsParent.transform.localPosition = Vector3.Lerp(actionsParentInitLocalPos, putObjectPromtPosition, t /timeToAnimate);
                selectedObjectIcon.transform.localScale = Vector3.zero;
                selectedObjectIconBackground.transform.localScale = Vector3.zero;
            }
            else
            {
                actionsParent.transform.localPosition = Vector3.Lerp(actionsParentInitLocalPos, selectedObjectIcon.transform.localPosition, t /timeToAnimate);
                selectedObjectIcon.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t/timeToAnimate);
                selectedObjectIconBackground.transform.localScale = selectedObjectIcon.transform.localScale;   
            }
            yield return null;
        }

        if (!PlayerInteractionController.instance.draggingObject)
            selectedObjectIconBackground.transform.localScale = Vector3.one + new Vector3(0.1f, 0.1f, 0);
        
        while (selectedObject)
        {
            if (!PlayerInteractionController.instance.draggingObject)
                actionsParent.transform.localPosition = selectedObjectIcon.transform.localPosition;
            yield return null;
        }

        t = 0;
        actionsUiTempPosition = actionsParent.transform.localPosition;
        actionsParentInitLocalPos.y = actionsParent.transform.localPosition.y;
        while (t < timeToAnimate)
        {
            t += Time.deltaTime;
            actionsParent.transform.localPosition = Vector3.Lerp(actionsUiTempPosition, new Vector3(actionsParentInitLocalPos.x, selectedObjectIcon.transform.localPosition.y, 0), t /timeToAnimate);

            if (!PlayerInteractionController.instance.draggingObject)
            {
                selectedObjectIcon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero , t/timeToAnimate);
                selectedObjectIconBackground.transform.localScale = selectedObjectIcon.transform.localScale;
            }
            yield return null;
        }

        animatePointerCoroutine = null;
    }

    private int previuosSelectedAction = 0;
    IEnumerator MovePointer()
    {
        previuosSelectedAction = -1;
        selectNewActionCooldownCurrent = 0;
        Vector3 mousePos;
        float distance = 0;
        float newDistance = 0;
        
        while (true)
        {
            if (selectNewActionCooldownCurrent > 0)
                selectNewActionCooldownCurrent -= Time.deltaTime;
            
            screenPos = MouseLook.instance.mainCamera.WorldToScreenPoint(selectedPosition);
            screenPos.z = (canvas.transform.position - MouseLook.instance.handsCamera.transform.position).magnitude;
            uiFeedbackPosition = MouseLook.instance.handsCamera.ScreenToWorldPoint(screenPos);
        
            selectedObjectIcon.transform.position = uiFeedbackPosition;
            selectedObjectIconBackground.transform.position = selectedObjectIcon.transform.position;

            if (selectNewActionCooldownCurrent <= 0)
            {
                // get mouse pos
                mousePos = Input.mousePosition;
                mousePos.z = 10.0f; //distance of the plane from the camera
                mousePos = MouseLook.instance.mainCamera.ScreenToWorldPoint(mousePos);
                distance = 10000000;

                // find closest action
                for (int i = 0; i < actionTextListUi.Count; i++)
                {
                    if (actionTextListUi[i].uiBackgroundImage.enabled == false)
                        continue;
                
                    newDistance = Vector3.Distance(mousePos, actionTextListUi[i].targetTransformForCursor.position);
                    if (newDistance < distance)
                    {
                        selectedAction = i;
                        distance = newDistance;
                    }
                }
                
                if (previuosSelectedAction != selectedAction)
                {
                    previuosSelectedAction = selectedAction;
                    selectNewActionCooldownCurrent = selectNewActionCooldown;
                    PlayerAudioController.instance.SelectNewUiAction();
                    for (int i = 0; i < actionTextListUi.Count; i++)
                    {
                        if (i == selectedAction)
                        {
                            StartCoroutine(SelectActionMenu(i));
                        }
                        else if (actionTextListUi[i].uiBackgroundImage.enabled)
                        {
                            StartCoroutine(UnselectActionMenu(i));
                        }
                    }
                
                }
            }
            
            yield return null;
        }
    }

    IEnumerator SelectActionMenu(int index)
    {
        float t = 0;
        var startHeight = actionTextListUi[index].uiBackgroundImage.rectTransform.sizeDelta.y; 
        var startFontSize = actionTextListUi[index].uiText.fontSize; 
        actionTextListUi[index].uiBackgroundImage.color = Color.white;
        actionTextListUi[index].uiText.color = Color.black;
        while (t < timeToSelect)
        {
            t += Time.deltaTime;
            
            actionTextListUi[index].uiBackgroundImage.rectTransform.sizeDelta = new Vector2(2555, Mathf.Lerp(startHeight,selectedActionBackgroundHeight, t / timeToAnimate));
            actionTextListUi[index].uiText.fontSize = (int)Mathf.Lerp(startFontSize,selectedActionTextFontSize, t / timeToAnimate);
            yield return null;
        }
        actionTextListUi[index].uiBackgroundImage.rectTransform.sizeDelta = new Vector2(2555, selectedActionBackgroundHeight);
        actionTextListUi[index].uiText.fontSize = selectedActionTextFontSize;
    }
    IEnumerator UnselectActionMenu(int index)
    {
        float t = 0;
        var startHeight = actionTextListUi[index].uiBackgroundImage.rectTransform.sizeDelta.y; 
        var startFontSize = actionTextListUi[index].uiText.fontSize; 
        actionTextListUi[index].uiBackgroundImage.color = Color.black;
        actionTextListUi[index].uiText.color = Color.white;
        while (t < timeToSelect)
        {
            t += Time.deltaTime;
            
            actionTextListUi[index].uiBackgroundImage.rectTransform.sizeDelta = new Vector2(2555, Mathf.Lerp(startHeight,unselectedActionBackgroundHeight, t / timeToAnimate));
            actionTextListUi[index].uiText.fontSize = (int)Mathf.Lerp(startFontSize,unselectedActionTextFontSize, t / timeToAnimate);
            yield return null;
        }
        actionTextListUi[index].uiBackgroundImage.rectTransform.sizeDelta = new Vector2(2555, unselectedActionBackgroundHeight);
        actionTextListUi[index].uiText.fontSize = unselectedActionTextFontSize;
    }

    public void CloseItemsWheel()
    {
        
    }
    
    public void OpenItemsWheel()
    {
        
    }
}

[Serializable]
public class UiActionText
{
    public Text uiText;
    public Image uiBackgroundImage;
    public Transform targetTransformForCursor;
}
