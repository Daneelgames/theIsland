using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantVisualController : MonoBehaviour
{
    public float minPulseTime = 0.5f;
    public float maxPulseTime = 1f;
    public float minScaleMultiplayer = 0.5f;
    public float maxScaleMultiplayer = 2f;
    public List<PlantsVisual> visualsByLifetime;
}