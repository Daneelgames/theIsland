using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController instance;

    public List<InventorySlot> inventory;
    
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public List<InventorySlot> GetInventory()
    {
        return inventory;
    }
    
    public int SeedUsed(int seedIndex)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].plantData && inventory[i].plantData.inventoryIndex == seedIndex)
            {
                inventory[i].amount--;
                return inventory[i].amount;
            }
        }
        return 0;
    }
    
    public void NewSeedFound(int seedIndex)
    {
        // already have this item
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].plantData && inventory[i].plantData.inventoryIndex == seedIndex)
            {
                inventory[i].amount++;
                return;
            }
        }

        for (int i = 0; i < PlayerToolsController.instance.allTools.Count; i++)
        {
            if (PlayerToolsController.instance.allTools[i].inventoryIndex == seedIndex)
            {
                inventory.Add(new InventorySlot());
                inventory[inventory.Count-1].plantData = PlayerToolsController.instance.allTools[i].plantData;
                inventory[inventory.Count-1].toolData = PlayerToolsController.instance.allTools[i].toolData;
                inventory[inventory.Count-1].inventoryIndex = PlayerToolsController.instance.allTools[i].inventoryIndex;
                inventory[inventory.Count-1].amount = 1;
                return;
            }
        }
    }
}

[Serializable]
public class InventorySlot
{
    public int inventoryIndex = -1;
    public PlantData plantData;
    public ToolData toolData;
    public int amount = 0;
}