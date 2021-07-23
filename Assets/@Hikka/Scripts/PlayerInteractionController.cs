using System;
using System.Collections;
using PlayerControls;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    public static PlayerInteractionController instance;

    public float raycastDistance = 5;
    public LayerMask raycastLayers;
    
    private RaycastHit hit;
    private Vector3 selectedInteractableCenter;

    public InteractiveObject draggingObject;

    private float raycastCooldownAfterDrop = 0.5f;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }

    IEnumerator Start()
    {
        bool haveSelectedObject = false;
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            
            haveSelectedObject = false;

            if (raycastCooldownAfterDrop > 0)
            {
                PlayerUiController.instance.NoSelectedObject();
                continue;
            }
                
            
            if (draggingObject == null)
            {
                if (Physics.Raycast(MouseLook.instance.mainCamera.transform.position,
                    MouseLook.instance.mainCamera.transform.forward, out hit,
                    raycastDistance, raycastLayers))
                {
                    if (hit.collider.gameObject.layer == 9) // interactable
                    {
                        selectedInteractableCenter = hit.collider.bounds.center;
                        PlayerUiController.instance.SelectedInteractableObject(hit.collider.gameObject, selectedInteractableCenter);
                        haveSelectedObject = true;
                    }
                }
                if (!haveSelectedObject)
                    PlayerUiController.instance.NoSelectedObject();
            }
            else
            {
                PlayerUiController.instance.SelectedInteractableObject(draggingObject.gameObject, draggingObject.collider.bounds.center);
            }
        }
    }

    private void Update()
    {
        if (raycastCooldownAfterDrop > 0)
            raycastCooldownAfterDrop -= Time.deltaTime;
        
        if (Input.GetButtonDown("Inventory"))
        {
            if (draggingObject)
            {
                DropDragginObject();
            }
            PlayerUiController.instance.ResetSelectedObject();
            if (!PlayerUiController.instance.itemWheelVisible)
                PlayerUiController.instance.OpenItemsWheel();
            else
                PlayerUiController.instance.CloseItemsWheel();
            
            return;
        }
        
        if (Input.GetButtonDown("Interact"))
        {
            if (draggingObject)
            {
                DropDragginObject();
                return;    
            }

            var objectToInteract = PlayerUiController.instance.GetSelectedObject();

            if (objectToInteract)
            {
                Interact(objectToInteract, PlayerUiController.instance.selectedAction);
                return;
            }
        }

        if (Input.GetButtonDown("UseTool"))
        {
            PlayerUiController.instance.ResetSelectedObject();
            if (draggingObject)
            {
                DropDragginObject();
                return;
            }
            
            if (PlayerUiController.instance.itemWheelVisible)
            {
                InteractOnWheel(PlayerUiController.instance.GetSelectedToolOnWheel());
                return;
            }
            
            if (PlayerToolsController.instance.selectedToolIndex != -1)
            {
                PlayerToolsController.instance.UseTool();
                return;
            }
        }

        if (Input.GetButtonUp("UseTool"))
        {
            if (PlayerToolsController.instance.selectedToolIndex != -1)
            {
                PlayerToolsController.instance.FireButtonUp();
            }
        }
    }

    public void DropDragginObject()
    {
        draggingObject.rb.useGravity = true;
        PlayerUiController.instance.ResetSelectedObject();
        draggingObject = null;
        raycastCooldownAfterDrop = 0.5f;
    }

    public void Interact(InteractiveObject objectToInteract, int selectedAction)
    {
        if (selectedAction == -1)
            return;
        
        if (PlayerUiController.instance.showTooltips)
            PlayerAudioController.instance.OkUi();
        
        if (PlayerUiController.instance.itemWheelVisible)
        {
            // plant seed with an index of 
            PlayerToolsController.instance.SelectTool(selectedAction);
            PlayerUiController.instance.CloseItemsWheel();
            return;
        }
        
        if (selectedAction >= objectToInteract.actionList.Count)
            return;
        
        switch (objectToInteract.actionList[selectedAction].actionType)
        {
            case InteractiveObject.ActionType.PickUp:
                StartCoroutine(PickUpObject(objectToInteract));
                break;
            case InteractiveObject.ActionType.PlantSeed:
                PlayerUiController.instance.ResetSelectedObject();
                PlayerUiController.instance.OpenItemsWheel();
                break;
        }
    }
    public void InteractOnWheel(ToolController toolController)
    {
        if (toolController == null)
            return;
        
        if (PlayerUiController.instance.showTooltips)
            PlayerAudioController.instance.OkUi();
        
        if (PlayerUiController.instance.itemWheelVisible)
        {
            // plant seed with an index of 
            PlayerToolsController.instance.SelectTool(toolController);
            PlayerUiController.instance.CloseItemsWheel();
            return;
        }
    }

    IEnumerator PickUpObject(InteractiveObject objectToInteract)
    {
        Vector3 targetPos = MouseLook.instance.portableTransform.position + objectToInteract.protableTransformOffset;
        draggingObject = objectToInteract;
        PlayerUiController.instance.DragSelectedObject();
        draggingObject.rb.useGravity = false;
        
        Vector3 targetVelocity;
        while (draggingObject)
        {
            yield return null;
            targetPos = MouseLook.instance.portableTransform.position;
            if (Vector3.Distance(targetPos, objectToInteract.transform.position) < objectToInteract.zeroVelocityDistanceThreshold)
                targetVelocity = (targetPos - objectToInteract.transform.position);
            else
                targetVelocity = (targetPos - objectToInteract.transform.position).normalized * objectToInteract.dragForce;

            objectToInteract.rb.velocity = Vector3.Lerp(objectToInteract.rb.velocity, targetVelocity, Time.deltaTime * objectToInteract.dragForce);
        }
    }
}
