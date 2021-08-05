using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantPart : MonoBehaviour
{
    [SerializeField] private Transform _partStartPoint;
    [SerializeField] private Transform _partEndPoint;

    public Transform partStartPoint
    {
        get { return _partStartPoint; }
        set { _partStartPoint = value; }
    }
    
    public Transform partEndPoint
    {
        get { return _partEndPoint; }
        set { _partEndPoint = value; }
    }
}
