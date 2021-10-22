using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HealthController : MonoBehaviour
{
    public bool player = false;
    
    public enum Fraction
    {
        Humans, Mermaids, Fish, Plant
    }

    public Fraction fraction = Fraction.Humans;
    public string unitName;
    public float healthCurrent = 100;
    public float healthMax = 100;
    public List<MobBodyPart> mobBodyParts;
    public GameObject deathParticles;

    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    public bool AddExplosiveForceOnDamage = false;
    public float explosivePowerOnDamage = 100;
    public float explosiveRaidusOnDamage = 50;

    [Header("AI")]
    public ShipController shipController;
    public AstarWalker astarWalker;
    
    [Header("Audio")]
    public AudioSource damagedAu;

    [Header("Drop")] public List<InteractiveObject> interactiveObjectsToDrop;
    void Start()
    {
        healthMax = healthCurrent;
        MobSpawnManager.instance.AddUnit(this);
    }

    public Rigidbody GetRigidbody
    {
        get { return rb; }
    }

        [ContextMenu("GetBodyParts")]
    public void GetBodyParts()
    {
        rb = gameObject.GetComponent<Rigidbody>();
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
        if (healthCurrent <= 0)
            return;
        
        healthCurrent -= damage;

        if (damagedAu)
        {
            damagedAu.pitch = Random.Range(0.75f, 1.25f);
            damagedAu.Play();   
        }

        if (AddExplosiveForceOnDamage)
        {
            rb.AddExplosionForce(explosivePowerOnDamage, transform.position + new Vector3(Random.Range(-5,5),Random.Range(-5,5),Random.Range(-5,5)), explosiveRaidusOnDamage);
        }
        
        if (player)
            PlayerMovement.instance.ControlledShipDamaged();
        
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

        for (int i = 0; i < interactiveObjectsToDrop.Count; i++)
        {
            var newDrop = Instantiate(interactiveObjectsToDrop[i], transform.position, transform.rotation);
            newDrop.rb.AddExplosionForce(25, transform.position + Random.onUnitSphere, 30);
        }
        
        if (!player)
            Destroy(gameObject);
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
