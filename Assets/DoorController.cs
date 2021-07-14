using System.Collections;
using PlayerControls;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform initialPlayerPosition;
    public float timeToMoveToInitialPos = 0.5f;
    public Transform finalPlayerPosition;
    public float timeToMoveToFinalPos = 1f;

    [Header("Door properties")] 
    public Collider doorCollider;

    public float timeToOpenTheDoor = 1f;
    public float timeToCloseTheDoor = 0.5f;
    public Vector3 doorEulerOpened;
    public Vector3 doorEulerClosed;

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
        if (other.gameObject == PlayerMovement.instance.gameObject)
        {
            StartCoroutine(MovePlayerThroughDoor());
        }
    }

    IEnumerator MovePlayerThroughDoor()
    {
        PlayerMovement.instance.inControl = false;
        float t = 0;
        PlayerMovement.instance.rb.isKinematic = true;
        
        // MOVE THE PLAYER TO INITIAL POINT
        Vector3 _initialPlayerPosition = PlayerMovement.instance.transform.position;
        while ( t < timeToMoveToInitialPos)
        {
            t += Time.smoothDeltaTime;
            PlayerAudioController.instance.PlaySteps();
            PlayerMovement.instance.transform.position = Vector3.Lerp(_initialPlayerPosition, initialPlayerPosition.position, t / timeToMoveToInitialPos);
            yield return null;
        }

        StartCoroutine(OpenDoor());

        // MOVE THE PLAYER THROUGH THE DOOR
        t = 0;
        _initialPlayerPosition = PlayerMovement.instance.transform.position;
        while ( t < timeToMoveToFinalPos)
        {
            t += Time.smoothDeltaTime;
            PlayerAudioController.instance.PlaySteps();
            PlayerMovement.instance.transform.position = Vector3.Lerp(_initialPlayerPosition, finalPlayerPosition.position, t / timeToMoveToInitialPos);
            yield return null;
        }
        
        PlayerMovement.instance.rb.isKinematic = false;
        PlayerMovement.instance.inControl = true;
    }

    IEnumerator OpenDoor()
    {
        Vector3 currentDoorRotation = doorCollider.transform.localEulerAngles;
        doorCollider.enabled = false;
         
        // OPEN THE DOOR
        float t = 0;
        while ( t < timeToOpenTheDoor)
        {
            t += Time.smoothDeltaTime;
            currentDoorRotation = new Vector3(0, Mathf.LerpAngle(doorEulerClosed.y, doorEulerOpened.y, t / timeToOpenTheDoor), 0);
            doorCollider.gameObject.transform.localEulerAngles = currentDoorRotation;
            yield return null;
        }
        
        // CLOSE THE DOOR
        t = 0;
        while ( t < timeToCloseTheDoor)
        {
            t += Time.smoothDeltaTime;
            currentDoorRotation = new Vector3(0, Mathf.LerpAngle(doorEulerOpened.y, doorEulerClosed.y, t / timeToCloseTheDoor), 0);
            doorCollider.gameObject.transform.localEulerAngles = currentDoorRotation;
            yield return null;
        }
        doorCollider.enabled = true;
    }
}
