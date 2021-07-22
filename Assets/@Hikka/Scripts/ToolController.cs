using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    public enum ToolType
    {
        Seed, Water, Gold
    }

    public ToolType toolType = ToolType.Seed;
    public float useToolCooldown = 1f;

    public int seedIndex = -1;
    public void UseTool()
    {
        PlayerToolsController.instance.SetUseToolCooldown(useToolCooldown);

        switch (toolType)
        {
            case ToolType.Seed:
                var selectedObject = PlayerUiController.instance.GetSelectedObject();
                if (selectedObject && selectedObject.plantController)
                {
                    selectedObject.plantController.PlantSeed(seedIndex);
                    int newSeedsAmount = PlayerInventoryController.instance.SeedUsed(seedIndex);

                    if (newSeedsAmount <= 0)
                    {
                        PlayerUiController.instance.SetSelectedItemIndexOnWheel(-1);
                        gameObject.SetActive(false);   
                    }
                }
                break;
        }
    }
}
