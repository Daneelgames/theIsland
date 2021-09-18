using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using Random = UnityEngine.Random;

public class HarpoonController : MonoBehaviour
{
    [SerializeField] Transform _cameraParent;
    [SerializeField] private ProjectileController projectile;

    enum State
    {
        Idle,
        ControlledByPlayer
    }

    public Transform CameraParent
    {
        get { return _cameraParent; }
    }

    private State state = State.Idle;

    [SerializeField] float attackCooldown = 1f;
    [SerializeField] float attackCooldownCurrent = 0f;

    [SerializeField] float rotationSpeedScaler = 0.5f;
    
    private Coroutine harpoonControlCoroutine;

    private float mouseX = 0;
    private float mouseY = 0;
    private string mouseXstring = "Mouse X";
    private string mouseYstring = "Mouse Y";
    float xRotation = 0;
    float yRotation = 0;
    
    [SerializeField]List<ProjectileController> spawnedProjectiles = new List<ProjectileController>();

    [SerializeField] float minXRotation = -60;
    [SerializeField] float maxXRotation = 60;
    [SerializeField] float minYRotation = -90;
    [SerializeField] float maxYRotation = 90;

    public Transform shotHolder;

    public AudioSource attackAu;
    public AudioSource reloadAu;

    public float delayOnStartControlling = 0.5f;
    
    private ShipController ship;
    
    [Header("Laser Spot Settings")]
    public GameObject laserSpot;
    public LayerMask laserSpotLayerMask;
    public float lasetSpotSpeed = 5;

    private Vector3 laserSpotDefaultLocalScale;
    private RaycastHit hit;
    private Coroutine UpdateLasetSpotCoroutine;

    private void Start()
    {
        if (laserSpot)
        {
            laserSpotDefaultLocalScale = laserSpot.transform.localScale;
        }
    }

    public void UseHarpoonInput(ShipController _ship)
    {
        if (_ship != null)
        {
            ship = _ship;
            
            if (_ship.Use360Movement == false)
                PlayerMovement.instance.PlayerControlsShip(null);

            
            if (laserSpot)
                UpdateLasetSpotCoroutine = StartCoroutine(UpdateLaserSpot());
                
            // lock player's movement
            // move player's head to cameraParent
            PlayerMovement.instance.PlayerControlsHarpoon(this);

            // StartControlling the gun
            if (harpoonControlCoroutine != null)
                StopCoroutine(harpoonControlCoroutine);
                
            harpoonControlCoroutine = StartCoroutine(HarpoonControlCoroutine());
            state = State.ControlledByPlayer;
        }
        else
        {
            if (laserSpot && UpdateLasetSpotCoroutine != null)
            {
                StopCoroutine(UpdateLasetSpotCoroutine);
                UpdateLasetSpotCoroutine = null;
                laserSpot.transform.localScale = Vector3.zero;
                laserSpot.transform.position = shotHolder.position;
            }
                
            // release player's head
            // unlock player's movement
            //PlayerMovement.instance.transform.position = ship.playerSit.position;
            PlayerMovement.instance.PlayerControlsHarpoon(null);

            // Stop Controlling the gun
            if (harpoonControlCoroutine != null)
            {
                StopCoroutine(harpoonControlCoroutine);
                harpoonControlCoroutine = null;
            }

            state = State.Idle;
        }
    }

    IEnumerator UpdateLaserSpot()
    {
        while (true)
        {
            //if (Physics.Raycast(shotHolder.position, shotHolder.forward, out hit,100,  laserSpotLayerMask))
            if (MouseLook.instance.aiming == false && Physics.Raycast(MouseLook.instance.mainCamera.transform.position, MouseLook.instance.playerCursor.transform.position - MouseLook.instance.mainCamera.transform.position, out hit,100,  laserSpotLayerMask))
            {
                laserSpot.transform.position = Vector3.Lerp(laserSpot.transform.position, hit.point, lasetSpotSpeed * Time.deltaTime);
                if (Vector3.Distance(laserSpot.transform.position, MouseLook.instance.mainCamera.transform.position) < 3)
                    laserSpot.transform.localScale = Vector3.Lerp(laserSpot.transform.localScale, Vector3.zero, lasetSpotSpeed * Time.deltaTime);
                else
                    laserSpot.transform.localScale = Vector3.Lerp(laserSpot.transform.localScale, laserSpotDefaultLocalScale, lasetSpotSpeed * Time.deltaTime);
                laserSpot.transform.LookAt(MouseLook.instance.transform.position);
            }
            yield return null;
        }
    }
    
