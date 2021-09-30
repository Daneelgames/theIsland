using System.Collections;
using System.Collections.Generic;
using Polarith.AI.Move;
using Polarith.AI.Package;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AstarWalker : MonoBehaviour
{
    public Transform targetTransform;
    public float updatePathRate = 3;
    public float updateCurrentTargetTileOnPathRate = 0.5f;
    public List<Tile> path = new List<Tile>();
    public int currentTargetTileOnPathIndex = 0;
    void Start()
    {
        StartCoroutine(FollOwTarget());
    }

    IEnumerator FollOwTarget()
    {
        while (true)
        {
            StartCoroutine(Astar.instance.FindPath(transform.position, targetTransform.position, this));
            float t = 0;
            float d = 0;
            while (t < updatePathRate)
            {
                if (d >= updateCurrentTargetTileOnPathRate)
                {
                    UpdateTargetTile();
                    d = 0;
                }
                t += Time.deltaTime;
                d += Time.deltaTime;
                yield return null;
            }
        }
    }

    public void UpdatePath(List<Tile> newPath)
    {
        path = new List<Tile>(newPath);
        currentTargetTileOnPathIndex = 0;
    }

    public void UpdateTargetTile()
    {
        float distance = 1000;
        float newDistance = 0;
        Tile closestTile = null;
        
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == null)
                continue;
            newDistance = Vector3.Distance(transform.position, path[i].worldPosition);
            if (newDistance < distance && i > currentTargetTileOnPathIndex)
            {
                distance = newDistance;
                closestTile = path[i];
            }
        }

        if (newDistance < NavigationManager.instance.tileSize / 5)
        {
            if (path.Count > path.IndexOf(closestTile) + 1)
            {
                // GO FOR NEXT POINT   
                currentTargetTileOnPathIndex = path.Count - 1;
            }
            else
            {
                // GO TO CLOSEST TILE
                currentTargetTileOnPathIndex = path.IndexOf(closestTile);
            }
        }
    }

    public void UpdateTargetPosition(Vector3 newPos)
    {
        targetTransform.position = newPos;
    }
    
    public Vector3 GetDirectionToNextTile()
    {
        return Vector3.zero;
    }
}