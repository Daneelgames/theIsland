using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/NewPlantData", order = 1)]
public class PlantData : ScriptableObject
{
    public int inventoryIndex = 0;
    public List<string> plantName = new List<string>();
    public int growDays = 3;
    public Sprite plantIcon;
    
    public List<string> plantDecription = new List<string>();

    public List<PlantController.ActionWithPlant> growthRequirementsList = new List<PlantController.ActionWithPlant>();
}