using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddExplosionForceToRigidbodies : MonoBehaviour
{
    public float explosionRadius = 100;
    public float startPower = 100;
    private void Start()
    {
        List<Rigidbody> activeRbs = new List<Rigidbody>(MobSpawnManager.instance.ActiveRigidbodies);
        float distance = 0;
        
        //float powerScaler = 1;
        
        for (int i = 0; i < activeRbs.Count; i++)
        {
            distance = Vector3.Distance(transform.position, activeRbs[i].transform.position);
            if (distance > explosionRadius)
                continue;

            /*
            powerScaler = 1 - explosionRadius / distance;
            activeRbs[i].AddExplosionForce(startPower * powerScaler, transform.position, explosionRadius);
            */
            
            activeRbs[i].AddExplosionForce(startPower, transform.position, explosionRadius);
        }
    }
}