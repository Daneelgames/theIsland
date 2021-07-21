using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType {PickUp, PlantSeed, Put}

    
    public List<InteractiveAction> actionList = new List<InteractiveAction>();
    public Collider collider;
    public Vector3 protableTransformOffset = Vector3.zero;
    public Rigidbody rb;
    public float dragForce = 5;
    public float zeroVelocityDistanceThreshold = 1;

    public InteractiveAction putAction;
    
    
    [Header("InteractiveObjectType")]
    public PlantController plantController;
}

[Serializable]
public class InteractiveAction
{
    public InteractiveObject.ActionType actionType = InteractiveObject.ActionType.PlantSeed;
    public List<string> displayedName = new List<string>();
}
