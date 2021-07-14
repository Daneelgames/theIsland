using System.Collections;
using PlayerControls;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool activateLoopsLoadingTrigger = false;
    public bool working = false;
    
    [Header("Player Properties")]
    public Transform initialPlayerPosition;
    public float timeToMoveToInitialPos = 0.5f;
    public Transform finalPlayerPosition;
    public float timeToMoveToFinalPos = 1f;

    [Header("Door properties")] 
    public Collider doorCollider;

    public float timeToOpenTheDoor = 1f;
    public float timeToWait = 0.5f;
    public float timeToCloseTheDoor = 0.5f;
    public Vector3 doorEulerOpened;
    public Vector3 doorEulerClosed;

    [Header("Audio")] 
    public AudioSource au;
    public AudioClip openDoorClip;
    public AudioClip closeDoorClip;

    [ContextMenu("SaveDoorClosedRotation")]
    public void SaveDoorClosedRotation()
    {
        doorEulerClosed = doorCollider.transform.localEulerAngles;
    }

    [ContextMenu("SaveDoorOpenedRotation")]
    public void SaveDoorOpenedRotation()
    {
        doorEulerOpened = doorCollider.transform.localEulerAngles;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!working && other.gameObject == PlayerMovement.instance.gameObject)
        {
            working = true;
            if (activateLoopsLoadingTrigger)
                LoopsLoadingManager.instance.loadingTrigger.gameObject.SetActive(true);
            StartCoroutine(MovePlayerThroughDoor());
        }
    }

    IEnumerator MovePlayerThroughDoor()
    {
        PlayerMovement.instance.inControl = false;
        float t = 0;
        PlayerMovement.instance.controller.enabled = false;
        
        // MOVE THE PLAYER TO INITIAL POINT
        Vector3 _initialPlayerPosition = PlayerMovement.instance.transform.position;
        while ( t < timeToMoveToInitialPos)
        {
            t += Time.deltaTime;
            PlayerAudioController.instance.PlaySteps();
            PlayerMovement.instance.transform.position = Vector3.Lerp(_initialPlayerPosition, initialPlayerPosition.position, t / timeToMoveToInitialPos);
            yield return null;
        }

        StartCoroutine(OpenDoor());

        // MOVE THE PLAYER THROUGH THE DOOR
        t = 0;
        while ( t < timeToMoveToFinalPos)
        {
            t += Time.deltaTime;
            PlayerAudioController.instance.PlaySteps();
            PlayerMovement.instance.transform.position = Vector3.Lerp(initialPlayerPosition.position, finalPlayerPosition.position, t / timeToMoveToFinalPos);
            yield return null;
        }
        
        PlayerMovement.instance.controller.enabled = true;
        PlayerMovement.instance.inControl = true;
    }

    IEnumerator OpenDoor()
    {
        Vector3 currentDoorRotation = doorCollider.transform.localEulerAngles;
        doorCollider.enabled = false;
        
        PlayAudio(openDoorClip);
        
        // OPEN THE DOOR
        float t = 0;
        while ( t < timeToOpenTheDoor)
        {
            t += Time.smoothDeltaTime;
            currentDoorRotation = new Vector3(0, Mathf.LerpAngle(doorEulerClosed.y, doorEulerOpened.y, t / timeToOpenTheDoor), 0);
            doorCollider.gameObject.transform.localEulerAngles = currentDoorRotation;
            yield return null;
        }

        yield return new WaitForSeconds(timeToWait);
        
        // CLOSE THE DOOR
        t = 0;
        while ( t < timeToCloseTheDoor)
        {
            t += Time.smoothDeltaTime;
            currentDoorRotation = new Vector3(0, Mathf.LerpAngle(doorEulerOpened.y, doorEulerClosed.y, t / timeToCloseTheDoor), 0);
            doorCollider.gameObject.transform.localEulerAngles = currentDoorRotation;
            yield return null;
        }
        
        PlayAudio(closeDoorClip);
        
        doorCollider.enabled = true;
        working = false;
    }

    void PlayAudio(AudioClip clip)
    {
        au.pitch = Random.Range(0.75f, 1.25f);
        au.clip = clip;
        au.Play();
    }
}
