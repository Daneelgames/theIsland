using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEditor;
using UnityEngine;

public class SetTargetToAi : MonoBehaviour
{
    public GameObject currentTarget;
    public float targetStopDistance;

    public Vector3 newTargetPosition = Vector3.zero;
    
    void Start()
    {
        if (currentTarget == null)
        {
            currentTarget = new GameObject(gameObject.name + "'s Target");
            currentTarget.transform.position = transform.position;
        }
        else if (currentTarget.transform.parent == transform)
        {
            currentTarget.transform.parent = null;
        }
        
        SetTarget(currentTarget);
    }

    private void OnDestroy()
    {
        if (currentTarget)
            Destroy(currentTarget);
    }

    public void SetTarget(GameObject targetGO)
    {
        currentTarget = targetGO;
    }

    private Coroutine moveTargetCoroutine;
    public void MoveTargetToPosition(Vector3 newPos, float speed)
    {
        if (moveTargetCoroutine != null)
            StopCoroutine(moveTargetCoroutine);

        newTargetPosition = newPos;
        moveTargetCoroutine = StartCoroutine(MoveTarget(newPos, speed));
    }

    IEnumerator MoveTarget(Vector3 newPos, float speed)
    {
        while (true)
        {
            currentTarget.transform.position = Vector3.MoveTowards(currentTarget.transform.position, newPos, speed * Time.deltaTime);
            if (Vector3.Distance(currentTarget.transform.position, newPos) < 0.5f)
                yield break;
            
            yield return null;
        }
    }
}