using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType
    {
        PickUp, PlantSeed, Put, TakeItem, ControlShip, ToggleLight, ToggleMusic, Grabber, Harpoon, DoorLock,
        RadioCallInteract, ChassisToggle
    }

    public bool playerCouldInteract = true;
    public List<InteractiveAction> actionList = new List<InteractiveAction>();
    
    [Header("FOR SHIPS")]
    public ShipController shipController;
    [SerializeField] GrabberController grabberController;
    [SerializeField] HarpoonController harpoonController;
    [SerializeField] DoorLockController doorLockController;
    
    [Header("Settings for different objects")]
    public Collider collider;
    public Vector3 protableTransformOffset = Vector3.zero;
    public Rigidbody rb;
    public float dragForce = 5;
    public float zeroVelocityDistanceThreshold = 1;
    public InteractiveAction putAction;
    public LandingObject chassis;
    
    [Header("InteractiveObjectType")]
    public PlantController plantController;
    public ToolController toolToPickUp;

    void Start()
    {
        if (plantController && InteractiveObjectsManager.instance)
            InteractiveObjectsManager.instance.potsInteractiveObjects.Add(this);
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
        
        switch (actionList[selectedAction].actionType)
        {
            case ActionType.ControlShip:
                shipController.TryToPlayerControlsShip();
                break;
            case ActionType.ToggleLight:
                shipController.TryToToggleLight();
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
}

[Serializable]
public class InteractiveAction
{
    public InteractiveObject.ActionType actionType = InteractiveObject.ActionType.PlantSeed;
    public List<string> displayedName = new List<string>();
}
