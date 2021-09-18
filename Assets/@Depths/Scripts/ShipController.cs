using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public bool playerShip = false;
    
    public HealthController hc;
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
    public ShipAudioManager shipAudioManager;
    public SetTargetToAi setTargetToAi;
    public LandingObject chassis;
    public Transform playerHeadTransform;
    public RadarObjectListController radar;

    [Header("Weapons")] 
    public List<HarpoonController> weaponsControlledByMainControl = new List<HarpoonController>();
    
    [Header("360 Movement Control")]
    public bool Use360Movement = true;
    public float turnspeed = 5.0f;
    public float verticalTurnSpeedScaler = 2f;
    public float  speed = 5.0f;
    public float  trueSpeed = 0.0f;
    public float maxTrueSpeed = 10f;
    public float minTrueSpeed = -3f;
    public float  strafeSpeed = 5.0f;

    private float roll;
    private float pitch;
    private float yaw;
    private Vector3 strafe;
    private float power;
    // CONTROLS
    public enum State
    {
        Idle, ControlledByPlayer, Docked
    }

    public State _state = State.Idle;
        
    void Start()
    {
        /*
        if (playerShip)
            TryToPlayerControlsShip();*/
    }

    public Vector3 TargetVelocity
    {
        get { return _targetVelocity; }
    }

    void Update()
    {
        if (!playerShip)
            return;
        
        if (Input.GetKeyDown(KeyCode.Space) && (_state == State.Idle || _state == State.Docked || (_state == State.ControlledByPlayer && MouseLook.instance.canControl)))
        {
            TryToPlayerControlsShip();
        }

        if (Use360Movement && _state == State.ControlledByPlayer)
        {
            GetPlayerInput360();
        }
        
        if (playerMovement)
            playerMovement.transform.position = playerSit.position;
    }

    public float CurrentSpeed
    {
        get { return Mathf.RoundToInt(trueSpeed); }
    }

    private float normalizedCursorY = 0;
    private float normalizedCursorX = 0;
    void GetPlayerInput360()
    {
        if (!MouseLook.instance.aiming)
        {
            /*
            pitch = Mathf.Clamp(Input.GetAxis("Mouse Y"), -1, 1);
            yaw = Mathf.Clamp(Input.GetAxis("Mouse X"), -1, 1);
            */
            normalizedCursorY = MouseLook.instance.playerCursor.transform.localPosition.y / Screen.height;
            normalizedCursorX = MouseLook.instance.playerCursor.transform.localPosition.x / Screen.width;
            
            if (Mathf.Abs(normalizedCursorY) > 0.05f)
                pitch = Mathf.Clamp(normalizedCursorY, -1, 1);
            else
                pitch = 0;
                
            if (Mathf.Abs(normalizedCursorX) > 0.05f)
                yaw = Mathf.Clamp(normalizedCursorX, -1, 1);
            else
                yaw = 0;
        }
        else
        {
            pitch = 0;
            yaw = 0;
        }
        
        roll = Input.GetAxis("Roll");
        
        power = Input.GetAxis("Power");

        if (Mathf.Approximately(Input.GetAxis("Mouse ScrollWheel"), 0) == false)
        {
            power = Input.GetAxis("Mouse ScrollWheel") * 2000 * Time.deltaTime;
        }

        strafe = new Vector3(Input.GetAxis("Horizontal") * strafeSpeed, Input.GetAxis("Vertical") * strafeSpeed, 0);
        
        //Truespeed controls

        if (trueSpeed < maxTrueSpeed && trueSpeed > minTrueSpeed)
        {
            trueSpeed += power * 50 * Time.deltaTime;
        }
        if (trueSpeed > maxTrueSpeed)
        {
            trueSpeed = maxTrueSpeed - 0.01f;	
        }
        if (trueSpeed < minTrueSpeed)
        {
            trueSpeed = minTrueSpeed + 0.01f;	
        }
        
        if (Input.GetKeyDown(KeyCode.F))
            trueSpeed = 0;
        
        rb.AddRelativeTorque(-pitch * turnspeed * verticalTurnSpeedScaler * Time.deltaTime, yaw * turnspeed * Time.deltaTime, -roll * turnspeed * Time.deltaTime, ForceMode.Force);
        
        rb.AddRelativeForce(0,0,trueSpeed * speed * Time.deltaTime, ForceMode.Force);
        rb.AddRelativeForce(strafe * Time.deltaTime, ForceMode.Force);
    }
    
    private void FixedUpdate()
    {
        if (Use360Movement)
            return;
        
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
    }

    public void TryToPlayerControlsShip()
    {
        playerMovement = PlayerMovement.instance;

        if (_state == State.ControlledByPlayer /*&& playerMovement.inControl == false*/)
        {
            trueSpeed = 0;
            playerMovement.PlayerControlsShip(null);
            StopControllingShip();
            for (int i = 0; i < weaponsControlledByMainControl.Count; i++)
            {
                weaponsControlledByMainControl[i].UseHarpoonInput(null);
            }
            return;   
        }
        
        playerMovement.PlayerControlsShip(this);
        shipAudioManager.StartMovingSfx();

        for (int i = 0; i < weaponsControlledByMainControl.Count; i++)
        {
            weaponsControlledByMainControl[i].UseHarpoonInput(this);
        }

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
        if (Use360Movement == false)
            StartCoroutine(ControlShip2Axis());
        
        shipAudioManager.StartMovingSfx();
        
        /*
        while (true)
        {
            yield return null;
            playerMovement.transform.position = playerSit.position;
        }*/
        yield return null;
    }

    IEnumerator ControlShip2Axis()
    {
        shipAudioManager.StartMovingSfx();
        while (true)
        {
            GetShipMovement2Axis();
            yield return null;
        }
    }

    void GetShipMovement2Axis()
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
        if (Use360Movement || (Math.Abs(mouseX) < 0.01f && Math.Abs(mouseY) < 0.01f))
        {
            return;
        }

        mouseX = Mathf.Clamp(mouseX, -1, 1);
        mouseY = Mathf.Clamp(mouseY, -1, 1);

        controlledInFrame = true;
        rb.AddRelativeTorque(transform.right * (-mouseY * (torqueSpeedScaler * Time.deltaTime)), ForceMode.Force);
        rb.AddRelativeTorque(transform.up * (mouseX * (torqueSpeedScaler * Time.deltaTime)), ForceMode.Force);
        //rb.AddRelativeTorque(new Vector3(-mouseY, 0, mouseX) * (torqueSpeedScaler * Time.deltaTime), ForceMode.Force);
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

    public void Dock()
    {
        _state = State.Docked;
    }
}
