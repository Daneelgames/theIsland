using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitStartsMoving : MonoBehaviour
{
    public string unitName;
    public GameObject newTarget; 
    
    void Start()
    {
        var unit = MobSpawnManager.instance.FindHcByName(unitName);
        if (unit && unit.shipController && unit.shipController.setTargetToAi)
        {
            Debug.Log("Set unit " + unitName + " moving");
            unit.shipController.setTargetToAi.SetTarget(newTarget);
            newTarget.transform.parent = null;
        }
    }
}
