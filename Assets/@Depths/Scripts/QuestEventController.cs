using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestEventController : MonoBehaviour
{
    public float delayToDestroy = 1;

    private void Start()
    {
        Debug.Log(gameObject.name + " + destroyed");
        Destroy(gameObject, delayToDestroy);
    }
}
