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

    private bool _arrivedToClosestTargetTileInPath = false;

    public float aiShipSpeedScaler = 1;
    public bool lookToMovementDirection = true;
    public float turnSpeed = 1;
    public bool ArrivedToClosestTargetTileInPath
    {
        get { return _arrivedToClosestTargetTileInPath; }
        set { _arrivedToClosestTargetTileInPath = value; }
    }
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
        Debug.Log("Update Path");
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

        // IF UNIT IS CLOSE TO ITS CLOSEST TILE ON PATH
        if (newDistance < NavigationManager.instance.tileSize / 5)
        {
            Debug.Log("path.IndexOf(closestTile) " + path.IndexOf(closestTile) + "; path.Count = " + path.Count);
            ArrivedToClosestTargetTileInPath = true;
            if (path.Count > path.IndexOf(closestTile) + 1)
            {
                // GO FOR NEXT POINT
                Debug.Log("GO FOR NEXT POINT");
                if (currentTargetTileOnPathIndex < path.Count - 1)
                    currentTargetTileOnPathIndex++;
            }
            else
            {
                // GO TO CLOSEST TILE
                Debug.Log("GO TO CLOSEST TILE");
                currentTargetTileOnPathIndex = path.IndexOf(closestTile);
            }
        }
        else // IF UNIT IS FAR FROM ITS CLOSEST TILE ON PATH
        {
            ArrivedToClosestTargetTileInPath = false;
        }
    }

    public void UpdateTargetPosition(Vector3 newPos)
    {
        targetTransform.position = newPos;
    }
    
    public Vector3 GetDirectionToNextTile()
    {
        if (path.Count <= 0 || currentTargetTileOnPathIndex >= path.Count)
            return Vector3.zero;
        
        return (path[currentTargetTileOnPathIndex].worldPosition - transform.position).normalized;
    }
    
    void OnDrawGizmosSelected()
    {
        if (path.Count > 0 )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, path[0].worldPosition);
            for (int i = 0; i < path.Count - 1; i++)
            {
                Gizmos.DrawLine(path[i].worldPosition, path[i+1].worldPosition);
            }
        }
    }
}