using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerToolsController : MonoBehaviour
{
    public static PlayerToolsController instance;
    public List<ToolController> allTools;
    public int selectedToolIndex = -1;

    private float useToolCooldown = 0;
    
    private void Awake()
    {
        instance = this;
    }

    public void SelectTool(int newSelectedTool)
    {
        selectedToolIndex = newSelectedTool;
        
        for (int i = 0; i < allTools.Count; i++)
        {
            if (i != selectedToolIndex)
                allTools[i].gameObject.SetActive(false);
            else
                allTools[i].gameObject.SetActive(true);
        }
    }

    public void UseTool()
    {
        if (useToolCooldown > 0)
            return;
        
        allTools[selectedToolIndex].UseTool();
    }

    public void SetUseToolCooldown(float newCooldown)
    {
        useToolCooldown = newCooldown;
    }
}
