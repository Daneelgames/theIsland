using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    public static NavigationManager instance;
    
    public float tileSize = 5;

    public NavigationRoom activeNavigationRoom;
    

    [Header("Debug")] 
    public Material freeTileMaterial;
    public Material occupiedTileMaterial;
}
