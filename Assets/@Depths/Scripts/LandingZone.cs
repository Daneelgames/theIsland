using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingZone : MonoBehaviour
{
    public Transform landingTransform;
    public float syncTransformSpeedScaler = 1f;

    private GameObject lastFrameFoundObject;
    private LandingObject landingObjectInCoroutine;
    private Coroutine landingCoroutine;

    public float effectDistance = 5;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != 7)
            return;
        if (other.gameObject == lastFrameFoundObject)
            return;
    
        lastFrameFoundObject = other.gameObject;
        LandingObject landingObject = other.gameObject.GetComponent<LandingObject>();
        if (landingObject == null || landingObject.shipController._state == ShipController.State.ControlledByPlayer || landingObject.shipController._state == ShipController.State.Docked)
        {
            return;
        }

        landingObjectInCoroutine = landingObject;
        landingCoroutine = StartCoroutine(LandShip(landingObjectInCoroutine));
    }

    IEnumerator LandShip(LandingObject landingObject)
    {
        Vector3 targetPos = landingTransform.position;
        Quaternion targetRot = landingTransform.rotation;

        float distancePos = Vector3.Distance(transform.position, landingObject.rb.transform.position);
        float angle = Quaternion.Angle(landingObject.rb.rotation, targetRot);
        
        
        if (landingObject.shipController == null)
            yield break;
        
        while (distancePos > 0.5f && angle > 0.5f)
        {
            if (landingObject.rb.velocity.magnitude < 1)
            {
                landingObject.rb.MovePosition(Vector3.Lerp(landingObject.rb.position, targetPos, syncTransformSpeedScaler * Time.deltaTime));
                landingObject.rb.MoveRotation(Quaternion.Slerp(landingObject.rb.rotation, targetRot, syncTransformSpeedScaler * Time.deltaTime));
            }
            
            distancePos = Vector3.Distance(transform.position, landingObject.rb.transform.position);
            angle = Quaternion.Angle(landingObject.rb.rotation, targetRot);

            if (distancePos > effectDistance)
            {
                yield break;   
            }
            
            yield return null;
        }

        if (landingObject.shipController)
        {
            landingObject.shipController.Dock();
        }
        landingCoroutine = null;
    }
}
