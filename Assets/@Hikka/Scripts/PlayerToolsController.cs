using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerToolsController : MonoBehaviour
{
    public List<ToolController> allTools;
    public static PlayerToolsController instance;
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
    public void SelectTool(ToolController newSelectedTool)
    {
        for (int i = 0; i < allTools.Count; i++)
        {
            if (allTools[i] != newSelectedTool)
                allTools[i].gameObject.SetActive(false);
            else
            {
                selectedToolIndex = i;
                allTools[i].gameObject.SetActive(true);
            }
        }
    }

    public void UseTool()
    {
        if (useToolCooldown > 0)
        {
            return;
        }

        if (allTools[selectedToolIndex].toolType != ToolController.ToolType.Water)
            allTools[selectedToolIndex].UseTool();
        else
            allTools[selectedToolIndex].StartPumpingWater();
    }

    public void SetUseToolCooldown(float newCooldown)
    {
        useToolCooldown = newCooldown;
        StartCoroutine(ResetToolCooldown());
    }

    IEnumerator ResetToolCooldown()
    {
        yield return new WaitForSeconds(useToolCooldown);
        useToolCooldown = 0;
    }

    public void CantUseToolFeedback()
    {
        
    }

    public void FireButtonUp()
    {
        if (allTools[selectedToolIndex].toolType == ToolController.ToolType.Water && allTools[selectedToolIndex].waterToUse > 0)
        {
            allTools[selectedToolIndex].UseTool();   
        }
    }
}
