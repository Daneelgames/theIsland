using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    public enum ToolType
    {
        Seed, Water, Gold, Blade
    }
    public ToolType toolType = ToolType.Seed;
    public float useToolCooldown = 1f;

    public int inventoryIndex = -1;
    [Header("SEED")] public PlantData plantData;

    [Header("WATER")] public ToolData toolData;
    public Transform watersackTrasform;
    public float watersackScaleSpeed = 0.5f;
    public float waterPumpingSpeed = 1;
    public float waterAmount = 0;
    public float waterToUse = 0;
    public int waterToUseMax = 3;
    public GameObject waterFx;
    
    private Vector3 watersackOriginalScale = Vector3.one;

    private InteractiveObject selectedObject;

    public Animator anim;
    private Coroutine pumpingWaterCoroutine;
    private static readonly int UseBlade = Animator.StringToHash("UseBlade");
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
                
                
                ReleaseWater();
                waterAmount -= _waterToUse;
                
                if (waterAmount < 0)
                    waterAmount = 0;
                
                if (selectedObject && selectedObject.plantController && selectedObject.plantController.GetSpawnedPlantVisual() != null)
                {
                    selectedObject.plantController.WaterUsed(_waterToUse);
                }
                break;
            
            case ToolType.Seed:
                
                if (selectedObject && selectedObject.plantController && selectedObject.plantController.GetSpawnedPlantVisual() != null)
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
            
            case ToolType.Blade:
                var selectedPlant = PlayerInteractionController.instance.SelectedPlantPart; 
                // FOR TEST
                if (selectedPlant) // AND SELECTED TOOL
                {
                    selectedPlant.MasterPlant.RemovePlantPart(selectedPlant);
                }

                anim.SetTrigger(UseBlade);
                break;
        }
    }


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
            watersackTrasform.localScale += watersackTrasform.localScale * watersackScaleSpeed * Time.deltaTime; 
            yield return null;
        }
    }

    public void ReleaseWater()
    {
        watersackTrasform.localScale = watersackOriginalScale; 
        waterFx.SetActive(true);
        anim.SetBool(ToolCharging, false);
    }
}
