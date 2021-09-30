using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager instance;
    
    public float tileSize = 5;

    public NavigationRoom activeNavigationRoom;


    [Header("Debug")] 
    public Transform testTarget;
    public bool debug = true;
    public Material playerTileMaterial;
    public Material freeTileMaterial;
    public Material occupiedTileMaterial;
    public Material pathTileMaterial;
    public List<Tile> path;

    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        if (!debug)
            yield break;

        Tile prevTile = null;
        
        int t = 0;
        
        while (true)
        {
            var currentTile = TileFromWorldPosition(PlayerMovement.instance.transform.position);
            if (currentTile != null)
            {
                currentTile.meshRenderer.material = playerTileMaterial;
                if (prevTile != null && prevTile != currentTile)
                {
                    prevTile.meshRenderer.material = prevTile.occupied ? occupiedTileMaterial : freeTileMaterial;
                }

                t++;
                
                if (t == 5 && testTarget != null)
                {
                    StartCoroutine(Astar.instance.FindPath(currentTile.worldPosition, testTarget.position, null));
                    t = 0;
                }
            }
            yield return new WaitForSeconds(0.1f);
            
            prevTile = currentTile;
        }
    }

    public Tile TileFromWorldPosition(Vector3 worldPosition)
    {
        float distance = 1000;
        float newDistance = 0;
        Tile closestTile = null;
        
        foreach (var tile in activeNavigationRoom.tilesSpawned)
        {
            newDistance = Vector3.Distance(tile.worldPosition, worldPosition); 
            if (newDistance < distance)
            {
                distance = newDistance;
                closestTile = tile;
            }
        }

        return closestTile;
    }

    public List<Tile> GetNeighbours(Tile tile)
    {
        List<Tile> neighbours = new List<Tile>();

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    if (x == 0 && y == 0 && z == 0)
                        continue;

                    int checkX = tile.coordinates.x + x;
                    int checkY = tile.coordinates.y + y;
                    int checkZ = tile.coordinates.z + z;

                    if (checkX >= 0 && checkX < activeNavigationRoom.roomSize.x &&
                        checkY >= 0 && checkY < activeNavigationRoom.roomSize.y &&
                        checkZ >= 0 && checkZ < activeNavigationRoom.roomSize.z)
                    {
                        neighbours.Add(activeNavigationRoom.tiles[checkX, checkY, checkZ]);
                    }
                }
            }
        }
        return neighbours;
    }

    public void SetPath(List<Tile> newPath)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].occupied)
                path[i].meshRenderer.material = occupiedTileMaterial;
            else
                path[i].meshRenderer.material = freeTileMaterial;
        }
        path = new List<Tile>(newPath);
        
        for (int i = 0; i < path.Count; i++)
        {
            path[i].meshRenderer.material = pathTileMaterial;
        }
    }
}
