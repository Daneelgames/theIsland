using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerUiController : MonoBehaviour
{
    public static PlayerUiController instance;

    public Canvas canvas;
    public bool showTooltips = false;
    [Header("Cursor")]
    public Image selectedObjectIcon;
    public Image selectedObjectIconBackground;

    [Header("Actions UI")] 
    public bool showActionsNames = false;
    public Transform actionsParent;
    Vector3 actionsParentInitLocalPos = new Vector3(1000, 0, 0);
    public List<UiActionText> actionTextListUi;
    public int selectedAction = 0;
    public int selectedActionTextFontSize = 72;
    public int unselectedActionTextFontSize = 42;
    public int selectedActionBackgroundHeight = 100;
    public int unselectedActionBackgroundHeight = 50;
    public Vector3 putObjectPromtPosition;
    private InteractiveObject currentSelectedObject;
    private bool selectedObject = false;
    private Vector3 selectedPosition;

    [Header("Time")]
    public float timeToAnimate = 0.25f;
    public float timeToSelect = 0.1f;    
    public float selectNewActionCooldown = 0.5f;    
    float selectNewActionCooldownCurrent = 0.5f;


    [Header("Items Wheel")] 
    public bool itemWheelVisible = false;
    public Transform itemWheel;
    public List<UiItemOnWheel> itemIcons;
    int selectedItemIndexOnWheel = -1;
    int previuosSelectedItemOnWheel = -1;
    List<InventorySlot> inventory;

    
    private Vector3 screenPos;
    private Vector3 uiFeedbackPosition;
    private GameObject lastSelectedGameObject = null;

    private Coroutine animatePointerCoroutine;
    private Coroutine movePointerCoroutine;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        actionsParent.transform.localPosition = actionsParentInitLocalPos;
    }

    private void Start()
    {
        CloseItemsWheel();
    }

    public InteractiveObject GetSelectedObject()
    {
        return currentSelectedObject;
    }

    public void ResetSelectedObject()
    {
        selectedObject = false;
    }
    public void DragSelectedObject()
    {
        selectedObject = false;
        SelectedInteractableObject(currentSelectedObject.gameObject, currentSelectedObject.collider.bounds.center, Vector3.zero);
    }
    
    public void SelectedInteractableObject(GameObject newSelectedGameObject, Vector3 newPos, Vector3 hitPoint)
    {
        if (newSelectedGameObject != lastSelectedGameObject)
        {
            var s = newSelectedGameObject.GetComponent<InteractiveObject>();

            if (s.actionList.Count <= 0 || s == null || s.playerCouldInteract == false)
                return;
            
            currentSelectedObject = s; 
            
            lastSelectedGameObject = newSelectedGameObject;
            
            PlayerAudioController.instance.SelectNewUiAction();
            if (s.listOfChoicesButtons.Count > 0)
            {
                s.SelectClosestListOfChoicesButton(hitPoint);
            }
        }
        else
        {
            currentSelectedObject.SelectClosestListOfChoicesButton(hitPoint);
        }
        
        if (!selectedObject)
        {
            selectedObject = true;

            if (showTooltips)
            {
                if (animatePointerCoroutine != null)
                    StopCoroutine(animatePointerCoroutine);
                if (movePointerCoroutine != null)
                    StopCoroutine(movePointerCoroutine);
            
                animatePointerCoroutine = StartCoroutine(AnimatePointer());
                movePointerCoroutine = StartCoroutine(MovePointer());   
            }
        }
        selectedPosition = newPos;
    }

    public void NoSelectedObject()
    {
        if (!currentSelectedObject && !lastSelectedGameObject && !selectedObject)
            return;
        
        if (showTooltips && lastSelectedGameObject && showActionsNames)
            PlayerAudioController.instance.CloseUi();
        
        currentSelectedObject = null;
        lastSelectedGameObject = null;
        selectedObject = false;
        selectedAction = -1;

        if (showTooltips && movePointerCoroutine != null)
            StopCoroutine(movePointerCoroutine);

        if (itemWheelVisible)
        {
            CloseItemsWheel();
        }
    }

    IEnumerator AnimatePointer()
    {
        float t = 0;
        Vector3 actionsUiTempPosition;
        actionsParentInitLocalPos.y = selectedObjectIcon.transform.localPosition.y;

        if (itemWheelVisible)
        {
            for (int i = 0; i < actionTextListUi.Count; i++)
            {
                actionTextListUi[i].uiBackgroundImage.enabled = false;
                actionTextListUi[i].uiText.enabled = false;
            }   
        }
        else if (PlayerInteractionController.instance.draggingObject)
        {
            for (int i = 0; i < actionTextListUi.Count; i++)
            {
                if (i > 0)
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = false;
                    actionTextListUi[i].uiText.enabled = false;
                }
                else if (showActionsNames)
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = true;
                    actionTextListUi[i].uiText.enabled = true;
                    actionTextListUi[i].uiText.text = currentSelectedObject.putAction.displayedName[GameManager.instance.gameLanguage].ToUpper();
                }
                else
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = false;
                    actionTextListUi[i].uiText.enabled = false;
                    actionTextListUi[i].uiText.text = String.Empty;
                }
            }   
        }
        else
        {
            for (int i = 0; i < actionTextListUi.Count; i++)
            {
                if (i >= currentSelectedObject.actionList.Count || (currentSelectedObject.actionList[i].actionType == InteractiveObject.ActionType.PlantSeed && selectedItemIndexOnWheel == -1))
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = false;
                    actionTextListUi[i].uiText.enabled = false;
                }
                else if (showActionsNames)
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = true;
                    actionTextListUi[i].uiText.enabled = true;
                    actionTextListUi[i].uiText.text = currentSelectedObject.actionList[i].displayedName[GameManager.instance.gameLanguage].ToUpper();
                }
                else
                {
                    actionTextListUi[i].uiBackgroundImage.enabled = false;
                    actionTextListUi[i].uiText.enabled = false;
                    actionTextListUi[i].uiText.text = String.Empty;
                }
            }   
        }
        
        while (t < timeToAnimate)
        {
            t += Time.smoothDeltaTime;

            // MOVE SELECTABLE ACTIONS
            if (PlayerInteractionController.instance.draggingObject)
            {
            
                actionsParent.transform.localPosition = Vector3.Lerp(actionsParentInitLocalPos, putObjectPromtPosition, t /timeToAnimate);
                selectedObjectIcon.transform.localScale = Vector3.zero;
                selectedObjectIconBackground.transform.localScale = Vector3.zero;   
            }
            else
            {
                //actionsParent.transform.localPosition = Vector3.Lerp(actionsParentInitLocalPos, selectedObjectIcon.transform.localPosition, t /timeToAnimate);
                actionsParent.transform.localPosition =  selectedObjectIcon.transform.localPosition;
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
            {
                actionsParent.transform.localPosition = selectedObjectIcon.transform.localPosition;   
            }
            yield return null;
        }

        var mousePos = Input.mousePosition;
        while (itemWheelVisible)
        {
            if (!PlayerInteractionController.instance.draggingObject)
            {
                mousePos = Input.mousePosition;
                mousePos.z = 10;
                actionsParent.transform.localPosition = MouseLook.instance.handsCamera.ScreenToWorldPoint(mousePos);
            }
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

            if (!itemWheelVisible && currentSelectedObject)
            {
                screenPos = MouseLook.instance.mainCamera.WorldToScreenPoint(selectedPosition);
                screenPos.z = (canvas.transform.position - MouseLook.instance.handsCamera.transform.position).magnitude;   
            }
            else
            {
                screenPos = Input.mousePosition;
                screenPos.z = 10;
            }
            
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

                if (itemWheelVisible)
                {
                    // find closest item on wheel
                    for (int i = 0; i < itemIcons.Count; i++)
                    {
                        if (itemIcons[i].uiImage.enabled == false)
                            continue;
                
                        newDistance = Vector3.Distance(mousePos, itemIcons[i].uiImage.transform.position);
                        if (newDistance < distance)
                        {
                            selectedItemIndexOnWheel = i;
                            distance = newDistance;
                        }
                    }
                
                    if (previuosSelectedItemOnWheel != selectedItemIndexOnWheel)
                    {
                        previuosSelectedItemOnWheel = selectedItemIndexOnWheel;
                        selectNewActionCooldownCurrent = selectNewActionCooldown;
                        
                        PlayerAudioController.instance.SelectNewUiAction();
                        for (int i = 0; i < itemIcons.Count; i++)
                        {
                            if (i == selectedItemIndexOnWheel)
                            {
                                StartCoroutine(SelectItemOnWheel(i));
                            }
                            else if (itemIcons[i].uiImage.enabled)
                            {
                                StartCoroutine(UnselectItemOnWheel(i));
                            }
                        }
                    }   
                }
                else
                {
                    selectedAction = 0;
                    // find closest action
                    for (int i = 0; i < actionTextListUi.Count; i++)
                    {
                        if ((showActionsNames && actionTextListUi[i].uiBackgroundImage.enabled == false) || actionTextListUi[i].uiText.text == String.Empty)
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
                        
                        //if (showTooltips)
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
            }
            
            yield return null;
        }
    }

    IEnumerator SelectActionMenu(int index)
    {
        if (!showActionsNames)
        {
            yield break;   
        }
        
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
        PlayerAudioController.instance.CloseUi();
        UniversalCursorController.instance.HideCursor();
        itemWheelVisible = false;

        if (animateItemWheelCoroutine != null)
        {
            StopCoroutine(animateItemWheelCoroutine);
            animateItemWheelCoroutine = null;
        }

        StartCoroutine(CloseItemsOnWheel());
    }
    
    IEnumerator CloseItemsOnWheel()
    {
        float t = 0;
        while (t < timeToSelect)
        {
            t += Time.deltaTime;
            for (int index = 0; index < itemIcons.Count; index++)
            {
                itemIcons[index].uiBackgroundImage.transform.localScale = Vector3.Lerp( itemIcons[index].uiBackgroundImage.transform.localScale, Vector3.zero,t/timeToSelect);   
            }
            yield return null;
        }
        
        for (int index = 0; index < itemIcons.Count; index++)
        {
            itemIcons[index].uiBackgroundImage.transform.localScale = Vector3.zero;   
            itemIcons[index].uiImage.enabled = false;
            itemIcons[index].uiBackgroundImage.enabled = false;
        }
    }
    
    public void OpenItemsWheel()
    {
        PlayerAudioController.instance.OkUi();
        UniversalCursorController.instance.ShowCursor();
        itemWheelVisible = true;

        inventory =  PlayerInventoryController.instance.GetInventory();

        for (int i = 0; i < inventory.Count; i++)
        {
            if (itemIcons.Count <= i)
                break;

            itemIcons[i].uiImage.enabled = true;
            itemIcons[i].uiBackgroundImage.enabled = true;
            itemIcons[i].uiBackgroundImage.transform.localScale = Vector3.one;
            if (inventory[i].plantData)
                itemIcons[i].uiImage.sprite = inventory[i].plantData.plantIcon;
            else if (inventory[i].toolData)
                itemIcons[i].uiImage.sprite = inventory[i].toolData.toolIcon;
        }
        
        animateItemWheelCoroutine = StartCoroutine(AnimateItemWheel());
        for (int i = 0; i < itemIcons.Count; i++)
        {
            if (i == 0)
            {
                StartCoroutine(SelectItemOnWheel(i));
            }
            else
            {
                StartCoroutine(UnselectItemOnWheel(i));
            }
        }
        if (animatePointerCoroutine != null)
            StopCoroutine(animatePointerCoroutine);
        if (movePointerCoroutine != null)
            StopCoroutine(movePointerCoroutine);
            
        animatePointerCoroutine = StartCoroutine(AnimatePointer());
        movePointerCoroutine = StartCoroutine(MovePointer());
    }

    private Coroutine animateItemWheelCoroutine;
    IEnumerator AnimateItemWheel()
    {
        while (true)
        {
            var cursorPos = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            cursorPos.z = 10.0f; //distance of the plane from the camera
            cursorPos = MouseLook.instance.mainCamera.ScreenToWorldPoint(cursorPos);
            
            itemWheel.position = cursorPos;
            
            //itemWheel.position = selectedObjectIcon.transform.position;
            yield return null;
        }
    }

    public void SetSelectedItemIndexOnWheel(int newIndex)
    {
        selectedItemIndexOnWheel = newIndex;
    }
    
    IEnumerator SelectItemOnWheel(int index)
    {
        if (index == -1 || index >= itemIcons.Count)
            yield break;
        
        float t = 0;
        itemIcons[index].uiBackgroundImage.color = Color.white;
        itemIcons[index].uiImage.color = Color.black;
        if (inventory[index].plantData)
            itemIcons[index].itemName.text = inventory[index].plantData.plantName[GameManager.instance.gameLanguage];
        else if (inventory[index].toolData)
            itemIcons[index].itemName.text = inventory[index].toolData.toolName[GameManager.instance.gameLanguage];
        
        while (t < timeToSelect)
        {
            t += Time.deltaTime;
            
            itemIcons[index].uiBackgroundImage.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.25f, t/timeToSelect);
            yield return null;
        }
        itemIcons[index].uiBackgroundImage.transform.localScale = Vector3.one * 1.25f;
    }

    public ToolController GetSelectedToolOnWheel()
    {
        if (selectedItemIndexOnWheel == -1)
            return null;
        
        for (int i = 0; i < PlayerToolsController.instance.allTools.Count; i++)
        {
            if (PlayerToolsController.instance.allTools[i].inventoryIndex == inventory[selectedItemIndexOnWheel].inventoryIndex)
            {
                return PlayerToolsController.instance.allTools[i];
            }
        }
        return null;
    }
    
    IEnumerator UnselectItemOnWheel(int index)
    {
        if (index == -1)
            yield break;
        
        float t = 0;
        itemIcons[index].uiBackgroundImage.color = Color.black;
        itemIcons[index].uiImage.color = Color.white;
        itemIcons[index].itemName.text = String.Empty;
        
        while (t < timeToSelect)
        {
            t += Time.deltaTime;
            
            itemIcons[index].uiBackgroundImage.transform.localScale = Vector3.Lerp( Vector3.one * 1.25f, Vector3.one * 0.75f,t/timeToSelect);
            yield return null;
        }
        itemIcons[index].uiBackgroundImage.transform.localScale = Vector3.one * 0.75f;
    }
}

[Serializable]
public class UiActionText
{
    public Text uiText;
    public Image uiBackgroundImage;
    public Transform targetTransformForCursor;
}

[Serializable]
public class UiItemOnWheel
{
    public Image uiBackgroundImage;
    public Image uiImage;
    public Text itemName;
}
