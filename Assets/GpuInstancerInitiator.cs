using System;
using System.Collections;
using System.Collections.Generic;
using GPUInstancer;
using UnityEngine;

public class GpuInstancerInitiator : MonoBehaviour
{
    public GPUInstancerPrefabManager prefabManager;

    private void Start()
    {
        if (prefabManager != null && prefabManager.gameObject.activeSelf && prefabManager.enabled)
        {
            //GPUInstancerAPI.RegisterPrefabInstanceList(prefabManager, asteroidInstances);
            GPUInstancerAPI.InitializeGPUInstancer(prefabManager);
        }
    }
}
