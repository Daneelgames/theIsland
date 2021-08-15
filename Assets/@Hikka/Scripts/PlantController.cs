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

    ProceduralPlant spawnedProceduralPlant;
    PlantData plantData;


    public ProceduralPlant GetSpawnedProceduralPlant()
    {
        return spawnedProceduralPlant;
    }
    
    public void PlantSeed(int seedIndex)
    {
        AssetSpawner.instance.SpawnProceduralPlant(ProceduralPlantsManager.instance.ProceduralPlantsReferences[seedIndex], plantOrigin.position, transform.rotation);
    }

    public IEnumerator ProceedPlant(GameObject plantVisualGO)
    {
        plantVisualGO.transform.parent = transform;
        spawnedProceduralPlant = plantVisualGO.GetComponent<ProceduralPlant>();
        plantData = spawnedProceduralPlant.plantData;
        
        spawnedProceduralPlant.PlantBorn();
        yield break;
    }

    public void WaterUsed(float waterAmount)
    {
        for (int i = 0; i < plantData.growthRequirementsList.Count; i++)
        {
            if (plantData.growthRequirementsList[i] == PlantData.PlantGrowthRequirements.NoWater)
            {
                // this is bad for plant
                Debug.Log("This plant doesn't like water.");
            }
            else if (plantData.growthRequirementsList[i] == PlantData.PlantGrowthRequirements.WaterEverydayMedium)
            {
                // this is bad for plant
                Debug.Log("This plant likes water.");
            }
        }
    }
}