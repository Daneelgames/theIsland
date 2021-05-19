using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using PlayerControls;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Rigidbody rb;
    public float damage = 100;
    public float projectileSpeed = 10;
    public float lifeTime = 3;

    public bool targetPlayer = true; 
    
    public ParticleSystem deathParticles;

    void FixedUpdate()
    {
        rb.AddForce(transform.forward * projectileSpeed, ForceMode.Force);
    }

    void OnEnable()
    {
        rb.velocity = Vector3.zero;
        StartCoroutine(DeactivateProjectile());
        if (targetPlayer)
            transform.LookAt(PlayerMovement.instance.transform.position);
    }

    void OnDisable()
    {
        rb.velocity = Vector3.zero;
        StopAllCoroutines();
    }

    void OnTriggerStay(Collider coll)
    {
    }

    public void Deflect()
    {
        rb.velocity = Vector3.zero;
        projectileSpeed *= 5;
        transform.Rotate(0,180,0, Space.Self);
    }
    

    IEnumerator DeactivateProjectile()
    {
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }
}