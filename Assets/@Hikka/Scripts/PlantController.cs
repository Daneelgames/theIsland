using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class PlantController : MonoBehaviour
{
    public enum ActionWithPlant
    {
        GiveWater, UseBlade
    }

    [SerializeField] private List<ActionWithPlant> actionsByDay = new List<ActionWithPlant>();
    
    public Transform plantOrigin;

    ProceduralPlant spawnedProceduralPlant;
    PlantData plantData;

    // list actions used by day
    
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
        
        spawnedProceduralPlant.PlantBorn(this);
        yield break;
    }

    public void WaterUsed(float waterAmount)
    {
        actionsByDay.Add(ActionWithPlant.GiveWater);
    }

    public void BladeUsed()
    {
        actionsByDay.Add(ActionWithPlant.UseBlade);
    }

    public int CompareActionsWithRequirements()
    {
        int hpOffset = 0;

        var tempRequirementsList = new List<ActionWithPlant>(plantData.growthRequirementsList);
        
        for (int i = tempRequirementsList.Count - 1; i >= 0; i--)
        {
            for (int j = actionsByDay.Count - 1; j >= 0; j--)
            {
                if (actionsByDay[j] == tempRequirementsList[i])
                {
                    //
                    tempRequirementsList.RemoveAt(i);
                    actionsByDay.RemoveAt(j);
                    
                    break;
                }
            }

            if (tempRequirementsList.Count != 0 || actionsByDay.Count != 0)
            {
                hpOffset = -1;
            }
            else
                hpOffset = 1;
        }   
        
        actionsByDay.Clear();
        return hpOffset;
    }
}