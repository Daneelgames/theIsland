using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSolids : MonoBehaviour
{
    public static LevelSolids instance;

    public List<GameObject> solids = new List<GameObject>();

    void Awake()
    {
        instance = this;
    }
}
