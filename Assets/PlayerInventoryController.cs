using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController instance;
    
    public List<InventoryPlant> plantsInInventory;

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

        for (int i = 0; i < plantsInInventory.Count; i++)
        {
            if (plantsInInventory[i].amount > 0)
                temp.Add(plantsInInventory[i]);
        }

        return temp;
    }
}

[Serializable]
public class InventoryPlant
{
    public PlantData plant;
    public int amount = 0;
}