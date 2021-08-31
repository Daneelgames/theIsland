using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabberController : MonoBehaviour
{
    public float maxDistance = 5;
    public float grabTime = 1f;
    public float holdMinTime = 1f;
    public float releaseTime = 1f;

    public Transform carryTransform;
    
    public Animator anim;

    private bool canAct = true;

    private InteractiveObject carryingObject;
    
    private static readonly int Grab = Animator.StringToHash("Grab");
    private static readonly int Hold = Animator.StringToHash("Hold");
    private static readonly int Release = Animator.StringToHash("Release");

    enum State
    {
        Idle,
        Grabbing,
        Holding,
        Release
    }

    private State grabberState = State.Idle;

    private Coroutine carryCoroutine;
    
    public void UseGrabberInput()
    {
        if (!canAct)
            return;

        switch (grabberState)
        {
            case State.Idle:
                grabberState = State.Grabbing;
                anim.SetTrigger(Grab);
                StartCoroutine(CanActCooldown(grabTime));
                break;
            case State.Holding:
                grabberState = State.Release;
                anim.SetTrigger(Release);

                StartCoroutine(CanActCooldown(releaseTime));
                break;
        }
    }

    IEnumerator CanActCooldown(float t)
    {
        canAct = false;
        yield return new WaitForSeconds(t);
        canAct = true;
        if (grabberState == State.Release)
        {
            ReleaseObject();
            grabberState = State.Idle;
        }
        else if (grabberState == State.Grabbing)
        {
            grabberState = State.Holding;
            anim.SetTrigger(Hold);
            StartCoroutine(CanActCooldown(holdMinTime));
        }
    }


    private bool canGrab = false;
    void OnTriggerStay(Collider coll)
    {
        if (grabberState != State.Grabbing || canAct || carryingObject != null)
            return;
        
        if (coll.gameObject.layer == 9)
        {
            InteractiveObject interactive = coll.gameObject.GetComponent<InteractiveObject>();
            if (interactive == null || interactive.actionList.Count <= 0)
                return;
            
            canGrab = false;
            
            for (int i = 0; i < interactive.actionList.Count; i++)
            {
                if (interactive.actionList[i].actionType == InteractiveObject.ActionType.PickUp)
                {
                    canGrab = true;
                    break;
                }
            }
            
            if (!canGrab)
                return;
                    
            GrabObject(interactive);

            carryCoroutine = StartCoroutine(CarryCoroutine());
        }
    }
    
    void GrabObject(InteractiveObject interactive)
    {
        carryingObject = interactive;
        carryingObject.transform.parent = transform;
    }

    void ReleaseObject()
    {
        if (carryCoroutine != null)
        {
            StopCoroutine(carryCoroutine);
            carryCoroutine = null;
        }
        
        if (carryingObject)
            carryingObject.transform.parent = null;
        
        carryingObject = null;
        
    }
    
    IEnumerator CarryCoroutine()
    {
        while (true)
        {
            if (carryingObject == null)
            {
                carryCoroutine = null;
                yield break;
            }
            
            carryingObject.transform.position = carryTransform.position;
            carryingObject.transform.rotation = carryTransform.rotation;
            
            
            yield return null;
        }
    }
}
