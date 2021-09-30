using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{
    public static Astar instance;

    private void Awake()
    {
        instance = this;
    }
    
    public IEnumerator FindPath(Vector3 startPos, Vector3 targetPos, AstarWalker astarWalker)
    {
        Debug.Log("FindPath for " + astarWalker);
        Tile startTile = NavigationManager.instance.TileFromWorldPosition(startPos);
        Tile targetTile = NavigationManager.instance.FreeTileFromWorldPosition(targetPos);
        
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>();
        
        openSet.Add(startTile);
        int t = 0;
        while (openSet.Count > 0)
        {
            Tile currentTile = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentTile.fCost || openSet[i].fCost == currentTile.fCost && openSet[i].hCost < currentTile.hCost )
                {
                    currentTile = openSet[i];
                }
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile)
            {
                RetracePath(startTile, targetTile, astarWalker);
                yield break;
            }

            foreach (var neighbour in NavigationManager.instance.GetNeighbours(currentTile))
            {
                if (neighbour.occupied || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newMovementCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
                if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newMovementCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetTile);
                    neighbour.parent = currentTile;
                    
                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }

            t++;
            if (t == 50)
            {
                t = 0;
                yield return null;
            }
        }
    }

    void RetracePath(Tile startTile, Tile endTile, AstarWalker astarWalker)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;
        while (currentTile  != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }
        path.Reverse();

        if (astarWalker)
        {
            astarWalker.UpdatePath(path);
        }
        else
        {
            NavigationManager.instance.SetPath(path);   
        }   
    }

    int GetDistance(Tile tileA, Tile tileB)
    {
        int distX = Mathf.Abs(tileA.coordinates.x - tileB.coordinates.x);
        int distY = Mathf.Abs(tileA.coordinates.y - tileB.coordinates.y);
        int distZ = Mathf.Abs(tileA.coordinates.z - tileB.coordinates.z);

        return distX + distY + distZ;
    }
}
