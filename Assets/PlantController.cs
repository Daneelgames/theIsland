using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class PlantController : MonoBehaviour
{
    public Transform plantOrigin;
    public int currentVisualIndex = -1;
    public List<PlantsVisual> visualsByLifetime;

    public float minPulseTime = 0.5f;
    public float maxPulseTime = 1f;
    public float minScaleMultiplayer = 0.5f;
    public float maxScaleMultiplayer = 2f;

    public List<AssetReference> plantsVisualsReferences = new List<AssetReference>();

    void Start()
    {
        foreach (var visual in visualsByLifetime)
        {
            visual.rootObject.SetActive(false);
        }
    }
    
    public void PlantSeed(int seedIndex)
    {
        AssetSpawner.instance.SpawnPlantVisual(plantsVisualsReferences[seedIndex], plantOrigin.position, transform.rotation);

    }

    public IEnumerator ProceedPlant(GameObject plantVisualGO)
    {
        var plantVisual = plantVisualGO.GetComponent<PlantVisualController>();
        
        visualsByLifetime = plantVisual.visualsByLifetime;

        for (int i = 0; i < visualsByLifetime.Count; i++)
        {
            visualsByLifetime[i].rootObject.SetActive(false);
        }

        currentVisualIndex = 0;
        visualsByLifetime[0].rootObject.transform.localScale = Vector3.zero;
        visualsByLifetime[0].rootObject.SetActive(true);
        
        float t = 0;
        float tt = Random.Range(1,5);

        while (t < tt)
        {
            t += Time.deltaTime;
            visualsByLifetime[0].rootObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t/tt);
            yield return null;
        }
        visualsByLifetime[0].rootObject.transform.localScale = Vector3.one;
        
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