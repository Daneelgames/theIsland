using System;
using UnityEngine;

namespace PlayerControls
{
    public class StaminaController : MonoBehaviour
    {
        public StaminaStats staminaStats;
        public PlayerMovementStats movementStats;
        
        private void Update()
        {
            float staminaRegen;
            if (PlayerMovement.instance._climbing && !PlayerMovement.instance._grounded)
            {
                staminaRegen = staminaStats.climbingRegen;
            }
            else if (!PlayerMovement.instance._climbing && !PlayerMovement.instance._grounded)
            {
                
                staminaRegen = 0;
            }
            else
            {
                switch (movementStats.movementState)
                {
                    case MovementState.Idle:
                        staminaRegen = staminaStats.idleRegen;
                        break;
                    case MovementState.Walking:
                        staminaRegen = movementStats.isRunning
                            ? staminaStats.runRegenCurrent 
                            : staminaStats.walkRegen;
                        break;
                    default:
                        staminaRegen = staminaStats.walkRegen;
                        break;
                }   
            }
            
            staminaStats.CurrentValue += staminaRegen * Time.deltaTime;
        }
    }
}
