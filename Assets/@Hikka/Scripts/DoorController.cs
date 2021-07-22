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
    
    private void OnTriggerStay(Collider other)
    {
        if (!working && other.gameObject == PlayerMovement.instance.gameObject && PlayerInteractionController.instance.draggingObject == null)
        {
            working = true;
            if (activateLoopsLoadingTrigger)
            {
                LoopsLoadingManager.instance.ActivateAllLoadingTriggers();
            }
            StartCoroutine(MovePlayerThroughDoor());
        }
    }

    IEnumerator MovePlayerThroughDoor()
    {
        initialPlayerPosition.position = new Vector3(initialPlayerPosition.position.x, PlayerMovement.instance.transform.position.y, initialPlayerPosition.position.z);
        finalPlayerPosition.position = new Vector3(finalPlayerPosition.position.x, PlayerMovement.instance.transform.position.y, finalPlayerPosition.position.z);
        
        // MOVE THE PLAYER TO INITIAL POINT
        if (timeToMoveToInitialPos > 0)
            yield return StartCoroutine(PlayerMovement.instance.MovePlayerWithoutControl(initialPlayerPosition.position, timeToMoveToInitialPos));

        StartCoroutine(OpenDoor());

        // MOVE THE PLAYER THROUGH THE DOOR
        yield return StartCoroutine(PlayerMovement.instance.MovePlayerWithoutControl(finalPlayerPosition.position, timeToMoveToFinalPos));
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
