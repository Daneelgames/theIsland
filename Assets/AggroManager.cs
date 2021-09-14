using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class AggroManager : MonoBehaviour
{
    public SetTargetToAi setTargetToAi;
    public float updateDelay = 1f;
    public float aggroDistanceMax = 50;
    public List<HealthController.Fraction> fractionsAggroOnSight = new List<HealthController.Fraction>();
    public float targetChangeMoveSpeed = 1000;

    private Vector3 lastNonCombatTargetPosition;

    private bool canSaveLastNonCombatTargetPosition = true;

    [Header("Wander Behaviour")] 
    public bool wander = true;
    public Vector2 wanderTimeMinMax = new Vector2(5f, 30);
    public Vector2 newPosOffsetMinMax = new Vector2(5f, 30f);
    
    public float maxDstBetweenPositionsToCountAsIdle = 0.5f;
    public float maxIdleTimeBeforeMovingTarget = 3;
    private Vector3 prevStepPos;
    private Vector3 curStepPos;
    private IEnumerator Start()
    {
        if (wander)
            StartCoroutine(WanderBehaviour());
        
        float distance = 1000;
        float newDistance = 1000;
        while (true)
        {
            // FIND CLOSEST UNIT TO ANGER ON
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
            
            // SAVE LAST NON COMBAT TARGET POSITION
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

    IEnumerator WanderBehaviour()
    {
        float timeToWait = 1;
        StartCoroutine(CheckIfStuck());
        
        while (true)
        {
            UpdateLastNonCombatPos();
            //setTargetToAi.MoveTargetToPosition(lastNonCombatTargetPosition, targetChangeMoveSpeed);
            
            timeToWait = Random.Range(wanderTimeMinMax.x, wanderTimeMinMax.y);
            yield return new WaitForSeconds(timeToWait);
        }
    }

    IEnumerator CheckIfStuck()
    {
        prevStepPos = transform.position;
        while (true)
        {
            curStepPos = transform.position;
            yield return new WaitForSeconds(maxIdleTimeBeforeMovingTarget);
            
            if (Vector3.Distance(curStepPos, transform.position) <= maxDstBetweenPositionsToCountAsIdle)
            {
                UpdateLastNonCombatPos();
            }
            prevStepPos = curStepPos;
        }
    }

    void UpdateLastNonCombatPos()
    {
        lastNonCombatTargetPosition = setTargetToAi.transform.position +
                                      Random.insideUnitSphere *
                                      Random.Range(newPosOffsetMinMax.x, newPosOffsetMinMax.y);
    }
}