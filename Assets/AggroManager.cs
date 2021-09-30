using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class AggroManager : MonoBehaviour
{
    public AstarWalker astarWalker;
    public float updateDelay = 1f;
    public float aggroDistanceMax = 50;
    public List<HealthController.Fraction> fractionsAggroOnSight = new List<HealthController.Fraction>();

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

    void OnEnable()
    {
        StartCoroutine(UpdateCycle());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator UpdateCycle()
    {
        if (wander)
            StartCoroutine(WanderBehaviour());
        
        float distance = 1000;
        float newDistance = 1000;
        while (true)
        {
            if (MobSpawnManager.instance == null)
            {
                yield return null;
                continue;
            }
            
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
            if (astarWalker.targetTransform && canSaveLastNonCombatTargetPosition)
            {
                lastNonCombatTargetPosition = astarWalker.targetTransform.transform.position;
            }
            
            if (closestHcToAnger != null)
            {
                if (canSaveLastNonCombatTargetPosition)
                    canSaveLastNonCombatTargetPosition = false;

                astarWalker.UpdateTargetPosition(closestHcToAnger.transform.position);
            }
            else
            {
                canSaveLastNonCombatTargetPosition = true;
                astarWalker.UpdateTargetPosition(lastNonCombatTargetPosition);
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
        lastNonCombatTargetPosition = astarWalker.targetTransform.position +
                                      Random.insideUnitSphere *
                                      Random.Range(newPosOffsetMinMax.x, newPosOffsetMinMax.y);
    }
}