using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class FishSpawnManager : MonoBehaviour
{
    public static FishSpawnManager instance;

    public List<FishSpawn> fishList;
    public float maxDistance = 50;
    public int spawnedInstancesMax = 10;
    public List<HealthController> spawnedFishes = new List<HealthController>();

    private float spawnDelay = 0.1f;
    private float updateDelay = 1;

    public float minSpawnDistanceToPlayer = 10;
    public float maxSpawnDistanceToPlayer = 20;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        StartCoroutine(KeepFishAroundPlayer());
    }

    IEnumerator KeepFishAroundPlayer()
    {
        while (true)
        {
            for (int i = spawnedFishes.Count; i < spawnedInstancesMax; i++)
            {
                SpawnFishAroundPlayer();   
                //yield return new WaitForSeconds(spawnDelay);
                yield return null;
            }
            for (int i = spawnedFishes.Count - 1; i >= 0; i--)
            {
                if (spawnedFishes[i] == null)
                {
                    spawnedFishes.RemoveAt(i);
                    continue;
                }
                
                if (Vector3.Distance(spawnedFishes[i].transform.position, PlayerMovement.instance.transform.position) > maxDistance)
                {
                    spawnedFishes[i].transform.position = SpawnPositionAroundPlayer(minSpawnDistanceToPlayer, maxSpawnDistanceToPlayer);
                }
                yield return null;
            }
            yield return new WaitForSeconds(updateDelay);
        }
    }

    void SpawnFishAroundPlayer()
    {
        var spawnPos = SpawnPositionAroundPlayer(minSpawnDistanceToPlayer, maxSpawnDistanceToPlayer);

        AssetReference fishToSpawn = fishList[Random.Range(0,fishList.Count)].fishReference;
        AssetSpawner.instance.Spawn(fishToSpawn, spawnPos, Quaternion.identity, AssetSpawner.ObjectType.Fish);
    }

    Vector3 SpawnPositionAroundPlayer(float minDistance, float maxDistance)
    {
        return PlayerMovement.instance.transform.position + Random.onUnitSphere * Random.Range(minDistance, maxDistance);
    }

    public void ProceedFish(GameObject go)
    {
        HealthController hc = go.GetComponent<HealthController>();
        if (hc == null)
            return;
        
        spawnedFishes.Add(hc);
    }
    
}

[Serializable]
public class FishSpawn
{
    public AssetReference fishReference;
    [Range(0,1)]public float weight = 0.5f;
}
