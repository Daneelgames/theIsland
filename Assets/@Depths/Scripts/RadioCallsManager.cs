using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioCallsManager : MonoBehaviour
{
    public static RadioCallsManager instance;
    
    public RadioCallController radioCallController;

    private RadioCallData currentCall;
    
    int currentPhrase = 0;

    private bool canPlayNewPhrase = true;
    private float playNewPhraseCooldown = 0.5f;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        radioCallController.SetMessage(null);
    }
    
    public void RadioCallToPlay(RadioCallData radioCall)
    {
        currentCall = radioCall;
        currentPhrase = -1;
        StartCoroutine(PlayNewPhrase());
    }

    public void Interact()
    {
        if (canPlayNewPhrase)
            StartCoroutine(PlayNewPhrase());
    }

    IEnumerator PlayNewPhrase()
    {
        currentPhrase++;
        if (currentPhrase >= currentCall.messagess.Count)
        {
            radioCallController.SetMessage(null);
        }
        else
        {
            radioCallController.SetMessage(currentCall.messagess[currentPhrase]);   
        }

        canPlayNewPhrase = false;
        
        float t = 0;
        while (t < playNewPhraseCooldown)
        {
            t += Time.deltaTime;
            yield return null;   
        }

        canPlayNewPhrase = true;
    }
}
