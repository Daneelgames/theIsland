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

    [ContextMenu("GenerateNavigationTiles")]
    public void GenerateNavigationTiles()
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

        /*
        var tempPos = transform.position + new Vector3(bounds.leftBound.localPosition.x, bounds.bottomBound.localPosition.y, bounds.forwardBound.position.z);
        GameObject zeroObject = new GameObject("Tile 0-0-0");
        zeroObject.transform.position = tempPos;
        zeroObject.transform.parent = tilesParent;
        var collider = zeroObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(4.5f,4.5f,4.5f);
        */
            
        // 0,0,0 is front left bottom tile
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    tiles[x, y, z] = new Tile();

                    tiles[x, y, z].worldPosition = transform.position + 
                            new Vector3(bounds.leftBound.localPosition.x, bounds.topBound.localPosition.y, bounds.forwardBound.position.z) + 
                        new Vector3( _navigationManager.tileSize * x, _navigationManager.tileSize * y,_navigationManager.tileSize * z);
                    
                    Debug.Log(tiles[x, y, z].worldPosition);
                    var tempPos = tiles[x, y, z].worldPosition;
                    GameObject tileObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    tileObject.name = "Tile " + x + "; "+ y + "; " + z + ";";
                    tileObject.transform.position = tempPos;
                    tileObject.transform.localScale = new Vector3(4.5f,4.5f,4.5f);
                    tileObject.transform.parent = tilesParent;
                    var meshRenderer = tileObject.GetComponent<MeshRenderer>();
                    meshRenderer.material = _navigationManager.freeTileMaterial;

                    tiles[x, y, z].tileObject = tileObject;
                    tilesSpawned.Add(tiles[x, y, z]);
                    /*
                    var collider = zeroObject.AddComponent<BoxCollider>();
                    collider.size = new Vector3(4.5f,4.5f,4.5f);
                    */
                }
            }
        }
    }

    [ContextMenu("ClearTiles")]
    public void ClearTiles()
    {
        if (tiles == null || tiles.Length <= 0)
            return;
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    if (tiles[x, y, z] != null && tiles[x,z,y].tileObject != null)
                    {
                        tiles[x, y, z] = null;
                        Destroy(tiles[x,z,y].tileObject);
                    }
                }
            }
        }

        tiles = null;
    }

    IEnumerator Start()
    {
        ClearTiles();
        GenerateNavigationTiles();
        //CheckNavigationTilesForObstacles();
        
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
}

[Serializable]
public class Tile
{
    public GameObject tileObject;
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
