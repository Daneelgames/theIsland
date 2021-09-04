using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class HealthController : MonoBehaviour
{
    public float healthCurrent = 100;
    public float healthMax = 100;
    public List<MobBodyPart> mobBodyParts;
    public GameObject deathParticles;

    void Start()
    {
        healthMax = healthCurrent;
    }
    
    [ContextMenu("GetBodyParts")]
    public void GetBodyParts()
    {
        mobBodyParts = new List<MobBodyPart>(GetComponentsInChildren<MobBodyPart>());
        for (int i = 0; i < mobBodyParts.Count; i++)
        {
            if (mobBodyParts[i] != null)
            {
                mobBodyParts[i].hc = this;
            }
        }
    }
    
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
        
        if (deathParticles)
            Instantiate(deathParticles, transform.position, transform.rotation);
        
        Destroy(gameObject);
    }
}
