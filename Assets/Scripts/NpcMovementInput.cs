using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class NpcMovementInput : MonoBehaviour
{
    public PlayerMovement npcMovement;
    
    private Vector3 movementVector;

    private Transform target;
    
    void Start()
    {
        target = PlayerMovement.instance.transform;
    }
    
    void Update()
    {
        CalculateMovementVector();

        if (Input.GetKeyDown("x"))
            transform.position = target.position;
    }

    private float distance = 0;
    void CalculateMovementVector()
    {
        npcMovement.movementTransform.LookAt(target);
        distance = Vector3.Distance(transform.position, target.position); 
        if (distance < 5 || distance > 50)
        {
            movementVector = Vector2.zero;   
        }
        else
        {
            movementVector = Vector3.forward;
            if (target.position.y > transform.position.y)
                movementVector += Vector3.up;
            else if (target.position.y < transform.position.y)
                movementVector += Vector3.down;
            
            if (target.position.y > transform.position.y + 20)
                npcMovement.Dash();
            
            /*
            movementVector = target.position - transform.position;
            movementVector = movementVector.normalized;

            movementVector.x = Mathf.RoundToInt(movementVector.x);
            movementVector.z = Mathf.RoundToInt(movementVector.z);*/
        }
        
        npcMovement.GetNpcMovementInput(movementVector.x, movementVector.z);
    }
}