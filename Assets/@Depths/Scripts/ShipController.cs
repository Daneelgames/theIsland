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
    public Transform playerSit;
    public float moveSpeedScaler = 1;
    public float dragScale = 0.5f;
    public float accelerationScale = 1;
    public float torqueSpeedScaler = 0.33f;
    public Rigidbody rb;
    private Vector3 currentVelocity;
    private Vector3 _targetVelocity;
    private bool controlledInFrame = false;
    public GameObject outdoorLights;
    public AudioSource musicSource;
    public ShipAudioManager shipAudioManager;
    public Transform playerHeadTransform;
    
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

    public Vector3 TargetVelocity
    {
        get { return _targetVelocity; }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && (_state == State.Idle|| (_state == State.ControlledByPlayer && MouseLook.instance.canControl)))
        {
            TryToPlayerControlsShip();
        }
    }
    
    /*
    private void FixedUpdate()
    {
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }
    */

    public void TryToPlayerControlsShip()
    {
        playerMovement = PlayerMovement.instance;

        if (_state == State.ControlledByPlayer /*&& playerMovement.inControl == false*/)
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
        playerPositionAtControl.position = playerSit.position;
        
        StartCoroutine(ControlShip());
        while (true)
        {
            yield return null;
            playerMovement.transform.position = playerSit.position;
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
        
        _targetVelocity = Vector3.zero;
        controlledInFrame = false;
        if (Input.GetKey(KeyCode.W))
        {
            controlledInFrame = true;    
            _targetVelocity += transform.forward;   
        }

        if (Input.GetKey(KeyCode.D))
        {
            controlledInFrame = true;
            _targetVelocity += transform.right;   
        }

        if (Input.GetKey(KeyCode.S))
        {
            controlledInFrame = true;
            _targetVelocity += -transform.forward;   
        }

        if (Input.GetKey(KeyCode.A))
        {
            controlledInFrame = true;
            _targetVelocity += -transform.right;   
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            controlledInFrame = true;
            _targetVelocity += transform.up;   
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            controlledInFrame = true;
            _targetVelocity += -transform.up;   
        }
        
        if (Input.GetKey(KeyCode.E))
        {
            controlledInFrame = true;
            rb.AddRelativeTorque(transform.forward * (-torqueSpeedScaler * Time.deltaTime), ForceMode.Force);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            controlledInFrame = true;
            rb.AddRelativeTorque(transform.forward * (torqueSpeedScaler * Time.deltaTime), ForceMode.Force);
        }
        
        _targetVelocity.Normalize();

        if (controlledInFrame == false)
        {
            _targetVelocity = Vector3.Lerp(_targetVelocity, Vector3.zero, Time.deltaTime * dragScale);
        }
        
        currentVelocity = Vector3.Lerp(currentVelocity, _targetVelocity * moveSpeedScaler, Time.deltaTime * accelerationScale);
        
        rb.velocity = currentVelocity;
    }

    public void AddTorqueFromPlayerHead(float mouseX, float mouseY)
    {
        if (Math.Abs(mouseX) < 0.01f && Math.Abs(mouseY) < 0.01f)
        {
            return;
        }

        controlledInFrame = true;
        rb.AddRelativeTorque(transform.right * (-mouseY * (torqueSpeedScaler * Time.deltaTime)), ForceMode.Force);
        rb.AddRelativeTorque(transform.up * (mouseX * (torqueSpeedScaler * Time.deltaTime)), ForceMode.Force);
        //rb.AddRelativeTorque(new Vector3(-mouseY, 0, mouseX) * (torqueSpeedScaler * Time.deltaTime), ForceMode.Force);
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
