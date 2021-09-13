using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Polarith.AI.Move;
using Polarith.AI.Package;
using UnityEngine;

public class SetTargetToAi : MonoBehaviour
{
    public GameObject currentTarget;
    public AIMSeekBounds aimSeekBounds;
    public AIMFleeBounds aimFleeBounds;
    public AIMAvoidBounds aimAvoidBounds;

    public float targetStopDistance;

    public Vector3 newTargetPosition = Vector3.zero;
    public SpaceshipController spaceshipController;
    
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
        
        /*
        for (int i = 0; i < LevelSolids.instance.solids.Count; i++)
        {
            aimFleeBounds.GameObjects.Add(LevelSolids.instance.solids[i]);
        }
        */
        //aimFleeBounds.GameObjects = new List<GameObject>(LevelSolids.instance.solids);
    }

    public void SetTarget(GameObject targetGO)
    {
        aimSeekBounds.GameObjects.Clear();
     
        currentTarget = targetGO;   
        aimSeekBounds.GameObjects.Add(currentTarget);   
        
        spaceshipController.SetNewTarget(currentTarget, targetStopDistance);
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