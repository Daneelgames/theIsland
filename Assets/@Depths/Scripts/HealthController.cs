using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using Polarith.AI.Package;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class HealthController : MonoBehaviour
{
    public bool player = false;
    public string unitName;
    public float healthCurrent = 100;
    public float healthMax = 100;
    public List<MobBodyPart> mobBodyParts;
    public GameObject deathParticles;

    [SerializeField] private Rigidbody rb;
    public bool AddExplosiveForceOnDamage = false;
    public float explosivePowerOnDamage = 100;
    public float explosiveRaidusOnDamage = 50;

    public ShipController shipController;
    public SpaceshipController spaceShipController;
    
    [Header("Audio")]
    public AudioSource damagedAu;
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
        
        if (!player)
            Destroy(gameObject);
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}
