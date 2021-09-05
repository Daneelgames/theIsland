using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioCallStartTrigger : MonoBehaviour
{
    public RadioCallData radioCallData;
    private void Start()
    {
        RadioCallsManager.instance.RadioCallToPlay(radioCallData);
    }
}
