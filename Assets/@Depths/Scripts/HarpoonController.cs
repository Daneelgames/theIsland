using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;

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

    private ShipController ship;
    
    public void UseHarpoonInput(ShipController _ship)
    {
        if (_ship != null)
            ship = _ship;
        
        //ship.StopControllingShip();
        if (_ship.Use360Movement == false)
            PlayerMovement.instance.PlayerControlsShip(null);

        switch (state)
        {
            case State.Idle:
                // lock player's movement
                // move player's head to cameraParent
                PlayerMovement.instance.PlayerControlsHarpoon(this);

                // StartControlling the gun
                harpoonControlCoroutine = StartCoroutine(HarpoonControlCoroutine());
                state = State.ControlledByPlayer;
                break;

            /*
            case State.ControlledByPlayer:
                // release player's head
                // unlock player's movement
                
                PlayerMovement.instance.transform.position = ship.playerSit.position;
                PlayerMovement.instance.PlayerControlsHarpoon(null);

                // Stop Controlling the gun
                if (harpoonControlCoroutine != null)
                {
                    StopCoroutine(harpoonControlCoroutine);
                    harpoonControlCoroutine = null;
                }

                state = State.Idle;
                break;*/
        }
    }

    Quaternion newLocalRotation = Quaternion.identity;
    IEnumerator HarpoonControlCoroutine()
    {
        float newMinXRotation = minXRotation;
        float newMaxXRotation = maxXRotation;
        float newMinYRotation = minYRotation;
        float newMaxYRotation = maxYRotation;
        float newRotationSpeedScaler = rotationSpeedScaler;
        
        while (true)
        {
            yield return null;

            /*
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ship.TryToUseHarpoon(this);
            }
            */
            
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
                newProjectile.transform.rotation = shotHolder.rotation;
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
