using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using PlayerControls;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    public Rigidbody rb;
    public int damage = 100;
    public float projectileSpeed = 10;
    public float lifeTime = 3;

    public bool targetPlayer = true; 
    
    public ParticleSystem deathParticles;

    public float verticalGravityForce = 1;
    private bool collided = false;

    public Vector3 force;
    
    void FixedUpdate()
    {
        Debug.Log("rb velocity " + rb.velocity);
        if (collided)
        {
            return;
        }

        force = transform.forward * projectileSpeed - Vector3.up * verticalGravityForce;
        rb.AddForce(force, ForceMode.Force);
    }

    void OnEnable()
    {
        collided = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
        StartCoroutine(DeactivateProjectile());
        
        if (targetPlayer)
            transform.LookAt(PlayerMovement.instance.transform.position);
    }

    void OnDisable()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
        StopAllCoroutines();
    }

    void OnCollisionStay(Collision coll)
    {
        Debug.Log("collided " + collided + "; coll.name " + coll.gameObject.name);
        
        if (collided)
            return;

        if (coll.gameObject.layer == 7)
        {
            var hpToDamage = coll.gameObject.GetComponent<HealthController>();
            if (hpToDamage)
                hpToDamage.Damage(damage);
            collided = true;
        }
        
        if (coll.gameObject.layer == 6 || coll.gameObject.layer == 9 ||
            coll.gameObject.layer == 11)
        {
            collided = true;
        }
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
        collided = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
        gameObject.SetActive(false);
    }
}