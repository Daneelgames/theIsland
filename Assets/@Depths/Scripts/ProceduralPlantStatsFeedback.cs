using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.UI;

public class ProceduralPlantStatsFeedback : MonoBehaviour
{
    public GameObject canvas;
    
    public Text nameText;
    public Text healthText;
    public Text ageText;

    void Start()
    {
        nameText.text = String.Empty;
        healthText.text = String.Empty;
        ageText.text = String.Empty;

        StartCoroutine(GetDistanceToPlayer());
    }

    IEnumerator GetDistanceToPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(1);
            if (Vector3.Distance(transform.position, PlayerMovement.instance.transform.position) < 1f)
            {
                if (!canvas.gameObject.activeInHierarchy)
                    canvas.gameObject.SetActive(true);
            }
            else
            {
                if (canvas.gameObject.activeInHierarchy)
                    canvas.gameObject.SetActive(false);
            }
        }
    }
    
    public void UpdateText(ProceduralPlant plant)
    {
        nameText.text = plant.plantData.plantName[GameManager.instance.gameLanguage].ToUpper();
        healthText.text = "HEALTH: " + plant.CurrentHealth;
        ageText.text = "AGE: " + plant.CurrentAge;
    }

    private void Update()
    {
        canvas.transform.LookAt(MouseLook.instance.mainCamera.transform.position);
    }
}
