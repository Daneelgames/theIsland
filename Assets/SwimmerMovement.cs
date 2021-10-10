using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwimmerMovement : MonoBehaviour
{
    public AstarWalker astarWalker;

    private Vector3 currentAstarDirection;
    [SerializeField]private Rigidbody rb;
    [SerializeField] private float speed = 1;
    
    void Start()
    {
        StartCoroutine(GetControlsFromAstarWalker());
    }
    
    IEnumerator GetControlsFromAstarWalker()
    {
        while (true)
        {
            yield return null;
          
            if (!astarWalker.ArrivedToClosestTargetTileInPath)
            {
                // MOVE UNIT TO TARGET
                currentAstarDirection = Vector3.Lerp(currentAstarDirection, astarWalker.GetDirectionToNextTile(), Time.deltaTime);
                rb.AddForce(currentAstarDirection * speed * astarWalker.aiShipSpeedScaler * Time.smoothDeltaTime, ForceMode.Force);

                if (astarWalker.lookToMovementDirection)
                {
                    rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation(astarWalker.GetDirectionToNextTile()), astarWalker.turnSpeed * Time.smoothDeltaTime);   
                }
            }
            else
            {
                if (astarWalker.lookToMovementDirection)
                {
                    rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.LookRotation((astarWalker.targetTransform.position - transform.position).normalized), astarWalker.turnSpeed * Time.smoothDeltaTime);   
                }
            }
        }
    }
}
