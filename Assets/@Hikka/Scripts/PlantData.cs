using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/NewPlantData", order = 1)]
public class PlantData : ScriptableObject
{
    public enum PlantGrowthRequirements
    {
        Light, Darkness, Warmth, Cold, NoWater, 
        WaterEverydayLittle, WaterEverydayMedium, WaterEverydayMuch, 
        WaterOtherdayLittle, WaterOtherdayMedium, WaterOtherdayMuch,
        BloodEverydayLittle, BloodEverydayMedium, BloodEverydayMuch, 
        BloodOtherdayLittle, BloodOtherdayMedium, BloodOtherdayMuch,
        GoldEverydayLittle,GoldEverydayMedium,GoldEverydayMuch,
        GoldOtherdayLittle,GoldOtherdayMedium,GoldOtherdayMuch
    }

    public int plantIndex = 0;
    public List<string> plantName = new List<string>();
    public int growDays = 3;
    public Sprite plantIcon;
    public List<string> plantDecription = new List<string>();

    public List<PlantGrowthRequirements> growthRequirementsList = new List<PlantGrowthRequirements>();
}