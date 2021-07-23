using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
    public static PlayerInventoryController instance;

    public List<ToolController> inventory;
    
    
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    public List<ToolController> GetInventory()
    {
        inventory.Clear();
        for (int i = 0; i < PlayerToolsController.instance.allTools.Count; i++)
        {
            inventory.Add(PlayerToolsController.instance.allTools[i]);
        }
        
        return inventory;
    }
    
    public int SeedUsed(int seedIndex)
    {
        return 0;
    }
}
