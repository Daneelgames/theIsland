using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Polarith.AI.Move;
using UnityEngine;

public class SetPlayerToAI : MonoBehaviour
{
    public AIMSeek aimSeek;
    public AIMSeekBounds aimSeekBounds;
    void Start()
    {
        aimSeek.GameObjects.Add(PlayerMovement.instance.gameObject);
        for (int i = 0; i < LevelSolids.instance.solids.Count; i++)
        {
            aimSeekBounds.GameObjects.Add(LevelSolids.instance.solids[i]);
        }
        //aimSeekBounds.GameObjects = new List<GameObject>(LevelSolids.instance.solids);
    }
}
