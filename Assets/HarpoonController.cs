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
        
        PlayerMovement.instance.PlayerControlsShip(null);

        switch (state)
        {
            case State.Idle:
                // lock player's movement
                // move player's head to cameraParent
                PlayerMovement.instance.PlayerControlsHarpoon(this);

                // StartControlling the gun
                Debug.Log("UseHarpoonInput");
                harpoonControlCoroutine = StartCoroutine(HarpoonControlCoroutine());
                state = State.ControlledByPlayer;
                break;

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
                break;
        }
    }

    IEnumerator HarpoonControlCoroutine()
    {
        while (true)
        {
            yield return null;

            if (Input.GetKey(KeyCode.Space))
            {
                ship.TryToUseHarpoon(this);
            }

            if (PlayerUiController.instance.itemWheelVisible == false)
            {
                mouseX = Input.GetAxis(mouseXstring) * MouseLook.instance.mouseSensitivity;
                mouseY = Input.GetAxis(mouseYstring) * MouseLook.instance.mouseSensitivity;

                xRotation -= mouseY;
                yRotation += mouseX;
                xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
                yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);
            }

            var newLocalRotation = Quaternion.Euler(xRotation, yRotation, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, newLocalRotation, Time.smoothDeltaTime);

            if (attackCooldownCurrent > 0)
                continue;

            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.F))
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
