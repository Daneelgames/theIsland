using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private PlayerMovement pm;

    private float currentDistanceToPlayer = 0;
    public float reactionDistance = 50;
    public float attackDistance = 50;
    public WeaponHandler weaponHandler;
    private bool alive = true;
    
    public Collider coll;
    public Rigidbody rb;
    public float followSpeed;

    [Range(0, 1)] public float playerHpHeal = 0.5f;
    void Start()
    {
        pm = PlayerMovement.instance;
        //transform.localScale = Vector3.one;
    }
    
    void OnEnable()
    {
        StartCoroutine(GetDistanceToPlayer());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator GetDistanceToPlayer()
    {
        while (true && alive)
        {
            pm = PlayerMovement.instance;
            if (pm)
                currentDistanceToPlayer = Vector3.Distance(transform.position, pm.transform.position);
            yield return new WaitForSeconds(1);
            
            if (!pm)
                continue;
            
            if (currentDistanceToPlayer <= attackDistance)
            {
                transform.LookAt(pm.cameraAnimator.transform.position);
                weaponHandler.StartAttacking();
                rb.velocity = (pm.transform.position - transform.position).normalized * followSpeed;
            }
            else
            {
                /*
                if (currentDistanceToPlayer <= reactionDistance)
                {
                    rb.velocity = (pm.transform.position - transform.position).normalized * followSpeed;
                    transform.LookAt(pm.cameraAnimator.transform.position);
                }
                */
                rb.velocity = Vector3.zero;
                weaponHandler.StopAttacking();
            }
        }
        
        weaponHandler.StopAttacking();
    }

    public void KilledByPlayer()
    {
    }

    IEnumerator FlyFromPlayer()
    {
        yield return new WaitForSeconds(0.1f);
        gameObject.layer = 11;
        transform.parent = null;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddExplosionForce(3000, pm.transform.position, 200);
    }
}
