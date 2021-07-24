using System;
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

    public int inventoryIndex = -1;
    [Header("SEED")] public PlantData plantData;

    [Header("WATER")] public ToolData toolData;
    public Transform watersackTrasform;

    private Vector3 watersackOriginalScale = Vector3.one;
    public float waterPumpingSpeed = 1;
    public float waterAmount = 0;
    public float waterToUse = 0;
    public int waterToUseMax = 3; 

    private InteractiveObject selectedObject;

    public Animator anim;
    private static readonly int ToolCharging = Animator.StringToHash("ToolCharging");

    public void Awake()
    {
        if (watersackTrasform)
            watersackOriginalScale = watersackTrasform.localScale;
    }

    public void UseTool()
    {
        PlayerToolsController.instance.SetUseToolCooldown(useToolCooldown);

        selectedObject = PlayerUiController.instance.GetSelectedObject();
        
        switch (toolType)
        {
            case ToolType.Water:

                if (pumpingWaterCoroutine != null)
                {
                    StopCoroutine(pumpingWaterCoroutine);
                    pumpingWaterCoroutine = null;
                    anim.SetBool(ToolCharging, false);
                }
                
                var _waterToUse = waterToUse;
                waterToUse = 0;
                
                if (waterAmount < 0)
                    waterAmount = 0;
                
                if (_waterToUse > waterAmount)
                {
                    PlayerToolsController.instance.CantUseToolFeedback();
                    return;
                }

                ReleaseWater();

                waterAmount -= _waterToUse;
                
                if (selectedObject && selectedObject.plantController && selectedObject.plantController.spawnedPlantVisual)
                {
                    // selectedObject.plantController.WaterUsed(_waterToUse);
                }
                break;
            
            case ToolType.Seed:
                
                if (selectedObject.plantController && selectedObject.plantController.spawnedPlantVisual != null)
                {
                    PlayerToolsController.instance.CantUseToolFeedback();
                    return;
                }
                if (selectedObject && selectedObject.plantController)
                {
                    selectedObject.plantController.PlantSeed(inventoryIndex);
                    int newSeedsAmount = PlayerInventoryController.instance.SeedUsed(inventoryIndex);

                    if (newSeedsAmount <= 0)
                    {
                        PlayerUiController.instance.SetSelectedItemIndexOnWheel(-1);
                        gameObject.SetActive(false);   
                    }
                }
                break;
        }
    }

    private Coroutine pumpingWaterCoroutine;

    public void StartPumpingWater()
    {
        PlayerToolsController.instance.SetUseToolCooldown(useToolCooldown);
        
        if (pumpingWaterCoroutine != null)
            return;

        pumpingWaterCoroutine = StartCoroutine(PumpingWater());
    }

    IEnumerator PumpingWater()
    {
        float waterToSpend = 0;
        anim.SetBool(ToolCharging, true);
        while (waterAmount > 0 && waterToUse <= waterToUseMax)
        {
            waterToSpend = Time.deltaTime * waterPumpingSpeed;
            waterAmount -= waterToSpend;
            waterToUse += waterToSpend;
            watersackTrasform.localScale += watersackTrasform.localScale * Time.deltaTime; 
            yield return null;
        }
    }

    public void ReleaseWater()
    {
        watersackTrasform.localScale = watersackOriginalScale; 
        anim.SetBool(ToolCharging, false);
    }
}
