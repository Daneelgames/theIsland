using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    public HealthController hc;
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
        Idle, ControlledByPlayer
    }

    public State _state = State.Idle;
    
    void Start()
    {
        TryToPlayerControlsShip();
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

        if (Use360Movement && _state == State.ControlledByPlayer)
        {
            GetPlayerInput360();
        }
    }

    public float CurrentSpeed
    {
        get { return Mathf.RoundToInt(trueSpeed); }
    }

    void GetPlayerInput360()
    {
        if (!MouseLook.instance.aiming)
        {
            pitch = Input.GetAxis("Mouse Y");
            yaw = Input.GetAxis("Mouse X");  
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
            power = Input.GetAxis("Mouse ScrollWheel") * 3000 * Time.deltaTime;
        }

        strafe = new Vector3(Input.GetAxis("Horizontal") * strafeSpeed * Time.deltaTime, Input.GetAxis("Vertical") * strafeSpeed * Time.deltaTime, 0);
        
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
        
        rb.AddRelativeTorque(-pitch * turnspeed * verticalTurnSpeedScaler * Time.deltaTime, yaw * turnspeed * Time.deltaTime, -roll * turnspeed * Time.deltaTime);
        
        rb.AddRelativeForce(0,0,trueSpeed * speed * Time.deltaTime);
        rb.AddRelativeForce(strafe);
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
            playerMovement.PlayerControlsShip(null);
            StopControllingShip();
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
        playerPositionAtControl.position = playerSit.position;
        
        if (Use360Movement == false)
            StartCoroutine(ControlShip2Axis());
        
        shipAudioManager.StartMovingSfx();
        
        while (true)
        {
            yield return null;
            playerMovement.transform.position = playerSit.position;
        }
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
