using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType
    {
        PickUp, PlantSeed, Put, TakeItem, ControlShip, ToggleLight, ToggleMusic, Grabber, Harpoon, DoorLock,
        RadioCallInteract, ChassisToggle, ListOfChoices
    }

    public bool playerCouldInteract = true;
    public List<InteractiveAction> actionList = new List<InteractiveAction>();
    
    [Header("FOR SHIPS")]
    public ShipController shipController;
    [SerializeField] GrabberController grabberController;
    [SerializeField] HarpoonController harpoonController;
    [SerializeField] DoorLockController doorLockController;
    [SerializeField] LightsToggle lightsToggle;
    [SerializeField] private Animator systemVisualAnimator;
    
    [Header("Settings for different objects")]
    public Collider collider;
    public Vector3 protableTransformOffset = Vector3.zero;
    public Rigidbody rb;
    public float dragForce = 5;
    public float zeroVelocityDistanceThreshold = 1;
    public InteractiveAction putAction;
    public LandingObject chassis;
    public List<ShipScreenButton> listOfChoicesButtons;
    
    [Header("InteractiveObjectType")]
    public PlantController plantController;
    public ToolController toolToPickUp;
    private static readonly int Update = Animator.StringToHash("Update");

    void Start()
    {
        if (InteractiveObjectsManager.instance)
        {
            if (!playerCouldInteract)
                InteractiveObjectsManager.instance.shipInteractiveObjects.Add(this);
            else
                InteractiveObjectsManager.instance.playerInteractiveObjects.Add(this);
        }
    }

    public void InteractWithObject(int selectedAction)
    {
        if (PlayerUiController.instance.showTooltips &&  actionList.Count > 0)
            PlayerAudioController.instance.OkUi();
        
        if (PlayerUiController.instance.itemWheelVisible)
        {
            // plant seed with an index of 
            PlayerToolsController.instance.SelectTool(selectedAction);
            PlayerUiController.instance.CloseItemsWheel();
            return;
        }
        if (selectedAction >= actionList.Count)
            return;
        
        if (systemVisualAnimator)
            systemVisualAnimator.SetTrigger(Update);
        
        switch (actionList[selectedAction].actionType)
        {
            case ActionType.ControlShip:
                shipController.TryToPlayerControlsShip();
                break;
            case ActionType.ToggleLight:
                lightsToggle.ToggleLight();
                break;
            case ActionType.ToggleMusic:
                shipController.TryToToggleMusic();
                break;
            case ActionType.Grabber:
                shipController.TryToUseGrabber(grabberController);
                break;
            case ActionType.Harpoon:
                shipController.TryToUseHarpoon(harpoonController);
                break;
            
            case ActionType.DoorLock:
                shipController.TryToUseDoorLock(doorLockController);
                break;
            case ActionType.RadioCallInteract:
                RadioCallsManager.playerShipInstance.Interact();
                break;
            case ActionType.ListOfChoices:
                // click on choice
                break;
            case ActionType.ChassisToggle:
                bool active = !chassis.gameObject.activeInHierarchy;
                if (chassis.feedbackTextField)
                {
                    if (active == false)
                    {
                        chassis.feedbackTextField.text = "CHASSIS CLOSED";
                    }
                    else
                    {
                        chassis.feedbackTextField.text = "CHASSIS OPENED";
                    }   
                }
                chassis.gameObject.SetActive(active);
                break;
            
            case ActionType.PickUp:
                StartCoroutine(PlayerInteractionController.instance.PickUpObject(this));
                break;
            case ActionType.PlantSeed:
                PlayerUiController.instance.ResetSelectedObject();
                PlayerUiController.instance.OpenItemsWheel();
                break;
            case ActionType.TakeItem:
                if (toolToPickUp)
                {
                    if (toolToPickUp.plantData)
                        PlayerInventoryController.instance.NewSeedFound(toolToPickUp.inventoryIndex);
                    else if (toolToPickUp.toolData)
                        PlayerInventoryController.instance.NewToolFound(toolToPickUp.inventoryIndex);
                }
                Destroy(gameObject);
                break;
        }
    }

    ShipScreenButton lastClosestButton = null;
    public void SelectClosestListOfChoicesButton(Vector3 hitPos)
    {
        float maxDistance = 10000;
        float curDistance = 0;
        ShipScreenButton closestButton = null;
        
        for (int i = 0; i < listOfChoicesButtons.Count; i++)
        {
            curDistance = Vector3.Distance(hitPos, listOfChoicesButtons[i].transform.position);
                
            if (curDistance < maxDistance)
            {
                maxDistance = curDistance;
                closestButton = listOfChoicesButtons[i];
            }
        }
        
        if (closestButton == lastClosestButton)
            return;
        
        lastClosestButton = closestButton;
        
        for (int i = 0; i < listOfChoicesButtons.Count; i++)
        {
            if (closestButton != listOfChoicesButtons[i])
            {
                listOfChoicesButtons[i].SelectTextField(false);
            }    
        }
        
        if (closestButton)
            closestButton.SelectTextField(true);
    }
}

[Serializable]
public class InteractiveAction
{
    public InteractiveObject.ActionType actionType = InteractiveObject.ActionType.PlantSeed;
    public List<string> displayedName = new List<string>();
}