using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalCursorController : MonoBehaviour
{
    public static UniversalCursorController instance;

    private void Awake()
    {
        instance = this;
    }

    public void ShowCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }
    
    public void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
