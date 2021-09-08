using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class ShipHealthFeedback : MonoBehaviour
{
    public ShipController ship;
    public Text textField;
    
    IEnumerator Start()
    {
        float healthCoeff = 1;
        
        while (true)
        {
            healthCoeff = ship.hc.healthCurrent / ship.hc.healthMax;
            
            if (healthCoeff > 0.95f)
            {
                textField.text = "NOT DAMAGED";
                textField.color = Color.green;
            }
            else if (healthCoeff > 0.7f)
            {
                textField.text = "LIGHT DAMAGES";
                textField.color = new Color(0.65f, 1f, 0f);
            } 
            else if (healthCoeff > 0.5f)
            {
                textField.text = "MEDIUM DAMAGES";
                textField.color = new Color(1f, 0.77f, 0f);
            } 
            else if (healthCoeff > 0.3f)
            {
                textField.text = "HEAVY DAMAGES";
                textField.color = new Color(1f, 0.24f, 0f);
            } 
            else
            {
                textField.text = "DANGER";
                textField.color = new Color(0.81f, 0.05f, 0f);
            } 
            yield return new WaitForSeconds(0.1f);
        }
    }
}
