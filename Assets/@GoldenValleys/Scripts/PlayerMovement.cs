﻿using System.Collections;
using UnityEngine;

namespace PlayerControls
{
    public class PlayerMovement : MonoBehaviour
    {
        public bool playerInstance = false;
        public static PlayerMovement instance;

        public bool inControl = true;
        public bool canClimb = true;
        public bool canSprint = true;
        public bool canJump = true;
        
        private float _dashCooldownCurrent = 0;

        public float playerHeight = 0.5f;
        public Animator cameraAnimator;
        public Animator crosshairAnimator;
        
        public float _x = 0;
        public float _z = 0;

        public float velocityChangeSpeed = 10;
        public Vector3 targetVelocity;
        
        private float _dashTimeCurrent = 1;
        private float _currentCameraJiggle = 0;
        private float _cameraChangeSpeed = 2;

        private StaminaController _staminaController;
        
        public PlayerMovementStats movementStats;
        public StaminaStats staminaStats;
        
        [HideInInspector]
        public float currentCrosshairJiggle = 0;

        private float _crosshairChangeSpeed = 0.1f;
        private Vector3 _move;

        private Vector3 _velocity;
        [SerializeField] private float gravity = -3f;
        private float gravityCurrent = -10;
        
        float coyoteTimeCur = 0;
        float coyoteTimeMax = 0.5f;
        
        public float groundSphereRadius = 0.4f;
        public float climbSphereRadius = 0.4f;
        public LayerMask groundMask;
        //private float characterControllerInitRaduis = 0;
    
        public MouseLook mouseLook;

        public bool _grounded = false;
        public bool _climbing = false;

        public CapsuleCollider damagecollider;
        private bool dangerousDash = false;
        public bool crouching = false;
        
        public Transform movementTransform;
        public PlayerAudioController playerAudio;

        public bool teleport = false; 

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
        public CharacterController controller;

        private float currentGravityScaler = 1;

        private float shootedCooldown = 0;
        private float shootedCooldownMax = 0.1f;

        public ShipController shipInControl;

        private void Awake()
        {
            if (playerInstance)
            {
                if (instance != null)
                {
                    Destroy(gameObject);
                    return;
                }
                
                instance = this;   
            }
        }

        private void Start()
        {
            movementStats.currentMoveSpeed = 0;
            _dashTimeCurrent = movementStats.dashTime;
            
            movementStats.movementState = MovementState.Idle;
        }

        void Update()
        {
            if (!inControl)
                return;
            
            if (canJump && playerInstance && Input.GetButtonDown(dashString))
            {
                Dash();
            }
            
            if (!_grounded && movementStats.movementState != MovementState.Dashing)
                currentGravityScaler += 3 * Time.deltaTime;

            if (!_grounded && !_climbing)
            {
                if (movementStats.movementState != MovementState.Dashing)
                    gravityCurrent = Mathf.Lerp(gravityCurrent, gravity, Time.deltaTime);
                    
                if (coyoteTimeCur < coyoteTimeMax)
                    coyoteTimeCur += Time.deltaTime;
            }
            else
            {
                gravityCurrent = Mathf.Lerp(gravityCurrent, gravity / 100, Time.deltaTime);
            }
        }

        public void PlayerControlsShip(ShipController ship)
        {
            if (ship == null)
            {
                MouseLook.instance.PlayerControlsShip(null);
                return;
            }
            
            if (rb)
                rb.isKinematic = true;
            else
                controller.enabled = false;
            MouseLook.instance.PlayerControlsShip(ship);
            shipInControl = ship;
            transform.parent = ship.transform;
            inControl = false;
        }
        
        public void PlayerControlsHarpoon(HarpoonController harpoon)
        {
            if (shipInControl.Use360Movement)
                return;
            
            //PlayerControlsShip(null);
            if (harpoon == null)
            {/*
                if (rb)
                    rb.isKinematic = false;
                else
                    controller.enabled = true;
                    
                inControl = true;
                */
                MouseLook.instance.PlayerControlsHarpoon(null);
                return;
            }
            
            if (rb)
                rb.isKinematic = true;
            else
                controller.enabled = false;
            
            MouseLook.instance.PlayerControlsHarpoon(harpoon);
            transform.parent = harpoon.transform;
            inControl = false;
        }
        
