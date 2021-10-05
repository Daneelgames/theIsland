using System;
using System.Collections;
using System.Collections.Generic;
using Depths.Scripts;
using UnityEngine;
using UnityEngine.Rendering;

public class NavigationRoom : MonoBehaviour
{
    public Vector3Int roomSize;

    public Tile[,,] tiles;

    public List<Tile> tilesSpawned;
    public Transform tilesParent;

    public LayerMask obstaclesLayerMask;

    public bool debug = false;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, (Vector3)roomSize * GamePropertiesStatic.tileSize);
    }

    [ContextMenu("GenerateRoomNavigation")]
    public void GenerateRoomNavigation()
    {
        ClearTiles();
        InitTiles();
        GenerateNavigationTiles();
        BoxCastAgainstSpawnedTiles();
    }

    void InitTiles()
    {
        tiles = new Tile[roomSize.x, roomSize.y,roomSize.z];
    }
    
    void GenerateNavigationTiles()
    {
        // 0,0,0 is back left bottom tile
        Vector3 zeroTilePos = new Vector3(-roomSize.x / 2 * GamePropertiesStatic.tileSize, -roomSize.y / 2 * GamePropertiesStatic.tileSize, -roomSize.z / 2 * GamePropertiesStatic.tileSize);
        
        for (int x = 0; x < roomSize.x; x++)
        {
            for (int y = 0; y < roomSize.y; y++)
            {
                for (int z = 0; z < roomSize.z; z++)
                {
                    tiles[x, y, z] = new Tile();
                    tiles[x,y,z].coordinates = new Vector3Int(x,y,z);
                     
                    Vector3 newTileOffset = new Vector3(GamePropertiesStatic.tileSize * x, GamePropertiesStatic.tileSize * y, GamePropertiesStatic.tileSize * z);
                    
                    tiles[x, y, z].worldPosition = transform.position + zeroTilePos + newTileOffset;
                    
                    var tempPos = tiles[x, y, z].worldPosition;

                    if (debug)
                    {
                        GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        DestroyImmediate(tileObject.GetComponent<Collider>());
                        tileObject.name = "Tile " + x + "; "+ y + "; " + z + ";";
                        tileObject.transform.position = tempPos;
                        tileObject.transform.localScale = new Vector3(.5f,.5f,.5f);
                        tileObject.transform.parent = tilesParent;
                    
                        tiles[x,y,z].meshRenderer = tileObject.GetComponent<MeshRenderer>();
                        tiles[x,y,z].meshRenderer.material = NavigationManager.instance.freeTileMaterial;
                    
                        if (tiles[x,y,z].occupied)
                            tiles[x,y,z].meshRenderer.material = NavigationManager.instance.occupiedTileMaterial;

                        tiles[x, y, z].tileObject = tileObject;   
                    }
                    
                    tilesSpawned.Add(tiles[x, y, z]);
                }
            }
        }
    } 
    
    void BoxCastAgainstSpawnedTiles()
    {
        int overlappedTilesAmount = 0;

        for (var index = 0; index < tilesSpawned.Count; index++)
        {
            var tile = tilesSpawned[index];
            
            Collider[] colliders = Physics.OverlapBox(tile.worldPosition, Vector3.one * 2.4f, transform.rotation, obstaclesLayerMask);

            if (colliders.Length > 0)
            {
                tile.occupied = true;
                overlappedTilesAmount++;
                
                if (debug)
                {
                    tile.meshRenderer.material = NavigationManager.instance.occupiedTileMaterial;   
                }
            }
        }

        Debug.Log("BoxcastAgainstSpawnedDebugTiles; overlappedTilesAmount = " + overlappedTilesAmount);
        
        ClearSpawnedObjects();
    }

    [ContextMenu("ClearSpawnedObjects")]
    public void ClearSpawnedObjects()
    {
        if (debug == false)
            return;
        
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
    public Vector3 worldPosition;
    public bool occupied = false;
    public Vector3Int coordinates;
    public int gCost;
    public int hCost;

    public Tile parent;
    // DEBUG
    public GameObject tileObject;
    public MeshRenderer meshRenderer;
    public int fCost { get { return gCost + hCost; } }
}