    Quaternion newLocalRotation = Quaternion.identity;
    IEnumerator HarpoonControlCoroutine()
    {
        yield return new WaitForSeconds(delayOnStartControlling);
        
        float newMinXRotation = minXRotation;
        float newMaxXRotation = maxXRotation;
        float newMinYRotation = minYRotation;
        float newMaxYRotation = maxYRotation;
        float newRotationSpeedScaler = rotationSpeedScaler;
        
        while (true)
        {
            yield return null;

            
            if (MouseLook.instance.aiming)
                continue;
            
            if (PlayerUiController.instance.itemWheelVisible == false)
            {
                mouseX = Input.GetAxis(mouseXstring) * MouseLook.instance.mouseSensitivity;
                mouseY = Input.GetAxis(mouseYstring) * MouseLook.instance.mouseSensitivity;

                xRotation -= mouseY;
                yRotation += mouseX;


                if (ship._state == ShipController.State.ControlledByPlayer)
                {
                    newMinXRotation = minXRotation;
                    newMaxXRotation = maxXRotation;
                    newMinYRotation = minYRotation;
                    newMaxYRotation = maxYRotation;
                    newRotationSpeedScaler = rotationSpeedScaler;
                }
                else if (ship._state == ShipController.State.Idle)
                {
                    newMinXRotation = minXRotation * 3;
                    newMaxXRotation = maxXRotation * 3;
                    newMinYRotation = minYRotation * 3;
                    newMaxYRotation = maxYRotation * 3;
                    newRotationSpeedScaler = rotationSpeedScaler * 3;
                }
                
                
                xRotation = Mathf.Clamp(xRotation, newMinXRotation, newMaxXRotation);
                yRotation = Mathf.Clamp(yRotation, newMinYRotation, newMaxYRotation);
            }

            newLocalRotation = Quaternion.Euler(xRotation, yRotation, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, newLocalRotation, newRotationSpeedScaler * Time.smoothDeltaTime);
            
            if (attackCooldownCurrent > 0)
                continue;

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                attackAu.Stop();
                attackAu.pitch = Random.Range(0.75f, 1.25f);
                attackAu.Play();
                
                StartCoroutine(AttackCooldown());
                GetNewProjectile();
            }
        }
    }

    void GetNewProjectile()
    {
        ProjectileController newProjectile = null;
        for (int i = 0; i < spawnedProjectiles.Count; i++)
        {
            if (spawnedProjectiles[i].gameObject.activeInHierarchy == false)
            {
                newProjectile = spawnedProjectiles[i];
                newProjectile.transform.position = shotHolder.position;
                //newProjectile.transform.rotation = shotHolder.rotation;
                
                newProjectile.transform.LookAt(laserSpot.transform.position);
                
                newProjectile.rb.velocity = Vector3.zero;
                newProjectile.rb.angularVelocity = Vector3.zero;
                newProjectile.rb.Sleep();
                newProjectile.gameObject.SetActive(true);
                break;
            }
        }

        if (newProjectile == null)
        {
            newProjectile = Instantiate(projectile, shotHolder.position, shotHolder.rotation);
            spawnedProjectiles.Add(newProjectile);   
        }
    }

IEnumerator AttackCooldown()
    {
        attackCooldownCurrent = attackCooldown;
        
        reloadAu.Stop();
        reloadAu.pitch = Random.Range(0.75f, 1.25f);
        reloadAu.Play();
        
        yield return new WaitForSeconds(attackCooldown);
        
        attackCooldownCurrent = 0;
        
        reloadAu.Stop();
    }
}
