using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeleeWeaponController : MonoBehaviour
{
    enum State
    {
        Idle,
        ControlledByPlayer
    }
    private State state = State.Idle;

    [SerializeField] private int damage = 300;
    [SerializeField] float attackCooldown = 1;
    float attackCooldownCurrent = 0;
    [SerializeField] bool dangerous = false;
    
    [Header("Attack Visual")]
    public Animator anim;
    public AudioSource attackAu;
    private static readonly int Attack1 = Animator.StringToHash("Attack");

    private ShipController ship;
    private float delayOnStartControlling = 0.5f;
    private Coroutine controlCoroutine;
    private List<HealthController> damagedHc = new List<HealthController>();
    public void UseWeaponInput(ShipController _ship)
    {
        if (_ship != null)
        {
            ship = _ship;
            
            // StartControlling the weapon
            if (controlCoroutine != null)
                StopCoroutine(controlCoroutine);
                
            controlCoroutine = StartCoroutine(ControlCoroutine());
            state = State.ControlledByPlayer;
        }
        else
        {
            // Stop Controlling the weapon
            if (controlCoroutine != null)
            {
                StopCoroutine(controlCoroutine);
                controlCoroutine = null;
            }

            state = State.Idle;
        }
    }

    IEnumerator ControlCoroutine()
    {
        yield return new WaitForSeconds(delayOnStartControlling);

        while (true)
        {
            yield return null;
            
            if (MouseLook.instance.aiming)
                continue;
            
            if (dangerous || attackCooldownCurrent > 0)
                continue;

            if (Input.GetKeyDown(KeyCode.R))
            {
                attackAu.Stop();
                attackAu.pitch = Random.Range(0.75f, 1.25f);
                attackAu.Play();
                
                StartCoroutine(Attack());
            }
        }
    }
    
    IEnumerator Attack()
    {
        damagedHc.Clear();
        anim.SetTrigger(Attack1);
        attackCooldownCurrent = attackCooldown;
        
        while (attackCooldownCurrent > 0)
        {
            attackCooldownCurrent -= Time.deltaTime;
            yield return null;
        }
    }

    public void SetDangerous(int i)
    {
        dangerous = i != 0;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (!dangerous)
            return;
        
        if (coll.gameObject.layer == 7)
        {
            if (coll.gameObject == ship.gameObject || coll.transform.parent == ship.transform)
                return;
            
            var hpToDamage = coll.gameObject.GetComponent<HealthController>();
            if (!hpToDamage)
            {
                var partToDamage = coll.gameObject.GetComponent<MobBodyPart>();
                if (partToDamage != null)
                    hpToDamage = partToDamage.hc;
            }
            
            if (damagedHc.Contains(hpToDamage))
                return;
            
            if (hpToDamage)
                hpToDamage.Damage(damage);
            
            damagedHc.Add(hpToDamage);
        }
    }
}