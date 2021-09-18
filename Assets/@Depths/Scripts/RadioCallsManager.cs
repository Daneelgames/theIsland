using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioCallsManager : MonoBehaviour
{
    public ShipController shipController;
    public bool playerShip = false;
    public static RadioCallsManager playerShipInstance;
    
    public RadioCallController radioCallController;

    private RadioCallData currentCall;
    private RadioCallStartTrigger triggerToDestroy;

    
    int currentPhrase = 0;

    private bool canPlayNewPhrase = true;
    private float playNewPhraseCooldown = 0.5f;
    private void Awake()
    {
        if (playerShip)
            playerShipInstance = this;
    }

    void Start()
    {
        radioCallController.SetMessage(null);
    }

    public void RadioCallToPlay(RadioCallData radioCall, RadioCallStartTrigger _triggerToDestroy)
    {
        currentCall = radioCall;
        triggerToDestroy = _triggerToDestroy;
        currentPhrase = -1;
        StartCoroutine(PlayNewPhrase());
    }

    public void Interact()
    {
        if (currentCall != null && canPlayNewPhrase)
            StartCoroutine(PlayNewPhrase());
        else if (currentCall == null && shipController.radar)
        {
            // TALK TO SHIP
        }
    }

    IEnumerator PlayNewPhrase()
    {
        currentPhrase++;
        if (currentPhrase >= currentCall.messagess.Count)
        {
            radioCallController.SetMessage(null);
            
            if (currentCall.spawnQuestEventOnEnd != null && currentCall.spawnQuestEventOnEnd.RuntimeKeyIsValid() /* && currentCall.spawnQuestEventOnEnd.IsValid()*/)
            {
                AssetSpawner.instance.Spawn(currentCall.spawnQuestEventOnEnd, Vector3.zero, Quaternion.identity, AssetSpawner.ObjectType.QuestEvent);
            }

            if (triggerToDestroy != null)
            {
                Destroy(triggerToDestroy.gameObject);
                triggerToDestroy = null;
            }

            currentCall = null;
        }
        else
        {
            radioCallController.SetMessage(currentCall.messagess[currentPhrase]);   
            if (currentCall.startSpawningMobsPhraseIndex == currentPhrase)
            {
                MobSpawnManager.instance.SetMaxMobsAlive(currentCall.maxAliveMobsToSet);
                
                MobSpawnManager.instance.StartSpawningMobs();
            }
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