using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantTestGrowController : MonoBehaviour
{
    public List<ProceduralPlant> plants;

    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            foreach (var plant in plants)
            {
                StartCoroutine(plant.NextGrowthStep());
            }
        }
        
        if (Input.GetButtonDown("Reset"))
        {
            foreach (var plant in plants)
            {
                plant.ResetPlant();
            }
        }
    }
}
