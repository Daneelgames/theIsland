using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantController : MonoBehaviour
{
    public int currentVisualIndex = -1;
    public List<PlantsVisual> visualsByLifetime;

    public float minPulseTime = 0.5f;
    public float maxPulseTime = 1f;
    public float minScaleMultiplayer = 0.5f;
    public float maxScaleMultiplayer = 2f;

    void Start()
    {
        foreach (var visual in visualsByLifetime)
        {
            visual.rootObject.SetActive(false);
        }
    }
    
    public void PlantSeed(int seedIndex)
    {
        for (int i = 0; i < visualsByLifetime.Count; i++)
        {
            visualsByLifetime[i].rootObject.SetActive(false);
        }

        currentVisualIndex = 0;
        visualsByLifetime[0].rootObject.SetActive(true);
        for (int i = 0; i < visualsByLifetime[0].pulsatingObjects.Count; i++)
        {
            StartCoroutine(AnimatePulsatingObject(visualsByLifetime[0].pulsatingObjects[i]));   
        }
    }

    IEnumerator AnimatePulsatingObject(GameObject pulsatingObject)
    {
        float t = 0;
        float tt = 0;
        Vector3 initScale = pulsatingObject.transform.localScale;
        Vector3 tempScale = pulsatingObject.transform.localScale;
        float r = 0;
        while (true)
        {
            t = 0;
            tt = Random.Range(minPulseTime, maxPulseTime);
            r = Random.Range(minScaleMultiplayer, maxScaleMultiplayer);
            tempScale = pulsatingObject.transform.localScale;
            while (t < tt)
            {
                t += Time.deltaTime;
                pulsatingObject.transform.localScale = Vector3.Lerp(tempScale, initScale  * r, t/tt);
                yield return null;
            }
        }
    }
}

[Serializable]
public class PlantsVisual
{
    public GameObject rootObject;
    public List<GameObject> pulsatingObjects;
}