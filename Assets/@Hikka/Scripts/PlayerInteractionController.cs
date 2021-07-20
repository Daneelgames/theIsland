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
                PlayerUiController.instance.SelectedInteractableObject(draggingObject.gameObject, Vector3.zero);
            }
            
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (draggingObject)
            {
                draggingObject.rb.useGravity = true;
                PlayerUiController.instance.ResetSelectedObject();
                draggingObject = null;
                return;    
            }
            
            var objectToInteract = PlayerUiController.instance.GetSelectedObject();

            if (objectToInteract)
            {
                Interact(objectToInteract, PlayerUiController.instance.selectedAction);
            }
        }
    }

    public void Interact(InteractiveObject objectToInteract, int selectedAction)
    {
        PlayerAudioController.instance.OkUi();
        if (PlayerUiController.instance.itemWheelVisible)
        {
            // plant seed with an index of 
            PlayerUiController.instance.GetSelectedItemOnWheel();
            return;
        }
        
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

    IEnumerator PickUpObject(InteractiveObject objectToInteract)
    {
        Vector3 targetPos = MouseLook.instance.portableTransform.position + objectToInteract.protableTransformOffset;
        draggingObject = objectToInteract;
        PlayerUiController.instance.ResetSelectedObject();
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
