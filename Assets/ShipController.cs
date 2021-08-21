using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public Transform playerPositionAtControl;
    // interacted with control panel
    PlayerMovement playerMovement;
    public float baseSpeedScaler = 5;
    public Rigidbody rb;
    
    public void PlayerControlsShip()
    {
        playerMovement = PlayerMovement.instance;
        
        if (playerMovement.inControl == false)
            return;

        
        playerMovement.PlayerControlsShip(this);

        StartCoroutine(MovePlayerToControlPosition());
    }

    IEnumerator MovePlayerToControlPosition()
    {
        float t = 0;
        float tt = 0.75f;
        Vector3 initPos = playerMovement.transform.position;
        while (t < tt)
        {
            yield return null;
            playerMovement.transform.position = Vector3.Lerp(initPos, playerPositionAtControl.position, t/tt);
            t += Time.deltaTime;
        }
        StartCoroutine(ControlShip());
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
        if (Input.GetKey(KeyCode.W))
            rb.AddForce(transform.forward * baseSpeedScaler);
        if (Input.GetKey(KeyCode.D))
            rb.AddForce(transform.right * baseSpeedScaler);
        if (Input.GetKey(KeyCode.S))
            rb.AddForce(transform.forward * - baseSpeedScaler);
        if (Input.GetKey(KeyCode.A))
            rb.AddForce(transform.right * - baseSpeedScaler);
        if (Input.GetKey(KeyCode.LeftShift))
            rb.AddForce(transform.up * baseSpeedScaler);
        if (Input.GetKey(KeyCode.LeftControl))
            rb.AddForce(transform.up * - baseSpeedScaler);
    }
}
