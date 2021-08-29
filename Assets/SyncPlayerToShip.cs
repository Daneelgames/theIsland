using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class SyncPlayerToShip : MonoBehaviour
{
    public ShipController ship;
    Coroutine syncCoroutine;
    
    /*
    private void OnTriggerStay(Collider other)
    {
        if (syncCoroutine == null && other.gameObject == PlayerMovement.instance.gameObject)
        {
            syncCoroutine = StartCoroutine(SyncPlayer());
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == PlayerMovement.instance.gameObject)
        {
            StopAllCoroutines();
            syncCoroutine = null;
        }
    }

    IEnumerator SyncPlayer()
    {
        while (true)
        {
            Debug.Log("SyncPlayer");
            
            if (PlayerMovement.instance.rb)
                PlayerMovement.instance.rb.AddForce(ship.TargetVelocity);
            else
                PlayerMovement.instance.controller.Move(ship.TargetVelocity);
            
            yield return null;
        }
    }*/
}
