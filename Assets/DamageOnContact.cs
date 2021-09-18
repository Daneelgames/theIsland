using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageOnContact : MonoBehaviour
{
    [Header("Used for taking damage by")]
    public HealthController hc;
    public Rigidbody rb;
    public float damageCooldown = 1;
    public int damageScaledByVelocity = 5;

    private Coroutine damageCoroutine;
    
    private void OnTriggerStay(Collider other)
    {
        if (damageCoroutine != null) 
            return;
        
        if (other.gameObject.layer == 6)
        {
            damageCoroutine = StartCoroutine(TakeDamage());
        }
    }

    IEnumerator TakeDamage()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            hc.Damage(Mathf.RoundToInt(damageScaledByVelocity * rb.velocity.magnitude));
            yield return new WaitForSeconds(damageCooldown);   
        }
        
        damageCoroutine = null;
    }
    
}
