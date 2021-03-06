 using System;
using System.Collections;
using PlayerControls;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public class MouseLook : MonoBehaviour
{
    public Transform playerBody;

    float mouseX = 0;
    float mouseY = 0;

    float xRotation = 0;
    float yRotation = 0;

    public float mouseSensitivity = 10;
    public float mouseLookSpeedCurrent = 10;

    public float cameraFovIdle = 60;
    public float cameraFovAim = 30;
    [HideInInspector]    
    public float cameraFovIdleInit = 60;
    [HideInInspector]
    public float cameraFovAimInit = 30;

    public bool canControl = false;

    [SerializeField]
    GameObject camHolder;
    public Camera mainCamera;
    [SerializeField]
    Camera handsCamera;

    float cameraFov = 60;
    [SerializeField]
    Transform targetRotation;

    public float hookshotDistance = 50;
    public LayerMask aimLayerMask;
    
    public bool aiming = false;
    public bool canAim = true;
    public Animator activeWeaponHolderAnim;
    //public MeshRenderer crosshair;

    PlayerMovement pm;
    public Transform hookCrosshairTransform;

    private bool crouching = false; 
    
    public static MouseLook instance;

    private Coroutine crouchCoroutine;

    private string aimString = "Aim";
    private string aimingString = "Aiming";
    private string mouseXstring = "Mouse X";
    private string mouseYstring = "Mouse Y";

    private void Awake()
    {
        instance = this;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        pm = PlayerMovement.instance;

        cameraFovIdleInit = cameraFovIdle;
        cameraFovAimInit = cameraFovAim;
    }

    public void ToggleCrouch(bool crouch)
    {
        
        if (crouch && !crouching)
        {
            crouching = crouch;
            if (crouchCoroutine != null)
                StopCoroutine(crouchCoroutine);
            
            crouchCoroutine = StartCoroutine(Crouch(2));

        }
        else if (!crouch && crouching)
        {
            crouching = crouch;
            if (crouchCoroutine != null)
                StopCoroutine(crouchCoroutine);

            crouchCoroutine = StartCoroutine(Crouch(4));
        }
    }

    IEnumerator Crouch(float newHeight)
    {
        float t = 0;
        float time = 1f;

        while (t < time)
        {
            camHolder.transform.localPosition = Vector3.Lerp(camHolder.transform.localPosition,Vector3.up * newHeight, t / time);
            transform.localPosition = Vector3.Lerp(transform.localPosition,Vector3.up * newHeight, t / time);
            t += Time.deltaTime;
            yield return null;
        }
    }

    void Update()
    {
        if (canControl  && canAim &&!pm.teleport)
        {
            Aiming();   
            //HandleHookshotStart();
        }
    }

    private void LateUpdate()
    {
        if (canControl  && canAim &&!pm.teleport)
        {
            Looking();
        }
    }

    void Aiming()
    {
        
        if (Math.Abs(Input.GetAxisRaw(aimString)) > 0.1f)
            aiming = true;
        else if (Input.GetButton(aimString))
            aiming = true;
        else
            aiming = false;

        if (aiming)
        {
            cameraFov = Mathf.Lerp(cameraFov, cameraFovAim, 3 * Time.deltaTime);
        }
        else
        {
            cameraFov = Mathf.Lerp(cameraFov, cameraFovIdle, 3 * Time.deltaTime);
        }

        mainCamera.fieldOfView = cameraFov;
        handsCamera.fieldOfView = cameraFov;
        
        if (activeWeaponHolderAnim)
            activeWeaponHolderAnim.SetBool(aimingString, aiming);
    }

    void Looking()
    {
        mouseX = Input.GetAxis(mouseXstring) * mouseSensitivity;
        mouseY = Input.GetAxis(mouseYstring) * mouseSensitivity;

        xRotation -= mouseY;
        yRotation += mouseX;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        targetRotation.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation.localRotation, Time.deltaTime * mouseLookSpeedCurrent);
        camHolder.transform.localRotation = Quaternion.Slerp(camHolder.transform.localRotation, transform.localRotation, Time.deltaTime * mouseLookSpeedCurrent);
        pm.movementTransform.transform.rotation = camHolder.transform.rotation;
    }

    public void Recoil()
    {
        targetRotation.localRotation = Quaternion.Euler(Random.Range(-50,50), Random.Range(-50, 50), 0);
    }
}