using System;
using System.Collections;
using System.Collections.Generic;
using Polarith.AI.Package;
using UnityEngine;

public class LandingZone : MonoBehaviour
{
    public Transform landingTransform;
    public float syncTransformSpeedScaler = 1f;

    private GameObject lastFrameFoundObject;
    private Rigidbody rbInCoroutine;
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
        if (landingObject == null)
        {
            return;
        }

        rbInCoroutine = landingObject.rb;
        landingCoroutine = StartCoroutine(LandShip(rbInCoroutine));
    }

    IEnumerator LandShip(Rigidbody rb)
    {
        Vector3 targetPos = landingTransform.position;
        Quaternion targetRot = landingTransform.rotation;

        float distancePos = Vector3.Distance(transform.position, rb.transform.position);
        float angle = Quaternion.Angle(rb.rotation, targetRot);
        
        while (distancePos > 0.5f && angle > 0.5f)
        {
            if (rb.velocity.magnitude < 1)
            {
                rb.MovePosition(Vector3.Lerp(rb.position, targetPos, syncTransformSpeedScaler * Time.deltaTime));
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, syncTransformSpeedScaler * Time.deltaTime));
            }
            
            distancePos = Vector3.Distance(transform.position, rb.transform.position);
            angle = Quaternion.Angle(rb.rotation, targetRot);

            if (distancePos > effectDistance)
            {
                yield break;   
            }
            
            yield return null;
        }

        ShipController shipController = rb.gameObject.GetComponent<ShipController>();
        if (shipController)
        {
            /*
            if (shipController._state == ShipController.State.ControlledByPlayer)
            {
                shipController.TryToPlayerControlsShip();
            }
            */

            shipController.Dock();
        }

        landingCoroutine = null;
    }
}
