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
        Debug.Log("SeedUsed. seedIndex: " + seedIndex);
        int tempAmount = 0;
        for (int i = inventory.Count - 1; i >= 0; i--)
        {
            if (inventory[i].plantData && inventory[i].plantData.inventoryIndex == seedIndex)
            {
                inventory[i].amount--;

                tempAmount = inventory[i].amount;

                if (inventory[i].amount <= 0)
                {
                    inventory.RemoveAt(i);
                    PlayerToolsController.instance.selectedToolIndex = -1;
                }
                
                return tempAmount;
            }
        }
        return 0;
    }
    
    public void NewSeedFound(int seedIndex)
    {
        // already have this item
        Debug.Log("NewSeedFound(int seedIndex) " + seedIndex);
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
    public void NewToolFound(int toolIndex)
    {
        // already have this item
        Debug.Log("NewToolFound(int toolIndex) " + toolIndex);
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].toolData && inventory[i].toolData.inventoryIndex == toolIndex)
            {
                inventory[i].amount++;
                return;
            }
        }

        for (int i = 0; i < PlayerToolsController.instance.allTools.Count; i++)
        {
            if (PlayerToolsController.instance.allTools[i].inventoryIndex == toolIndex)
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