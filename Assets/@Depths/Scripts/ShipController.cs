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
    public Rigidbody rb;
    private Vector3 currentVelocity;
    private bool controlledInFrame = false;
    public ShipAudioManager shipAudioManager;
    public LandingObject chassis;
    public Transform playerHeadTransform;
    public RadarObjectListController radar;

    [Header("AI")]
    public AstarWalker astarWalker;
    public SetTargetToAi setTargetToAi;
    
    [Header("Weapons")] 
    public List<RangedWeaponController> rangedWeapons = new List<RangedWeaponController>();
    public List<MeleeWeaponController> meleeWeapons = new List<MeleeWeaponController>();
    
    [Header("360 Movement Control")]
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

    void Update()
    {
        if (!playerShip)
        {
            if (astarWalker)
                GetControlsFromAstarWalker();
            
            return;   
        }
        
        if (Input.GetKeyDown(KeyCode.Space) && (_state == State.Idle || _state == State.Docked || (_state == State.ControlledByPlayer && MouseLook.instance.canControl)))
        {
            TryToPlayerControlsShip();
        }

        if (_state == State.ControlledByPlayer)
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

    #region PLAYER CONTROLS

    private float normalizedCursorY = 0;
    private float normalizedCursorX = 0;
    void GetPlayerInput360()
    {
        if (!MouseLook.instance.aiming)
        {
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
        
        shipAudioManager.SetShipsEngineTrueSpeed(trueSpeed, minTrueSpeed, maxTrueSpeed);
    }
    
    public void TryToPlayerControlsShip()
    {
        playerMovement = PlayerMovement.instance;

        if (_state == State.ControlledByPlayer /*&& playerMovement.inControl == false*/)
        {
            trueSpeed = 0;
            playerMovement.PlayerControlsShip(null);
            StopControllingShip();
            for (int i = 0; i < rangedWeapons.Count; i++)
            {
                rangedWeapons[i].UseWeaponInput(null);
            }
            for (int i = 0; i < meleeWeapons.Count; i++)
            {
                meleeWeapons[i].UseWeaponInput(null);
            }
            return;   
        }
        
        playerMovement.PlayerControlsShip(this);
        chassis.gameObject.SetActive(false);
        shipAudioManager.StartMovingSfx();

        
        for (int i = 0; i < rangedWeapons.Count; i++)
        {
            rangedWeapons[i].UseWeaponInput(this);
        }
        for (int i = 0; i < meleeWeapons.Count; i++)
        {
            meleeWeapons[i].UseWeaponInput(this);
        }

        rb.isKinematic = false;
        _state = State.ControlledByPlayer;
        MovePlayerToControlPosition();
    }

    #endregion

    private Vector3 currentAstarDirection = Vector3.zero;
    void GetControlsFromAstarWalker()
    {
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
    
    public void StopControllingShip()
    {
        StopAllCoroutines(); 
        shipAudioManager.StopMovingSfx();
        _state = State.Idle;
    }
    
    void MovePlayerToControlPosition()
    {
        shipAudioManager.StartMovingSfx();
    }

    public void TryToUseGrabber(GrabberController grabber)
    {
        grabber.UseGrabberInput();
    }
    public void TryToUseHarpoon(RangedWeaponController rangedWeapon)
    {
        rangedWeapon.UseWeaponInput(this);
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
