using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class QuestEventOnDocking : MonoBehaviour
{
    public QuestTrigger questTrigger;
    private IEnumerator Start()
    {
        while (true)
        {
            if (PlayerMovement.instance.shipInControl && PlayerMovement.instance.shipInControl._state == ShipController.State.Docked)
            {
                StartCoroutine(questTrigger.Activate());
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
