using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiController : MonoBehaviour
{
    public List<Image> staminaBars;
    void Update()
    {
        for (int i = 0; i < staminaBars.Count; i++)
        {
            staminaBars[i].fillAmount = PlayerMovement.instance.staminaStats.CurrentValue /
                                        PlayerMovement.instance.staminaStats.MaxValue;
        }
    }
}
