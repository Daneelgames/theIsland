using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobCloseAttack : MonoBehaviour
{
    public int damage = 50;
    bool searchForNewCollision = true;
    public float attackLenght = 3f;

    private bool dangerous = false;

    public Animator anim;

    private List<HealthController> damagedHC = new List<HealthController>();
    private Coroutine attackCoroutine;
    void OnCollisionStay(Collision coll)
    {
        if (coll.gameObject.layer == 7 || coll.gameObject.layer == 11)
        {
            if (dangerous)
                Damage(coll.gameObject);
            
            if (!searchForNewCollision)
                return;
            if (attackCoroutine == null)
                attackCoroutine = StartCoroutine(AttackCoroutine());
        }
    }

    void Damage(GameObject go)
    {
        for (var index = 0; index < damagedHC.Count; index++)
        {
            if (damagedHC[index] == null)
                continue;
            
            if (go == damagedHC[index].gameObject)
                return;
        }

        var hpToDamage = go.GetComponent<HealthController>();
        if (!hpToDamage)
        {
            var partToDamage = go.GetComponent<MobBodyPart>();
            if (partToDamage != null)
                hpToDamage = partToDamage.hc;
        }

        if (hpToDamage)
        {
            hpToDamage.Damage(damage);
            damagedHC.Add(hpToDamage);
        }

    }

    IEnumerator AttackCoroutine()
    {
        Debug.Log("AttackCoroutine() ");
        damagedHC.Clear();
        searchForNewCollision = false;
        anim.SetTrigger("Attack");
        yield return new WaitForSeconds(attackLenght);
        dangerous = false;
        searchForNewCollision = true;
        
        attackCoroutine = null;
    }

    public void DangerousTrue()
    {
        dangerous = true;
    }
    
    public void DangerousFalse()
    {
        dangerous = false;
    }
}
