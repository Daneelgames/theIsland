using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public Transform playerPositionAtControl;
    // interacted with control panel
    PlayerMovement playerMovement;
    public float moveSpeedScaler = 1;
    public float torqueSpeedScaler = 0.33f;
    public Rigidbody rb;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    private bool controlledInFrame = false;

    void Start()
    {
        StartCoroutine(ControlShip());
    }

    public void PlayerControlsShip()
    {
        playerMovement = PlayerMovement.instance;

        if (playerMovement.inControl == false)
        {
            StopAllCoroutines();
            rb.velocity = Vector3.zero;
            playerMovement.PlayerControlsShip(null);
            return;   
        }

        
        playerMovement.PlayerControlsShip(this);

        StartCoroutine(MovePlayerToControlPosition());
    }

    IEnumerator MovePlayerToControlPosition()
    {
        playerPositionAtControl.position = playerMovement.transform.position;
        
        /*
        float t = 0;
        float tt = 0.75f;
        Vector3 initPos = playerMovement.transform.position;
        while (t < tt)
        {
            yield return null;
            playerMovement.transform.position = Vector3.Lerp(initPos, playerPositionAtControl.position, t/tt);
            t += Time.deltaTime;
        }*/
        
        StartCoroutine(ControlShip());
        while (true)
        {
            yield return null;
            playerMovement.transform.position = playerPositionAtControl.position;
        }
    }

    IEnumerator ControlShip()
    {
        while (true)
        {
            GetShipMovement();
            yield return null;
        }
    }

    void GetShipMovement()
    {
        //targetVelocity = currentVelocity;
        
        targetVelocity = Vector3.zero;
        controlledInFrame = false;
        if (Input.GetKey(KeyCode.W))
        {
            controlledInFrame = true;    
            targetVelocity += transform.forward;   
        }

        if (Input.GetKey(KeyCode.D))
        {
            controlledInFrame = true;
            targetVelocity += transform.right;   
        }

        if (Input.GetKey(KeyCode.S))
        {
            controlledInFrame = true;
            targetVelocity += -transform.forward;   
        }

        if (Input.GetKey(KeyCode.A))
        {
            controlledInFrame = true;
            targetVelocity += -transform.right;   
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            controlledInFrame = true;
            targetVelocity += transform.up;   
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            controlledInFrame = true;
            targetVelocity += -transform.up;   
        }
        if (Input.GetKey(KeyCode.E))
        {
            controlledInFrame = true;
            rb.AddRelativeTorque(transform.up * torqueSpeedScaler, ForceMode.Force);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            controlledInFrame = true;
            rb.AddRelativeTorque(transform.up * -torqueSpeedScaler, ForceMode.Force);
        }
        
        targetVelocity.Normalize();

        if (controlledInFrame == false)
        {
            targetVelocity = Vector3.Lerp(targetVelocity, Vector3.zero, Time.deltaTime);
        }
        
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity * moveSpeedScaler, Time.deltaTime);
        //rb.velocity = currentVelocity;
    }
}
