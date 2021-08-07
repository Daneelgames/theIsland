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
    private GameObject lastSelectedPlantGO = null;
    [SerializeField] private PlantPart lastSelectedPlantPart;

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
            yield return new WaitForSeconds(0.05f);

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
                        PlayerUiController.instance.SelectedInteractableObject(hit.collider.gameObject,
                            selectedInteractableCenter);
                        haveSelectedObject = true;
                    }
                    else if (PlayerToolsController.instance.selectedToolIndex == 3 && hit.collider.gameObject.layer == 10) // plant part
                    {
                        SelectPlantPart(hit.collider.gameObject);
                        haveSelectedObject = true;
                    }
                }

                if (!haveSelectedObject)
                {
                    PlayerUiController.instance.NoSelectedObject();
                    if (lastSelectedPlantGO != null)
                    {
                        NoSelectedPlantPartGO();
                    }
                }
            }
            else
            {
                PlayerUiController.instance.SelectedInteractableObject(draggingObject.gameObject,
                    draggingObject.collider.bounds.center);
                if (lastSelectedPlantGO != null)
                {
                    NoSelectedPlantPartGO();
                }
            }
        }
    }

    public PlantPart SelectedPlantPart
    {
        get { return lastSelectedPlantPart; }
    }

    void SelectPlantPart(GameObject plantGo)
    {
        if (plantGo != lastSelectedPlantGO)
        {
            if (lastSelectedPlantPart)
            {
                //lastSelectedPlantPart.UnselectPart();
                lastSelectedPlantPart.ParentPlantNode.UnselectNode();
            }

            var temp = plantGo.GetComponent<PlantPart>();

            switch (temp.plantPartType)
            {
                case PlantPart.PlantPartType.Knot:
                    SelectNode(plantGo, temp);

                    break;
                case PlantPart.PlantPartType.Branch:
                    SelectNode(plantGo, temp);

                    break;
                case PlantPart.PlantPartType.Leaves:
                    SelectPart(plantGo, temp);
                    break;
                case PlantPart.PlantPartType.Fruit:
                    SelectPart(plantGo, temp);
                    break;
            }
        }
    }

    void SelectNode(GameObject plantGo, PlantPart temp)
    {
        lastSelectedPlantGO = plantGo;
        lastSelectedPlantPart = temp;
        lastSelectedPlantPart.ParentPlantNode.SelectNode();
    }
    void SelectPart(GameObject plantGo, PlantPart temp)
    {
        lastSelectedPlantGO = plantGo;
        lastSelectedPlantPart = temp;
        lastSelectedPlantPart.SelectPart();
    }

void NoSelectedPlantPartGO()
    {
        //lastSelectedPlantPart.UnselectPart();
        lastSelectedPlantPart.ParentPlantNode.UnselectNode();
        lastSelectedPlantPart = null;
        lastSelectedPlantGO = null;
    }

    private void Update()
    {
        if (raycastCooldownAfterDrop > 0)
            raycastCooldownAfterDrop -= Time.deltaTime;
        
        if (Input.GetButtonDown("Inventory"))
        {
            var inventory = PlayerInventoryController.instance.GetInventory();
            if (inventory == null || inventory.Count <= 0)
            {
                if (PlayerUiController.instance.itemWheelVisible)
                    PlayerUiController.instance.CloseItemsWheel();
                return;   
            }
            
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
            if (PlayerUiController.instance.itemWheelVisible)
            {
                InteractOnWheel(PlayerUiController.instance.GetSelectedToolOnWheel());
                PlayerUiController.instance.ResetSelectedObject();
                return;
            }
            if (draggingObject)
            {
                DropDragginObject();
                PlayerUiController.instance.ResetSelectedObject();
                return;
            }
            
            if (PlayerToolsController.instance.selectedToolIndex != -1)
            {
                PlayerToolsController.instance.UseTool();
                PlayerUiController.instance.ResetSelectedObject();
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
        Debug.Log("Interact; selectedAction " + selectedAction);
        
        if (selectedAction == -1)
            return;
        
        objectToInteract.InteractWithObject(selectedAction);
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

    public IEnumerator PickUpObject(InteractiveObject objectToInteract)
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
