using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType {PickUp, ApplyItem, Put}
    
    public List<InteractiveAction> actionList = new List<InteractiveAction>();
    public float pickUpTime = 0;
    public Vector3 protableTransformOffset = Vector3.zero;
    public Rigidbody rb;
    public float dragForce = 5;
    public float zeroVelocityDistanceThreshold = 1;

    public InteractiveAction putAction;
}

[Serializable]
public class InteractiveAction
{
    public InteractiveObject.ActionType actionType = InteractiveObject.ActionType.ApplyItem;
    public List<string> displayedName = new List<string>();
}
