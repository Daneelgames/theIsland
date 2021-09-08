using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestEventController : MonoBehaviour
{
    public float delayToCheckChildsCount = 1;

    private IEnumerator Start()
    {
        while (true)
        {
            if (transform.childCount <= 0)
                break;
            yield return new WaitForSeconds(delayToCheckChildsCount);
        }
        
        Debug.Log(gameObject.name + " + destroyed");
        Destroy(gameObject, delayToCheckChildsCount);
    }
}
