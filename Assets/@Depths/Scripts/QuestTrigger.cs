using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Experimental.GlobalIllumination;

public class QuestTrigger : MonoBehaviour
{
    public QuestLine questLineToStartOnTrigger;
    public AssetReference questEventToSpawn;

    private float cooldown = 0.5f;
    private float cooldownCurrent = 0f;

    public float delayBeforeActivating = 0;
    
    bool activated = false;

    private void Start()
    {
        transform.parent = null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (cooldownCurrent < 0 || activated)
            return;
        
        StartCoroutine(Cooldown());
        
        if (other.gameObject == PlayerMovement.instance.gameObject)
        {
            StartCoroutine(Activate());
        }
    }
    
    public IEnumerator Activate()
    {
        activated = true;
        StopAllCoroutines();

        yield return new WaitForSeconds(delayBeforeActivating);
        
        if (questLineToStartOnTrigger)
            QuestLinesManager.instance.StartQuestLine(questLineToStartOnTrigger);
        if (questEventToSpawn != null)
            AssetSpawner.instance.Spawn(questEventToSpawn, Vector3.zero, Quaternion.identity, AssetSpawner.ObjectType.QuestEvent);
            
        Destroy(gameObject);
    }

    IEnumerator Cooldown()
    {
        cooldownCurrent = cooldown;
        yield return new WaitForSeconds(cooldownCurrent);
        cooldownCurrent = 0;
    }
}
