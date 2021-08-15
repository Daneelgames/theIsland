using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProceduralPlantsManager : MonoBehaviour
{
    [SerializeField] List<AssetReference> proceduralPlantsReferences;
    
    [SerializeField] List<ProceduralPlant> spawnedProceduralPlants;

    public static ProceduralPlantsManager instance;

    void Awake()
    {
        instance = this;
    }

    public List<AssetReference> ProceduralPlantsReferences
    {
        get => proceduralPlantsReferences;
    }
    
    public List<ProceduralPlant> SpawnedProceduralPlants
    {
        get => spawnedProceduralPlants;
    }

    public void AddProceduralPlant(ProceduralPlant plant)
    {
        spawnedProceduralPlants.Add(plant);
    }
    
    public void NewDay()
    {
        for (int i = 0; i < spawnedProceduralPlants.Count; i++)
        {
            spawnedProceduralPlants[i].NewDay();
        }
    }
}
