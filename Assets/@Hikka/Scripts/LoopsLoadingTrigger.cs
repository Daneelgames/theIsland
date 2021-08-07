using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class LoopsLoadingTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerMovement.instance.gameObject)
        {
            LoopsLoadingManager.instance.LoadNextLoop(other.gameObject.transform.position - transform.position);
            gameObject.SetActive(false);   
        }
    }
}
