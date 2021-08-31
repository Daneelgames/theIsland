using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipAudioManager : MonoBehaviour
{
    public AudioSource shipStartMovingSfxSource;
    public AudioSource shipEndMovingSfxSource;
    public AudioSource shipMovingSfxSource;

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
}
