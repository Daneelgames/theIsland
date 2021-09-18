using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

public class MobSpawnManager : MonoBehaviour
{
    public static MobSpawnManager instance;
    
    public List<Transform> spawners = new List<Transform>();
    
    public Vector2Int spawnDelayMinMax = new Vector2Int(30, 31);
    public Vector2Int distanceToPlayerToSpawnMinMax = new Vector2Int(30, 101);
    public Vector2Int mobsSpawnPerCycleAmountMinMax = new Vector2Int(1, 2);

    public int maxAliveMobsAmount = 5;
    
    [SerializeField] List<AssetReference> mobsReferences = new List<AssetReference>();
    [SerializeField] List<HealthController> _units = new List<HealthController>();
    public List<HealthController> spawnedMobs = new List<HealthController>();

    [SerializeField] List<Rigidbody> _activeRidigbodies = new List<Rigidbody>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //StartSpawningMobs();
    }

    public void SetMaxMobsAlive(int amount)
    {
        maxAliveMobsAmount = amount;
    }
    
    public void StartSpawningMobs()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnEnemiesCoroutine());
    }

    public List<HealthController> Units
    {
        get { return _units; }
    }
    
    public void AddUnit(HealthController hc)
    {
        _units.Add(hc);
    }

    IEnumerator SpawnEnemiesCoroutine()
    {
        while (true)
        {
            if (maxAliveMobsAmount > spawnedMobs.Count)
                StartCoroutine(SpawnEnemies());
            
            yield return new WaitForSeconds(Random.Range(spawnDelayMinMax.x, spawnDelayMinMax.y));
        }
    }

    IEnumerator SpawnEnemies()
    {
        int mobsToSpawn = Random.Range(mobsSpawnPerCycleAmountMinMax.x, mobsSpawnPerCycleAmountMinMax.y);

        var tempMobsList = new List<AssetReference>(mobsReferences);
        var tempSpawnersList = new List<Transform>(spawners);

        for (int i = tempSpawnersList.Count - 1; i >= 0; i--)
        {
            float distance = Vector3.Distance(PlayerMovement.instance.transform.position, tempSpawnersList[i].position);
            if (distance < distanceToPlayerToSpawnMinMax.x || distance > distanceToPlayerToSpawnMinMax.y)
            {
                tempSpawnersList.RemoveAt(i);
            }
        }
        
        for (int i = 0; i < mobsToSpawn; i++)
        {
            if (tempMobsList.Count <= 0)
                tempMobsList = new List<AssetReference>(mobsReferences);
            if (tempSpawnersList.Count <= 0)
                tempSpawnersList = new List<Transform>(spawners);
            
            int r = Random.Range(0, tempMobsList.Count);
            int rr = Random.Range(0, tempSpawnersList.Count);
            AssetSpawner.instance.Spawn(tempMobsList[r], tempSpawnersList[rr].position, Quaternion.identity, AssetSpawner.ObjectType.Mob);
            yield return null;
        }
    }

    public void AddSpawnedMob(GameObject go)
    {
        var newHc = go.GetComponent<HealthController>();
        
        if (newHc == null)
            return;
        
        spawnedMobs.Add(newHc);
        _activeRidigbodies.Add(newHc.GetRigidbody);
    }

    public void MobKilled(HealthController hc)
    {
        if (spawnedMobs.Contains(hc))
        {
            spawnedMobs.Remove(hc);
        }
        if (_units.Contains(hc))
        {
            _units.Remove(hc);
        }

        if (_activeRidigbodies.Contains(hc.GetRigidbody))
            _activeRidigbodies.Remove(hc.GetRigidbody);
    }

    public List<Rigidbody> ActiveRigidbodies
    {
        get { return _activeRidigbodies; }
    }

    public HealthController FindHcByName(string name)
    {
        for (int i = 0; i < _units.Count; i++)
        {
            if (_units[i].unitName == name)
            {
                return _units[i];
            }
        }

        return null;
    }
}