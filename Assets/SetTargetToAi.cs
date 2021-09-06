using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Polarith.AI.Move;
using Polarith.AI.Package;
using UnityEngine;

public class SetTargetToAi : MonoBehaviour
{
    public GameObject currentTarget;
    public AIMSeek aimSeek;
    public AIMSeekBounds aimSeekBounds;

    public float targetStopDistance;

    public SpaceshipController spaceshipController;
    void Start()
    {
        SetTarget(currentTarget);
        
        /*
        for (int i = 0; i < LevelSolids.instance.solids.Count; i++)
        {
            aimSeekBounds.GameObjects.Add(LevelSolids.instance.solids[i]);
        }*/
        //aimSeekBounds.GameObjects = new List<GameObject>(LevelSolids.instance.solids);
    }

    public void SetTarget(GameObject targetGO)
    {
        aimSeek.GameObjects.Clear();
     
        currentTarget = targetGO;   
        aimSeek.GameObjects.Add(currentTarget);   
        
        spaceshipController.SetNewTarget(currentTarget, targetStopDistance);
    }
}