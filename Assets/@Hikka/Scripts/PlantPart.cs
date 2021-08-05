using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantPart : MonoBehaviour
{
    [SerializeField] private CapsuleCollider coll;
    
    [SerializeField] private Transform _partStartPoint;
    [SerializeField] private Transform _partEndPoint;
    [SerializeField] private List<Transform> _leafPoints;

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

    public List<Transform> LeafPoints
    {
        get => _leafPoints;
        set => _leafPoints = value;
    }

    private Coroutine rotateAwayCoroutine;
    public void RotateAway(Quaternion newRot)
    {
        if (rotateAwayCoroutine != null)
            StopCoroutine(rotateAwayCoroutine);
        
        rotateAwayCoroutine = StartCoroutine(RotateAwayFromConstrainer(newRot));
    }
    
    public IEnumerator RotateAwayFromConstrainer(Quaternion newRotation)
    {
        float t = 0;
        float tt = Random.Range(0.5f, 2f);

        var startRotation = transform.rotation;
        while (t < tt)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, newRotation, t/tt);
            yield return null;
        }
        StopCoroutine(rotateAwayCoroutine);
    }
}
