using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType {PickUp, PlantSeed, Put, TakeItem, ControlShip, ToggleLight, ToggleMusic, Grabber}

    public bool playerCouldInteract = true;
    public List<InteractiveAction> actionList = new List<InteractiveAction>();
    
    [Header("Settings for different objects")]
    public ShipController shipController;
    [SerializeField] GrabberController grabberController;
    public Collider collider;
    public Vector3 protableTransformOffset = Vector3.zero;
    public Rigidbody rb;
    public float dragForce = 5;
    public float zeroVelocityDistanceThreshold = 1;

    public InteractiveAction putAction;
    
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
                shipController.PlayerControlsShip();
                break;
            case ActionType.ToggleLight:
                shipController.ToggleLight();
                break;
            case ActionType.ToggleMusic:
                shipController.ToggleMusic();
                break;
            case ActionType.Grabber:
                shipController.UseGrabber(grabberController);
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
