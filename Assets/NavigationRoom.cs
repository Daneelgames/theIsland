using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class NavigationRoom : MonoBehaviour
{
    public Bounds bounds;

    public Vector3Int roomSize;
    float width = 1;
    float height = 1;
    float length = 1;

    public Tile[,,] tiles;

    public List<Tile> tilesSpawned;
    public Transform tilesParent;
    [SerializeField] NavigationManager _navigationManager;

    public LayerMask obstaclesLayerMask;

    IEnumerator Start()
    {
        while (true)
        {
            if (tilesSpawned.Count <= 0)
            {
                yield return null;
                continue;
            }

            int t = 0;
            for (int i = 0; i < tilesSpawned.Count; i++)
            {
                tilesSpawned[i].tileObject.SetActive(!tilesSpawned[i].tileObject.activeInHierarchy);
                t++;
                if (t > 20)
                {
                    t = 0;
                    yield return null;   
                }
            }
        }
    }

    [ContextMenu("GenerateRoomNavigation")]
    public void GenerateRoomNavigation()
    {
        ClearTiles();
        InitTiles();
        GenerateNavigationTilesDebugVisuals();
        BoxcastAgainstSpawnedDebugTiles();
    }

    void InitTiles()
    {
        // GET BOUNDS
        width = Mathf.Abs(bounds.leftBound.transform.localPosition.x) + Mathf.Abs(bounds.rightBound.transform.localPosition.x);
        width /= _navigationManager.tileSize;
        height = Mathf.Abs(bounds.topBound.transform.localPosition.y) + Mathf.Abs(bounds.bottomBound.transform.localPosition.y);
        height /= _navigationManager.tileSize;
        length = Mathf.Abs(bounds.forwardBound.transform.localPosition.z) + Mathf.Abs(bounds.backBound.transform.localPosition.z);
        length /= _navigationManager.tileSize;

        roomSize = new Vector3Int(Mathf.RoundToInt(width ), Mathf.RoundToInt(height), Mathf.RoundToInt(length));
        
        tiles = new Tile[roomSize.x, roomSize.y,roomSize.z];
        Debug.Log(tiles.Length);
    }
    
    void GenerateNavigationTilesDebugVisuals()
    {
        // 0,0,0 is back left bottom tile
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    tiles[x, y, z] = new Tile();

                    tiles[x, y, z].worldPosition = transform.position + 
                            new Vector3(bounds.leftBound.localPosition.x, bounds.bottomBound.localPosition.y, bounds.backBound.position.z) + 
                        new Vector3( _navigationManager.tileSize * x, _navigationManager.tileSize * y,_navigationManager.tileSize * z);
                    
                    Debug.Log(tiles[x, y, z].worldPosition);
                    var tempPos = tiles[x, y, z].worldPosition;
                    GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileObject.name = "Tile " + x + "; "+ y + "; " + z + ";";
                    tileObject.transform.position = tempPos;
                    tileObject.transform.localScale = new Vector3(.5f,.5f,.5f);
                    tileObject.transform.parent = tilesParent;
                    
                    tiles[x,y,z].meshRenderer = tileObject.GetComponent<MeshRenderer>();
                    tiles[x,y,z].meshRenderer.material = _navigationManager.freeTileMaterial;
                    
                    if (tiles[x,y,z].occupied)
                        tiles[x,y,z].meshRenderer.material = _navigationManager.occupiedTileMaterial;

                    tiles[x, y, z].tileObject = tileObject;
                    tilesSpawned.Add(tiles[x, y, z]);
                }
            }
        }
    } 
    
    void BoxcastAgainstSpawnedDebugTiles()
    {
        int overlappedTilesAmount = 0;

        for (var index = 0; index < tilesSpawned.Count; index++)
        {
            var tile = tilesSpawned[index];
            
            Collider[] colliders = Physics.OverlapBox(tile.worldPosition, Vector3.one * 2.4f, transform.rotation, obstaclesLayerMask);

            if (colliders.Length > 0)
            {
                tile.occupied = true;
                tile.meshRenderer.material = _navigationManager.occupiedTileMaterial;
                overlappedTilesAmount++;
            }
        }

        Debug.Log("BoxcastAgainstSpawnedDebugTiles; overlappedTilesAmount = " + overlappedTilesAmount);
    }

    [ContextMenu("ClearSpawnedObject")]
    public void ClearSpawnedObject()
    {
        for (int i = tilesSpawned.Count - 1; i >= 0; i--)
        {
            if (tilesSpawned[i].tileObject == null)
            {
                continue;
            }

            DestroyImmediate(tilesSpawned[i].tileObject);
        }
    }
    
    [ContextMenu("CLEAR TILES")]
    public void ClearTiles()
    {
        for (int i = tilesSpawned.Count - 1; i >= 0; i--)
        {
            if (tilesSpawned[i].tileObject == null)
            {
                tilesSpawned.RemoveAt(i);
                continue;
            }

            DestroyImmediate(tilesSpawned[i].tileObject);
            tilesSpawned.RemoveAt(i);
        }
        tilesSpawned.Clear();
        tiles = null;
    }
}

[Serializable]
public class Tile
{
    public GameObject tileObject;
    public MeshRenderer meshRenderer;
    public Vector3 worldPosition;
    public bool occupied = false;
}

[Serializable]
public class Bounds
{
    public Transform forwardBound;
    public Transform rightBound;
    public Transform backBound;
    public Transform leftBound;
    public Transform topBound;
    public Transform bottomBound;
}
