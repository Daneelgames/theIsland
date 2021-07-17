using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType {PickUp, ApplyItem}
    
    public List<InteractiveAction> actionList = new List<InteractiveAction>();
}

[Serializable]
public class InteractiveAction
{
    public InteractiveObject.ActionType actionType = InteractiveObject.ActionType.ApplyItem;
    public List<string> displayedName = new List<string>();
}
