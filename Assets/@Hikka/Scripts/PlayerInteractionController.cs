using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    public static PlayerInteractionController instance;

    public float raycastDistance = 5;
    public LayerMask raycastLayers;
    
    private RaycastHit hit;
    private Vector3 selectedInteractableCenter;
    
    
    IEnumerator Start()
    {
        bool haveSelectedObject = false;
        while (true)
        {
            haveSelectedObject = false;
            
            if (Physics.Raycast(MouseLook.instance.mainCamera.transform.position,
                MouseLook.instance.mainCamera.transform.forward, out hit,
                raycastDistance, raycastLayers))
            {
                if (hit.collider.gameObject.layer == 9) // interactable
                {
                    selectedInteractableCenter = hit.collider.bounds.center;
                    PlayerUiController.instance.SelectedInteractableObject(selectedInteractableCenter);
                    haveSelectedObject = true;
                }
            }

            if (!haveSelectedObject)
                PlayerUiController.instance.NoSelectedObject();
            
            yield return new WaitForSeconds(0.1f);
        }
    }
}
