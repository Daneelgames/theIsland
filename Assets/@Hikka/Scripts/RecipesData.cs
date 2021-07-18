using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/RecipesData", order = 1)]
public class RecipesData : ScriptableObject
{
    public List<PlantRecipe> plantRecipes;
    public List<Recipe> craftRecipes;
    public List<CraftObjectData> craftObjectsList;
}

[Serializable]
public class PlantRecipe
{
    public List<Ingredient> ingredients;
    public PlantData resultObject;
}

[Serializable]
public class PlantData
{
    public enum PlantType {FingersPlant}

    public PlantType type = PlantType.FingersPlant;
    public List<string> name = new List<string>();
    public List<string> description = new List<string>();
}

[Serializable]
public class Recipe
{
    public List<Ingredient> ingredients;
    public Ingredient resultObject;
}

[Serializable]
public class Ingredient
{
    public CraftObjectData.CraftObjectType objectType = CraftObjectData.CraftObjectType.Gold;
    public int amount = 1;
}

[Serializable]
public class CraftObjectData
{
    public enum CraftObjectType {Gold, FingerSeed}

    public CraftObjectType type = CraftObjectType.Gold;
    public List<string> name = new List<string>();
    public List<string> description = new List<string>();
}
