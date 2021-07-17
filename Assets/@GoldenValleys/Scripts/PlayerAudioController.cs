using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using PlayerControls;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[Serializable]
public class FootSteps
{
    public List<AudioClip> walk;
    public List<AudioClip> run;
}

public class PlayerAudioController : MonoBehaviour
{
    public static PlayerAudioController instance;
    public AudioSource selectNewUiActionSource;
    public List<AudioSource> footStepSources;
    public List<AudioClip> footStepWalk;
    public List<AudioClip> footStepRun;
    
    [Header("Greater value = more time between steps")]
    public float walkStepFrequencyCoefficient = 5;
    public float runStepFrequencyCoefficient = 8;
    public float dashStepFrequencyCoefficient = 1;
    public float smallStepCooldown = 0.5f;
    private float _timeFromPreviousSmallStep;
    
    [SerializeField] 
    private PlayerMovementStats playerMovementStats;
    
    int lastLeg = 0;
    int lastGunShot = 0;
    float currentStepTime = 0;
    
    /// <summary>
    /// If true character not moved when previous PlaySteps() was called.
    /// Used to find small steps.
    /// </summary>
    private bool _previousFrameWasIdle;

    
    PlayerMovement pm;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }

    void Start()
    {
        pm = PlayerMovement.instance;
    }
    
    private void Update()
    {
        _timeFromPreviousSmallStep += Time.deltaTime;
        if (playerMovementStats.movementState == MovementState.Idle)
            _previousFrameWasIdle = true;
    }

    
    public void PlayDash(bool withoutStamina = false)
    {
    }
    
    public void PlaySteps() // every frame
    {
        currentStepTime += Time.deltaTime;

        switch (playerMovementStats.movementState)
        {
            case MovementState.Idle:
                _previousFrameWasIdle = true;
                if (currentStepTime >= walkStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                {
                    PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.75f);
                }
                else
                    currentStepTime = 0;
                break;

            case MovementState.Walking:
                if (!playerMovementStats.isRunning) //Walk
                {
                    if (currentStepTime >= walkStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    {
                        if (!pm.crouching)
                        {
                            PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.75f);
                        }
                        else
                        {
                            PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.5f);
                        }
                    }
                    else if(_previousFrameWasIdle && _timeFromPreviousSmallStep > smallStepCooldown)
                    {
                        PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 0.5f);
                        _timeFromPreviousSmallStep = 0;
                    }
                    _previousFrameWasIdle = false;
                }
                else //Run
                {
                    if (currentStepTime >= runStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    {
                        PlayStep(footStepRun[Random.Range(0, footStepRun.Count - 1)], 1);
                    }
                    else if(_previousFrameWasIdle && _timeFromPreviousSmallStep > smallStepCooldown)
                    {
                        PlayStep(footStepWalk[Random.Range(0, footStepWalk.Count - 1)], 1);
                        _timeFromPreviousSmallStep = 0;
                    }
                    _previousFrameWasIdle = false;
                }
                break;
                
            case MovementState.Dashing:
                _previousFrameWasIdle = false;
                if (currentStepTime >= dashStepFrequencyCoefficient / playerMovementStats.currentMoveSpeed)
                    PlayStep(footStepRun[Random.Range(0, footStepRun.Count - 1)], 1);
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(playerMovementStats.movementState), "Unexpected enum value");
        }
    }

    void PlayStep(AudioClip step, float volume)
    {
        if (step == null)
            return;
        
        currentStepTime = 0;
        if (lastLeg == 0) lastLeg = 1;
        else lastLeg = 0;

        footStepSources[lastLeg].volume = volume;
        footStepSources[lastLeg].pitch = Random.Range(0.75f, 1.25f);
        footStepSources[lastLeg].clip = step;
        footStepSources[lastLeg].Play();
    }


    public void SelectNewUiAction()
    {
        selectNewUiActionSource.Stop();
        selectNewUiActionSource.pitch = Random.Range(0.75f, 1.25f);
        selectNewUiActionSource.Play();
    }
}