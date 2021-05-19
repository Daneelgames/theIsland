using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    public int shotsInAttack = 5;
    public float delayBetweenShots = 0.1f;
    public float delayBetweenAttacks = 1f;
        
    private Coroutine attackingCoroutine;

    public Transform shotPosition;

    private PlayerMovement pm;
    public LayerMask playerLayerMask;
    
    public void StartAttacking()
    {
        if (attackingCoroutine == null)
        {
            pm = PlayerMovement.instance;
            attackingCoroutine = StartCoroutine(Attacking());   
        }
    }

    public void StopAttacking()
    {
        if (attackingCoroutine != null)
        {
            StopCoroutine(attackingCoroutine);
            attackingCoroutine = null;
        }
    }

    IEnumerator Attacking()
    {
        RaycastHit hit;
        while (true)
        {
            if (Physics.Raycast(shotPosition.position, pm.transform.position - transform.position, out hit, 200, 
                playerLayerMask))
            {
                if (hit.collider.gameObject.layer == 9)
                {
                    for (int i = 0; i < shotsInAttack; i++)
                    {
                        var newProjectile = ObjectPooling.instance.GetBullet();
                        newProjectile.transform.position = shotPosition.position;
                        newProjectile.transform.LookAt(pm.cameraAnimator.transform.position);
                        yield return new WaitForSeconds(delayBetweenShots);   
                    }      
                } 
            }
            yield return new WaitForSeconds(delayBetweenAttacks);   
        }
    }
}
