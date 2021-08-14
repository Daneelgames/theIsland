using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlantPart : MonoBehaviour
{
    public enum PlantPartType
    {
        Knot, Branch, Leaves, Fruit
    }

    [SerializeField] private PlantPartType _plantPartType = PlantPartType.Branch;
    
    [SerializeField] private CapsuleCollider coll;
    
    [SerializeField] private Transform _partStartPoint;
    [SerializeField] private Transform _partEndPoint;
    [SerializeField] private List<Transform> _spawnPoints;
    private List<Transform> _fruitsSpawnPoints = new List<Transform>();

    [SerializeField] private MeshRenderer mesh;
    private Material _material;
    
    private Coroutine rotateAwayCoroutine;
    private static readonly int IsSelected = Shader.PropertyToID("is_selected");

    private ProceduralPlant _masterPlant;
    private PlantNode _parentPlantNode;

    private bool rotating = false;
    private static readonly int Emission = Shader.PropertyToID("Emission");


    void Start()
    {
        if (mesh)
            _material = mesh.material;
    }

    public PlantPartType plantPartType
    {
        get { return _plantPartType; }
    }
    public bool isRotating
    {
        get { return rotating; }
    }
    public ProceduralPlant MasterPlant
    {
        get { return _masterPlant; }
        set { _masterPlant = value; }
    }
    public PlantNode ParentPlantNode
    {
        get { return _parentPlantNode; }
        set
        {
            _parentPlantNode = value;
        }
    }
    
    #region select Part
        public void SelectPart()
        {
            
        }

        public void UnselectPart()
        {
            
        }
    #endregion
    
    public Transform partStartPoint
    {
        get { return _partStartPoint; }
    }
    
    public Transform partEndPoint
    {
        get { return _partEndPoint; }
    }

    public CapsuleCollider capsuleCollider
    {
        get {return coll; }
    }
    
    public Collider boxCollider
    {
        get {return coll; }
    }

    public List<Transform> spawnPoints
    {
        get => _spawnPoints;
        set => _spawnPoints = value;
    }
    public List<Transform> fruitsSpawnPoints
    {
        get => _fruitsSpawnPoints;
        set => _fruitsSpawnPoints = value;
    }


    public void RotateAway(Quaternion newRot)
    {
        if (rotateAwayCoroutine != null)
            StopCoroutine(rotateAwayCoroutine);
        
        rotateAwayCoroutine = StartCoroutine(RotateAwayFromConstrainer(newRot));
    }
    
    public IEnumerator RotateAwayFromConstrainer(Quaternion newRotation)
    {
        rotating = true;
        float t = 0;
        float tt = Random.Range(0.2f, 0.5f);

        var startRotation = transform.rotation;
        while (t < tt)
        {
            t += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, newRotation, t/tt);
            yield return null;
        }

        rotating = false;
        StopCoroutine(rotateAwayCoroutine);
    }
    
    
}
