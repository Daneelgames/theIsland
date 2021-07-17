using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeSoundSourcesPitch : MonoBehaviour
{
    public List<AudioSource> sources = new List<AudioSource>();
    public float pitchMin = 0.75f;
    public float pitchMax = 1.25f;
    void Start()
    {
        for (int i = 0; i < sources.Count; i++)
        {
            sources[i].pitch = Random.Range(pitchMin, pitchMax);
        }
    }
}
