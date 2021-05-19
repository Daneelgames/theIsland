﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerControls
{
    public class PlayerMovement : MonoBehaviour
    {
        public static PlayerMovement instance;

        private float _dashCooldownCurrent = 0;

        public Transform playerHead;
        public Animator cameraAnimator;
        public Animator crosshairAnimator;
        //public CharacterController controller;
        
        public float _x = 0;
        public float _z = 0;

        [Header("DashAttack")] 
        public int dashDamage = 100;
        private float _dashTimeCurrent = 1;

        private float _currentCameraJiggle = 0;
        private float _cameraChangeSpeed = 2;

        private StaminaController _staminaController;
        
        public PlayerMovementStats movementStats;
        public StaminaStats staminaStats;
        
        [HideInInspector]
        public float currentCrosshairJiggle = 0;

        private float _crosshairChangeSpeed = 0.1f;
        public float mechSpeed = 5;
        private Vector3 _move;

        private Vector3 _velocity;
        [SerializeField] private float gravity = -3f;
        public Transform groundCheck;
        public bool fallingInHole = false;
        public float groundDistance = 0.4f;
        public LayerMask groundMask;
        //private float characterControllerInitRaduis = 0;
    
        public MouseLook mouseLook;

        public bool _grounded = false;

        public CapsuleCollider damagecollider;
        private bool dangerousDash = false;
        public bool crouching = false;
        
        public Transform movementTransform;
        public PlayerAudioController playerAudio;
        public Collider doorCollider;

        public bool teleport = false; 

        public Transform portableTransform;

        public Transform hookshotTarget;
        public float hookshotSpeed = 5;

        private string speedString = "Speed";
        private string dashString = "Jump";
        private string runningString = "Run";
        private string runningStringWeapon = "Running";
        private string jiggleString = "Jiggle";
        private string horizontalString = "Horizontal";
        private string verticalString = "Vertical";

        bool ableToChooseRespawn = false;
        bool adrenaline = false;
        public float coldScaler = 1;
        private Transform parent = null;
        public Rigidbody rb;

        public float weightSpeedScaler = 1;

        private float currentGravityScaler = 1;

        private float shootedCooldown = 0;
        private float shootedCooldownMax = 0.1f;
        
        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            movementStats.currentMoveSpeed = 0;
            _dashTimeCurrent = movementStats.dashTime;
            //characterControllerInitRaduis = controller.radius;
            
            //dashCollider.enabled = false;
            
            playerHead.parent = null;
            movementStats.movementState = MovementState.Idle;
        }

        private void Update()
        {
            if (!teleport)
            {
                if (movementStats.movementState != MovementState.Hookshot && movementStats.movementState != MovementState.Shooted)
                {
                    Movement();
                    if (!_grounded && movementStats.movementState != MovementState.Dashing)
                        currentGravityScaler += 3 * Time.deltaTime;
                }
            }

            // smooth camera movement when character controller is stepping up
            if (!teleport)
            {
                playerHead.position = Vector3.Lerp(playerHead.position, transform.position, 50 * Time.deltaTime);
            }

            if (!teleport)
                Gravity();
        }

        public void Dash()
        {
            if (_dashCooldownCurrent > 0 || !_grounded) return; 

            movementStats.movementState = MovementState.Dashing;
        
            var right = movementTransform.right;
            var forward = movementTransform.forward;
        
            _move = Mathf.Approximately(_z, 0) && Mathf.Approximately(_x, 0)
                ? right * 0 + forward * -1 + Vector3.up * movementStats.jumpPower
                : right * _x + forward * _z + Vector3.up * movementStats.jumpPower;

            _dashCooldownCurrent = movementStats.dashCooldown;
            
            cameraAnimator.SetTrigger(dashString);
            _dashTimeCurrent = 0;
            
            if (staminaStats.CurrentValue >= staminaStats.dashCostCurrent)
            {
                movementStats.movementState = MovementState.Dashing;
                playerAudio.PlayDash();
            }
            
            _grounded = false;
            rb.AddForce(Vector3.up * movementStats.jumpPower, ForceMode.Impulse);
        }
        
        private void Movement()
        {
            _x = Input.GetAxisRaw(horizontalString);
            _z = Input.GetAxisRaw(verticalString);   

            if (parent != null)
            {
                if (Mathf.Approximately(_x ,0) && Mathf.Approximately(_z ,0))
                    rb.isKinematic = true;
                else
                    rb.isKinematic = false;
            }
            
            movementTransform.eulerAngles = new Vector3(0, movementTransform.eulerAngles.y, 0);

            if ((Input.GetButtonDown(dashString)))
            {
                Dash();
            }

            if (movementStats.movementState != MovementState.Dashing)
                movementStats.movementState = MovementState.Walking;


            if (_dashTimeCurrent < movementStats.dashTime)
            {
                if (_dashTimeCurrent < movementStats.dashTime / 2)
                {
                    _move = new Vector3(_move.x, movementStats.jumpPower, _move.z);
                }
                _dashTimeCurrent += Time.deltaTime;
            }
            else if (_dashTimeCurrent >= movementStats.dashTime &&
                movementStats.movementState == MovementState.Dashing) // DASH END
            {
                movementStats.movementState = MovementState.Idle;
                currentGravityScaler = 0;
                dangerousDash = false;
            }

            _dashCooldownCurrent -= Time.deltaTime;
            if (_dashCooldownCurrent < 0) 
                _dashCooldownCurrent = 0;
            
            // IF DASHING
            if (movementStats.movementState == MovementState.Dashing)
            {
                movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, movementStats.dashSpeed, Time.deltaTime * 0.2f);
                rb.MovePosition(rb.position + _move.normalized * movementStats.currentMoveSpeed + Vector3.up * movementStats.jumpPower);
            }
            else
            {
                // MOVEMENT DIRECTION
                ////////////////////

                if (_grounded)
                    _move = mouseLook.transform.right * _x + mouseLook.transform.forward * _z;
                else
                    _move = mouseLook.transform.right * _x + movementTransform.forward * _z;
                
                movementStats.movementState = _move.magnitude < 0.3f ? MovementState.Idle : MovementState.Walking;
                
                if (!_grounded) _move /= 100; // FALLING

                if (_z > 0) // MOVING FORWARD
                {
                    rb.MovePosition(rb.position + _move.normalized * movementStats.currentMoveSpeed);
                }
                else if (Mathf.Approximately(_z, 0) && !Mathf.Approximately(_x, 0)) // STRAIFING
                {
                    rb.MovePosition(rb.position + _move.normalized * movementStats.currentMoveSpeed);
                }
                else if (_z < 0) // walking && strafing backwards
                {
                    rb.MovePosition(rb.position + _move.normalized * movementStats.currentMoveSpeed);

                }
                else if (Mathf.Approximately(_z, 0) && Mathf.Approximately(_x, 0)) // to idle
                {
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 0, Time.deltaTime);
                    rb.velocity = Vector3.zero;
                }

                // RUNNING
                //////////
                if (_grounded && Input.GetAxis(runningString) > 0 && !mouseLook.aiming)
                {
                    movementStats.isRunning = true;
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, movementStats.baseMoveSpeed + movementStats.runSpeedBonusCurrent, Time.deltaTime * 0.1f);
                }
                else
                {
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, movementStats.baseMoveSpeed, Time.deltaTime * 0.1f);
                }
            }


            if (_move.magnitude > 0 && _grounded) 
                playerAudio.PlaySteps();
            
            // HEARTBEAT
            /////////////
            if (movementStats.isRunning || movementStats.movementState == MovementState.Dashing)
            {
                playerAudio.heartbeatSource.volume = Mathf.Lerp(playerAudio.heartbeatSource.volume, 1, Time.deltaTime * 0.1f);
            }
            else
            {
                playerAudio.heartbeatSource.volume = Mathf.Lerp(playerAudio.heartbeatSource.volume, 0, Time.deltaTime * 0.1f);
            }
            
            // VISUAL FEEDBACK
            //////////////////
            
            _currentCameraJiggle = ControlJiggle(_currentCameraJiggle, _cameraChangeSpeed);
            currentCrosshairJiggle = ControlJiggle(currentCrosshairJiggle, _crosshairChangeSpeed);
            cameraAnimator.SetFloat(speedString, _currentCameraJiggle);
            cameraAnimator.speed = 1; //todo: check ???
            crosshairAnimator.SetFloat(jiggleString, currentCrosshairJiggle);
        }

        private void Gravity()
        {
            if (movementStats.movementState != MovementState.Hookshot && movementStats.movementState != MovementState.Shooted)
            {
                if (_dashTimeCurrent < movementStats.dashTime || movementStats.movementState == MovementState.Dashing)
                    return;
                
                _grounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
                
                _velocity = Vector3.zero;

                if (!_grounded)
                {
                    _velocity.y += gravity * 7.5f * currentGravityScaler;
                    rb.MovePosition(rb.position + _velocity.normalized * movementStats.currentMoveSpeed);   
                }
            }
            else
            {
                _grounded = false;
            }
            
            if (_grounded) 
                currentGravityScaler = 1;
            else
                currentGravityScaler = 0;
        }

        private float ControlJiggle(float jiggle, float changeSpeed)
        {
            if (_move.sqrMagnitude <= 0.01f)
            {
                if (jiggle > 0.01f)
                    jiggle = Mathf.Lerp(jiggle, 0, changeSpeed * 0.5f * Time.deltaTime);
                else
                    jiggle = 0;
            }
            else
            {
                if (movementStats.isRunning)
                {
                    if (jiggle < 1)
                        jiggle = Mathf.Lerp(jiggle, 1, changeSpeed * 2 * Time.deltaTime);
                }
                else if (movementStats.movementState == MovementState.Dashing)
                {
                    if (jiggle < 1)
                        jiggle = Mathf.Lerp(jiggle, 1, changeSpeed * 3 * Time.deltaTime);
                }
                else
                    jiggle = Mathf.Lerp(jiggle, 0.5f, changeSpeed * Time.deltaTime);
            }

            return jiggle;
        }

        public void Teleport(bool active)
        {
            teleport = active;
        }

        public void EnterParent(Transform p)
        {
            parent = p;
            transform.parent = p;
        }

        public void ExitParent(Transform p)
        {
            if (parent && parent == p)
            {
                parent = null;
                transform.parent = null;
            }
        }
    }
}
