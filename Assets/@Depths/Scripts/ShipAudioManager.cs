using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAudioManager : MonoBehaviour
{
    public AudioSource shipStartMovingSfxSource;
    public AudioSource shipEndMovingSfxSource;
    public AudioSource shipMovingSfxSource;

    [Header("EngineAMbient")]
    public AudioSource engineAmbientSource0;
    public AudioSource engineAmbientSource1;
    public AudioSource engineAmbientSource2;
    
    private float previousEngineTrueSpeed = 0; 
    
    public void StartMovingSfx()
    {
        shipStartMovingSfxSource.pitch = Random.Range(0.75f, 1.25f);
        shipStartMovingSfxSource.Play();
        shipMovingSfxSource.pitch = Random.Range(0.75f, 1.25f);
        shipMovingSfxSource.Play();
    }
    public void StopMovingSfx()
    {
        shipEndMovingSfxSource.pitch = Random.Range(0.75f, 1.25f);
        shipEndMovingSfxSource.Play();
        shipMovingSfxSource.Stop();
    }

    public void SetShipsEngineTrueSpeed(float trueSpeed, float minTrueSpeed, float maxTrueSpeed)
    {
        if (trueSpeed < 0)
            trueSpeed *= -1;
        
        if (minTrueSpeed < 0)
            minTrueSpeed = 0;

        float magnitude = trueSpeed / maxTrueSpeed;
        
        if (trueSpeed != previousEngineTrueSpeed)
        {
            previousEngineTrueSpeed = trueSpeed;
        }

        if (trueSpeed > -1 && trueSpeed < 1) // IDLE
        {
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 0f, Time.deltaTime);
            engineAmbientSource1.volume = Mathf.Lerp(engineAmbientSource1.volume, 0f, Time.deltaTime);
            engineAmbientSource2.volume = Mathf.Lerp(engineAmbientSource2.volume, 0f, Time.deltaTime);
        }
        else if (magnitude < .2f)
        {
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 0.5f, Time.deltaTime);
            engineAmbientSource1.volume = Mathf.Lerp(engineAmbientSource1.volume, 0f, Time.deltaTime);
            engineAmbientSource2.volume = Mathf.Lerp(engineAmbientSource2.volume, 0f, Time.deltaTime);   
        }
        else if (magnitude < .4f)
        {
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 0.25f, Time.deltaTime);
            engineAmbientSource1.volume = Mathf.Lerp(engineAmbientSource1.volume, 0.25f, Time.deltaTime);
            engineAmbientSource2.volume = Mathf.Lerp(engineAmbientSource2.volume, 0f, Time.deltaTime);   
        }
        else if (magnitude < .6f)
        {
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 0f, Time.deltaTime);
            engineAmbientSource1.volume = Mathf.Lerp(engineAmbientSource1.volume, 0.5f, Time.deltaTime);
            engineAmbientSource2.volume = Mathf.Lerp(engineAmbientSource2.volume, 0f, Time.deltaTime);   
        }
        else if (magnitude < .8f)
        {
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 0f, Time.deltaTime);
            engineAmbientSource1.volume = Mathf.Lerp(engineAmbientSource1.volume, 0.25f, Time.deltaTime);
            engineAmbientSource2.volume = Mathf.Lerp(engineAmbientSource2.volume, 0.25f, Time.deltaTime);   
        }
        else
        {
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 0f, Time.deltaTime);
            engineAmbientSource1.volume = Mathf.Lerp(engineAmbientSource1.volume, 0f, Time.deltaTime);
            engineAmbientSource2.volume = Mathf.Lerp(engineAmbientSource2.volume, 0.5f, Time.deltaTime);   
        }
    }
}