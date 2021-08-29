using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetSpawner : MonoBehaviour
{
    public enum ObjectType
    {
        ProceduralPlant, Prop, Path, Tool, Mob
    }

    public static AssetSpawner instance;
    
    private readonly Dictionary<AssetReference, List<GameObject>> spawnedAssets = 
        new Dictionary<AssetReference, List<GameObject>>();

    private readonly Dictionary<AssetReference, Queue<Vector3>> queuedSpawnRequests = 
        new Dictionary<AssetReference, Queue<Vector3>>(); 
    private readonly Dictionary<AssetReference, AsyncOperationHandle<GameObject>> asyncOperationHandles = 
        new Dictionary<AssetReference, AsyncOperationHandle<GameObject>>();

    void Awake()
    {
        instance = this;
    }

    public void SpawnProceduralPlant(AssetReference assetReference, Vector3 newPos, Quaternion newRot)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
        }
        if (asyncOperationHandles.ContainsKey(assetReference)) // if exists
        {
            if (asyncOperationHandles[assetReference].IsDone) // if exists and loaded
            {
                SpawnFromLoadedReference(assetReference, newPos, newRot, ObjectType.ProceduralPlant);
            }
            else // if exists and not loaded
                EnqueueSpawnForAfterInitialization(assetReference, newPos, newRot);
                
            return;
        }
        
        // if not exists
        LoadAndSpawn(assetReference, newPos, newRot, ObjectType.ProceduralPlant);
    }
    
    public void Spawn(AssetReference assetReference, Vector3 newPos, Quaternion newRot, ObjectType objType)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
        }
        if (asyncOperationHandles.ContainsKey(assetReference)) // if exists
        {
            if (asyncOperationHandles[assetReference].IsDone) // if exists and loaded
            {
                SpawnFromLoadedReference(assetReference, newPos, newRot, objType);
            }
            else // if exists and not loaded
                EnqueueSpawnForAfterInitialization(assetReference, newPos, newRot);
                
            return;
        }
        
        // if not exists
         LoadAndSpawn(assetReference, newPos, newRot, objType);
    }

    public void SpawnTile(AssetReference assetReference, Vector3 newPos, int tileIndex, int masterRoomIndex, int effectIndex, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        if (assetReference.RuntimeKeyIsValid() == false)
        {
            Debug.Log("Invalid Key " + assetReference.RuntimeKey.ToString());
        }
        
        if (asyncOperationHandles.ContainsKey(assetReference)) // if exists
        {
            if (asyncOperationHandles[assetReference].IsDone) // if exists and loaded
            {
                SpawnTileFromLoadedReference(assetReference, newPos, tileIndex, masterRoomIndex, effectIndex, spawner, coridorIndex, cultTile);
            }
            else // if exists and not loaded
                EnqueueSpawnForAfterInitialization(assetReference, newPos, Quaternion.identity);
                
            return;
        }
        
        // if not exists
        LoadAndSpawnTile(assetReference, newPos, tileIndex, masterRoomIndex, effectIndex, spawner, coridorIndex, cultTile);
    }

    void SpawnFromLoadedReference(AssetReference assetReference, Vector3 newPos, Quaternion newRot,  ObjectType objectType)
    {
        assetReference.InstantiateAsync(newPos, Quaternion.identity).Completed 
            += (asyncOperationHandle) =>
        {
            if (spawnedAssets.ContainsKey(assetReference) == false)
            {
                spawnedAssets[assetReference] = new List<GameObject>();
            }
            
            spawnedAssets[assetReference].Add(asyncOperationHandle.Result);

            switch (objectType)
            {
                case ObjectType.ProceduralPlant:
                    ProceedPlant(asyncOperationHandle.Result);
                    break;
                case ObjectType.Prop:
                    ProceedProp(asyncOperationHandle.Result);
                    break;
                case ObjectType.Path:
                    ProceedPath(asyncOperationHandle.Result);
                    break;
                case ObjectType.Tool:
                    ProceedTool(asyncOperationHandle.Result);
                    break;
                case ObjectType.Mob:
                    ProceedMob(asyncOperationHandle.Result);
                    break;
            }
            
            var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
            notify.Destroyed += Remove;
            notify.AssetReference = assetReference;
        };
    }
    void SpawnTileFromLoadedReference(AssetReference assetReference, Vector3 newPos, int tileIndex, int masterRoomIndex, int effectIndex, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        assetReference.InstantiateAsync(newPos, Quaternion.identity).Completed 
            += (asyncOperationHandle) =>
        {
            if (spawnedAssets.ContainsKey(assetReference) == false)
            {
                spawnedAssets[assetReference] = new List<GameObject>();
            }
            
            spawnedAssets[assetReference].Add(asyncOperationHandle.Result);

            StartCoroutine(DynamicLevelGenerator.instance.ProceedTile(asyncOperationHandle.Result));
            
            var notify = asyncOperationHandle.Result.AddComponent<NotifyOnDestroy>();
            notify.Destroyed += Remove;
            notify.AssetReference = assetReference;
        };
    }

    void ProceedPlant(GameObject go)
    {
        StartCoroutine(InteractiveObjectsManager.instance.GetClosestInteractiveObject(go.transform.position,
            InteractiveObjectsManager.instance.potsInteractiveObjects).plantController.ProceedPlant(go));
    }
    
    void ProceedProp(GameObject go)
    {
        
    }

    void ProceedPath(GameObject go)
    {
        DynamicLevelGenerator.instance.ProceedPath(go);
    }
    
    void ProceedTool(GameObject go)
    {
        
    }
    void ProceedMob(GameObject go)
    {
        MobSpawnManager.instance.AddSpawnedMob(go);
    }

    void EnqueueSpawnForAfterInitialization(AssetReference assetReference, Vector3 newPos, Quaternion newRot )
    {
        if (queuedSpawnRequests.ContainsKey(assetReference) == false)
            queuedSpawnRequests[assetReference] = new Queue<Vector3>();
        queuedSpawnRequests[assetReference].Enqueue(newPos);
    }

    void LoadAndSpawn(AssetReference assetReference, Vector3 newPos, Quaternion newRot, ObjectType objectType)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnFromLoadedReference(assetReference, newPos, newRot, objectType);
            if (queuedSpawnRequests.ContainsKey(assetReference))
            {
                while (queuedSpawnRequests[assetReference]?.Any() == true)
                {
                    var position = queuedSpawnRequests[assetReference].Dequeue();
                    SpawnFromLoadedReference(assetReference, position, newRot, objectType);
                }
            }
        };
    }
    void LoadAndSpawnTile(AssetReference assetReference, Vector3 newPos, int tileIndex, int masterRoomIndex, int effectIndex, 
        bool spawner, int coridorIndex, bool cultTile)
    {
        var op = Addressables.LoadAssetAsync<GameObject>(assetReference);
        asyncOperationHandles[assetReference] = op;
        op.Completed += (operation) =>
        {
            SpawnTileFromLoadedReference(assetReference, newPos, tileIndex, masterRoomIndex, effectIndex, spawner,coridorIndex,cultTile);
            if (queuedSpawnRequests.ContainsKey(assetReference))
            {
                while (queuedSpawnRequests[assetReference]?.Any() == true)
                {
                    var position = queuedSpawnRequests[assetReference].Dequeue();
                    SpawnTileFromLoadedReference(assetReference, position, tileIndex, masterRoomIndex, effectIndex, spawner,coridorIndex,cultTile);
                }
            }
        };
    }

    void Remove(AssetReference assetReference, NotifyOnDestroy obj)
    {
        Addressables.ReleaseInstance(obj.gameObject);

        spawnedAssets[assetReference].Remove(obj.gameObject);
        if (spawnedAssets[assetReference].Count == 0)
        {
           // Debug.Log($"Removed all{assetReference.RuntimeKey.ToString()}");
            
            if (asyncOperationHandles.Count > 0 && asyncOperationHandles[assetReference].IsValid())
                Addressables.Release(asyncOperationHandles[assetReference]);

            asyncOperationHandles.Remove(assetReference);
        }
    }
}