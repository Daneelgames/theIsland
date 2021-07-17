using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum ActionType {PickUp, ApplyItem}
    
    public List<ActionType> actionList = new List<ActionType>();
}