        private void FixedUpdate()
        {
            if (teleport || !inControl)
            {
                return;
            }
            
            if (playerInstance)
                GetPlayerMovementInput();
            
            CalculateMovement();
            Gravity();
            if (canClimb)
                Climbing();

            ApplyVelocity();
        }

        void ApplyVelocity()
        {
            //controller.velocity = Vector3.Lerp(controller.velocity, targetVelocity, Time.smoothDeltaTime * velocityChangeSpeed);
            if (rb)
                rb.AddForce(targetVelocity * (Time.smoothDeltaTime * velocityChangeSpeed));
            else
                controller.Move(targetVelocity * (Time.smoothDeltaTime * velocityChangeSpeed));
        }

        public void TeleportPlayerHead()
        {
            MouseLook.instance.TeleportPlayerHead();
        }

        public void Dash()
        {
            if (_dashCooldownCurrent > 0 || coyoteTimeCur >= coyoteTimeMax) return; 

            movementStats.movementState = MovementState.Dashing;
        
            var right = movementTransform.right;
            var forward = movementTransform.forward;
        
            _move = Mathf.Approximately(_z, 0) && Mathf.Approximately(_x, 0)
                ? right * 0 + forward * -1 + Vector3.up * movementStats.jumpPower
                : right * _x + forward * _z + Vector3.up * movementStats.jumpPower;

            _dashCooldownCurrent = movementStats.dashCooldown;
            
            if (playerInstance)
                cameraAnimator.SetTrigger(dashString);
            
            _dashTimeCurrent = 0;
            
            if (playerInstance && staminaStats.CurrentValue >= staminaStats.dashCostCurrent)
            {
                movementStats.movementState = MovementState.Dashing;
                playerAudio.PlayDash();
            }
            
            _grounded = false;
            StartCoroutine(DashCoroutine());
        }

        IEnumerator DashCoroutine()
        {
            float jumpPowerCur = movementStats.jumpPower;
            
            Transform forwardTransform = movementTransform;
            if (playerInstance) forwardTransform = mouseLook.transform;
            
            while (_dashTimeCurrent < movementStats.dashTime)
            {
                targetVelocity = (forwardTransform.forward + Vector3.up / 2).normalized * jumpPowerCur;
                //jumpPowerCur = Mathf.Lerp(movementStats.jumpPower, 0, (_dashTimeCurrent / movementStats.dashTime) * 1.5f);
                jumpPowerCur = Mathf.Lerp(movementStats.jumpPower * 2, movementStats.jumpPower, (_dashTimeCurrent / movementStats.dashTime));
                yield return null;
            }
        }

        void GetPlayerMovementInput()
        {
            _x = Input.GetAxisRaw(horizontalString);
            _z = Input.GetAxisRaw(verticalString);   
        }
        
        public void GetNpcMovementInput(float x, float z)
        {
            _x = x;
            _z = z;   
        }
        
        private void CalculateMovement()
        {
            movementTransform.eulerAngles = new Vector3(0, movementTransform.eulerAngles.y, 0);

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
                targetVelocity = _move.normalized * movementStats.currentMoveSpeed + Vector3.up * movementStats.jumpPower;
            }
            else
            {
                // MOVEMENT DIRECTION
                ////////////////////

                if (!playerInstance)
                    _move = movementTransform.forward * _x + movementTransform.forward * _z;
                else if (_climbing)
                    _move = mouseLook.transform.right * _x + mouseLook.transform.forward * _z;
                else
                    _move = mouseLook.transform.right * _x + movementTransform.forward * _z;
                
                movementStats.movementState = _move.magnitude < 0.3f ? MovementState.Idle : MovementState.Walking;
                
                if (!_grounded) _move /= 2; // FALLING

                if (_z > 0) // MOVING FORWARD
                {
                    targetVelocity = _move.normalized * movementStats.currentMoveSpeed;
                }
                else if (Mathf.Approximately(_z, 0) && !Mathf.Approximately(_x, 0)) // STRAIFING
                {
                    targetVelocity = _move.normalized * movementStats.currentMoveSpeed;
                }
                else if (_z < 0) // walking && strafing backwards
                {
                    targetVelocity = _move.normalized * movementStats.currentMoveSpeed;
                }
                else if (Mathf.Approximately(_z, 0) && Mathf.Approximately(_x, 0)) // to idle
                {
                    movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, 0, Time.deltaTime);
                    targetVelocity = Vector3.zero;
                }

