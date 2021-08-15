using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ProceduralPlantsManager : MonoBehaviour
{
    [SerializeField] private List<AssetReference> toolPickUpsReferences;
    [SerializeField] private List<AssetReference> proceduralPlantsReferences;
    
    [SerializeField] private List<ProceduralPlant> spawnedProceduralPlants;

    public static ProceduralPlantsManager instance;

    void Awake()
    {
        instance = this;
    }

    public List<AssetReference> ToolPickUpsReferences
    {
        get => toolPickUpsReferences;
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

    public void RemovePlant(ProceduralPlant plant)
    {
        spawnedProceduralPlants.Remove(plant);
    }
}
