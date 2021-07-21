using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController instance;
    
    public List<InventoryPlant> seedsInInventory;
    
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public List<InventoryPlant> GetPlantsInInventory()
    {
        List<InventoryPlant> temp = new List<InventoryPlant>();

        for (int i = 0; i < seedsInInventory.Count; i++)
        {
            if (seedsInInventory[i].amount > 0)
                temp.Add(seedsInInventory[i]);
        }

        return temp;
    }

    public int SeedUsed(int seedIndex)
    {
        seedsInInventory[seedIndex].amount --;
        return seedsInInventory[seedIndex].amount;
    }
}

[Serializable]
public class InventoryPlant
{
    public PlantData plant;
    public int amount = 0;
}