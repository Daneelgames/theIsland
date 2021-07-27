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

    float minPulseTime = 0.5f;
    float maxPulseTime = 1f;
    float minScaleMultiplayer = 0.5f;
    float maxScaleMultiplayer = 2f;

    public List<AssetReference> plantsVisualsReferences = new List<AssetReference>();

    PlantVisualController spawnedPlantVisual;
    PlantData plantData;

    void Start()
    {
        foreach (var visual in visualsByLifetime)
        {
            visual.rootObject.SetActive(false);
        }
    }

    public PlantVisualController GetSpawnedPlantVisual()
    {
        return spawnedPlantVisual;
    }
    
    public void PlantSeed(int seedIndex)
    {
        AssetSpawner.instance.SpawnPlantVisual(plantsVisualsReferences[seedIndex], plantOrigin.position, transform.rotation);
    }

    public IEnumerator ProceedPlant(GameObject plantVisualGO)
    {
        plantVisualGO.transform.parent = transform;
        spawnedPlantVisual = plantVisualGO.GetComponent<PlantVisualController>();
        plantData = spawnedPlantVisual.plantData;
        
        visualsByLifetime = spawnedPlantVisual.visualsByLifetime;

        minPulseTime = spawnedPlantVisual.minPulseTime;        
        maxPulseTime = spawnedPlantVisual.maxPulseTime;        
        minScaleMultiplayer = spawnedPlantVisual.minScaleMultiplayer;        
        maxScaleMultiplayer = spawnedPlantVisual.maxScaleMultiplayer;        
        
        currentVisualIndex = 0;

        StartCoroutine(SetVisualState(currentVisualIndex));
        yield break;
    }

    IEnumerator SetVisualState(int index)
    {
        for (int i = 0; i < visualsByLifetime.Count; i++)
        {
            visualsByLifetime[i].rootObject.SetActive(false);
        }
        visualsByLifetime[index].rootObject.transform.localScale = Vector3.zero;
        visualsByLifetime[index].rootObject.SetActive(true);
        
        float t = 0;
        float tt = Random.Range(1,5);

        if (index > 0)
            tt = 0;
        
        while (t < tt)
        {
            t += Time.deltaTime;
            visualsByLifetime[index].rootObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t/tt);
            yield return null;
        }
        visualsByLifetime[index].rootObject.transform.localScale = Vector3.one;
        
        for (int i = 0; i < visualsByLifetime[index].pulsatingObjects.Count; i++)
        {
            StartCoroutine(AnimatePulsatingObject(visualsByLifetime[index].pulsatingObjects[i]));   
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

    public void NewCycle()
    {
        if (currentVisualIndex >= 0 && currentVisualIndex < visualsByLifetime.Count - 1)
        {
            currentVisualIndex++; 
            StartCoroutine(SetVisualState(currentVisualIndex));
        }
    }

    public void WaterUsed(float waterAmount)
    {
        for (int i = 0; i < plantData.growthRequirementsList.Count; i++)
        {
            if (plantData.growthRequirementsList[i] == PlantData.PlantGrowthRequirements.NoWater)
            {
                
            }
        }
    }
}

[Serializable]
public class PlantsVisual
{
    public PlantData plantData;
    public GameObject rootObject;
    public List<GameObject> pulsatingObjects;
}