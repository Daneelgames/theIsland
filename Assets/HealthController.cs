using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public int healthCurrent = 100;
    public GameObject deathParticles;
    public void Damage(int damage)
    {
        healthCurrent -= damage;

        if (healthCurrent <= 0)
        {
            Death();
        }
    }

    public void Death()
    {
        MobSpawnManager.instance.MobKilled(this);
        Instantiate(deathParticles, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
