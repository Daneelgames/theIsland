using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AggroManager : MonoBehaviour
{
    public SetTargetToAi setTargetToAi;
    public float updateDelay = 1f;
    public float aggroDistanceMax = 50;
    public List<HealthController.Fraction> fractionsAggroOnSight = new List<HealthController.Fraction>();
    public float targetChangeMoveSpeed = 1000;

    private Vector3 lastNonCombatTargetPosition;

    private bool canSaveLastNonCombatTargetPosition = true;
    
    private IEnumerator Start()
    {
        float distance = 1000;
        float newDistance = 1000;
        while (true)
        {
            HealthController closestHcToAnger = null;
            distance = 1000;
            for (int i = 0; i < MobSpawnManager.instance.Units.Count; i++)
            {
                if (!fractionsAggroOnSight.Contains(MobSpawnManager.instance.Units[i].fraction))
                {
                    continue;
                }
                newDistance = Vector3.Distance(transform.position, MobSpawnManager.instance.Units[i].transform.position);
                if (newDistance < aggroDistanceMax)
                {
                    if (newDistance < distance)
                    {
                        distance = newDistance;
                        closestHcToAnger = MobSpawnManager.instance.Units[i];
                    }
                }

                yield return null;
            }
            
            if (setTargetToAi.currentTarget && canSaveLastNonCombatTargetPosition)
            {
                lastNonCombatTargetPosition = setTargetToAi.currentTarget.transform.position;
            }
            
            if (closestHcToAnger != null)
            {
                if (canSaveLastNonCombatTargetPosition)
                    canSaveLastNonCombatTargetPosition = false;
                
                setTargetToAi.MoveTargetToPosition(closestHcToAnger.transform.position, targetChangeMoveSpeed);
            }
            else
            {
                canSaveLastNonCombatTargetPosition = true;
                setTargetToAi.MoveTargetToPosition(lastNonCombatTargetPosition, targetChangeMoveSpeed);
            }
            
            yield return new WaitForSeconds(updateDelay);
        }
    }
}