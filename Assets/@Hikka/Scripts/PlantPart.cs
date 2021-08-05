using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantPart : MonoBehaviour
{
    [SerializeField] private CapsuleCollider coll;
    
    [SerializeField] private Transform _partStartPoint;
    [SerializeField] private Transform _partEndPoint;

    [SerializeField] private bool rotating = false;
    public Transform partStartPoint
    {
        get { return _partStartPoint; }
    }
    
    public Transform partEndPoint
    {
        get { return _partEndPoint; }
    }

    public CapsuleCollider collider
    {
        get {return coll; }
    }

    public bool Rotating
    {
        get { return rotating; }
        set {rotating = value; }
    }
}
