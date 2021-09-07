using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStartsMoving : MonoBehaviour
{
    public string unitName;
    public GameObject newTarget;
    public float speed = 1;
    
    void Start()
    {
        var unit = MobSpawnManager.instance.FindHcByName(unitName);
        if (unit && unit.shipController && unit.shipController.setTargetToAi)
        {
            unit.shipController.setTargetToAi.MoveTargetToPosition(newTarget.transform.position, speed);
        }
    }
}
