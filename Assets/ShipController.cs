using System;
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
    public float dragScale = 0.5f;
    public float accelerationScale = 1;
    public float torqueSpeedScaler = 0.33f;
    public Rigidbody rb;
    private Vector3 currentVelocity;
    private Vector3 targetVelocity;
    private bool controlledInFrame = false;
    public GameObject outdoorLights;
    public AudioSource musicSource;
    public ShipAudioManager shipAudioManager;
    
    // CONTROLS
    enum State
    {
        Idle, ControlledByPlayer
    }

    private State _state = State.Idle;
    
    void Start()
    {
        //(ControlShip());
    }

    private void FixedUpdate()
    {
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    public void TryToPlayerControlsShip()
    {
        playerMovement = PlayerMovement.instance;

        if (_state == State.ControlledByPlayer && playerMovement.inControl == false)
        {
            playerMovement.PlayerControlsShip(null);
            StopControllingShip();
            return;   
        }

        
        playerMovement.PlayerControlsShip(this);
        shipAudioManager.StartMovingSfx();

        rb.isKinematic = false;
        _state = State.ControlledByPlayer;
        StartCoroutine(MovePlayerToControlPosition());
    }

    public void StopControllingShip()
    {
        StopAllCoroutines(); 
        shipAudioManager.StopMovingSfx();
        _state = State.Idle;
    }
    
    IEnumerator MovePlayerToControlPosition()
    {
        playerPositionAtControl.position = playerMovement.transform.position;
        
        StartCoroutine(ControlShip());
        while (true)
        {
            yield return null;
            playerMovement.transform.position = playerPositionAtControl.position;
        }
    }

    IEnumerator ControlShip()
    {
        shipAudioManager.StartMovingSfx();
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
        if (Input.GetKey(KeyCode.Space))
        {
            TryToPlayerControlsShip();
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
            targetVelocity = Vector3.Lerp(targetVelocity, Vector3.zero, Time.deltaTime * dragScale);
        }
        
        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity * moveSpeedScaler, Time.deltaTime * accelerationScale);
        
        rb.velocity = currentVelocity;
    }

    public void TryToToggleLight()
    {
        outdoorLights.SetActive(!outdoorLights.activeInHierarchy);
    }

    public void TryToToggleMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
        else
        {
            musicSource.pitch = UnityEngine.Random.Range(0.75f, 1f);
            musicSource.Play();
        }
    }

    public void TryToUseGrabber(GrabberController grabber)
    {
        grabber.UseGrabberInput();
    }
    public void TryToUseHarpoon(HarpoonController harpoon)
    {
        harpoon.UseHarpoonInput(this);
    }
    public void TryToUseDoorLock(DoorLockController doorLock)
    {
        doorLock.UseDoorLockInput();
    }
}
