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

        if (magnitude < .25f)
            engineAmbientSource0.volume = Mathf.Lerp(engineAmbientSource0.volume, 1, Time.deltaTime);
    }
}