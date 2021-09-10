using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LightsToggle : MonoBehaviour
{
    public GameObject lights;
    public Text textFeedbackField;

    public void ToggleLight()
    {
        bool active = !lights.activeInHierarchy;
        if (active)
            textFeedbackField.text = "LIGHTS ON";
        else
            textFeedbackField.text = "LIGHTS OFF";
        lights.SetActive(active);
    }
}
