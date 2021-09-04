using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnginesDriveFeedbackController : MonoBehaviour
{
    public Text textField;

    public ShipController _shipController;
    
    private void Update()
    {
        textField.text = "ENGINE POWER: " + _shipController.CurrentSpeed;
    }
}