                // RUNNING
                //////////
                if (playerInstance)
                {
                    if (canSprint && _grounded && Input.GetAxis(runningString) > 0 && !mouseLook.aiming)
                    {
                        movementStats.isRunning = true;
                        movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, movementStats.baseMoveSpeed + movementStats.runSpeedBonusCurrent, Time.smoothDeltaTime);
                    }
                    else
                    {
                        movementStats.isRunning = false;
                        movementStats.currentMoveSpeed = Mathf.Lerp(movementStats.currentMoveSpeed, movementStats.baseMoveSpeed, Time.smoothDeltaTime);
                    }   
                }
                else movementStats.currentMoveSpeed = movementStats.baseMoveSpeed;
            }

            if (playerInstance)
            {
                if (_move.magnitude > 0 && _grounded) 
                    playerAudio.PlaySteps();
            
                _currentCameraJiggle = ControlJiggle(_currentCameraJiggle, _cameraChangeSpeed);
                currentCrosshairJiggle = ControlJiggle(currentCrosshairJiggle, _crosshairChangeSpeed);
                cameraAnimator.SetFloat(speedString, _currentCameraJiggle);
                cameraAnimator.speed = 1; //todo: check ???
                crosshairAnimator.SetFloat(jiggleString, currentCrosshairJiggle);   
            }
        }

        private RaycastHit[] hitInfoGround;
        
        private void Gravity()
        {
            if (_dashTimeCurrent < movementStats.dashTime || movementStats.movementState == MovementState.Dashing)
            {
                _grounded = false;
                return;
            }

            hitInfoGround = Physics.SphereCastAll(transform.position, groundSphereRadius,Vector3.down,0.1f ,groundMask);
                    
            _grounded = hitInfoGround.Length > 0;
                
            _velocity = targetVelocity;

            if (!_grounded && !_climbing /* && coyoteTimeCur >= coyoteTimeMax*/)
            {
                _velocity.y = gravityCurrent * 7.5f * currentGravityScaler;
            }
            else if (_grounded && !_climbing)
                _velocity.y = gravity / 100;
            
            targetVelocity = _velocity;

            if (_grounded || _climbing)
            {
                currentGravityScaler = 1;
                coyoteTimeCur = 0;
            }
            else
            {
                currentGravityScaler = 0;
            }
        }
        
        private RaycastHit[] hitInfoClimb;
        private void Climbing()
        {
            if (playerInstance && staminaStats.CurrentValue <= 0)
            {
                _climbing = false;
                return;
            }
            
            hitInfoClimb = Physics.SphereCastAll(transform.position + Vector3.up * playerHeight, climbSphereRadius,Vector3.up,climbSphereRadius,groundMask);
                    
            _climbing = hitInfoClimb.Length > 0;
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

        public IEnumerator MovePlayerWithoutControl(Vector3 targetPos, float time)
        {
            float t = 0;
            inControl = false;
            if (rb)
                rb.isKinematic = true;
            else
                controller.enabled = false;
            Vector3 startPos = transform.position;
            while (t < time)
            {
                PlayerAudioController.instance.PlaySteps();
                transform.position = Vector3.Lerp(startPos, targetPos, t / time);
                t += Time.deltaTime;
                yield return null;
            }
            if (rb)
                rb.isKinematic = false;
            else
                controller.enabled = true;
            inControl = true;
        }

        public void ControlledShipDamaged()
        {
            cameraAnimator.SetTrigger("Damage");
        }
    }
}
