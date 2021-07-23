using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/NewToolData", order = 1)]
public class ToolData : ScriptableObject
{
    public int inventoryIndex = 0;
    public List<string> toolName = new List<string>();
    public Sprite toolIcon;
    
    public List<string> toolDecription = new List<string>();

}