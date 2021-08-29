using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLockController : MonoBehaviour
{
    enum State
    {
        Opened, Closed
    }
    private State _state = State.Closed;
    public Animator anim;
    private static readonly int Opened = Animator.StringToHash("Opened");

    public void UseDoorLockInput()
    {
        switch (_state)
        {
            case State.Closed:
                OpenDoor();
                break;
            
            case State.Opened:
                CloseDoor();
                break;
        }
    }
    
    void OpenDoor()
    {
        _state = State.Opened;
        anim.SetBool(Opened, true);
    }
    void CloseDoor()
    {
        _state = State.Closed;
        anim.SetBool(Opened, false);
    }
}