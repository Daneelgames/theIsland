using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPUInstancer;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralPlant : MonoBehaviour
{
    [Header("Plants parts")]
    [SerializeField] private PlantPartData knots;
    [SerializeField] private PlantPartData branches;
    [SerializeField] private PlantPartData leaves;
    [SerializeField] private PlantPartData fruits;

    [Header("Growth settings")]
    [SerializeField] private int growthRate = 1;
    [SerializeField] private bool animateRotation = false;
    [SerializeField] private bool animateScale = true;
    [SerializeField] private bool scaleEveryNode = false;
    [SerializeField] [Range(1,1.5f)] private float maxKnotGrowScalerPerCycle = 1.2f;
    [SerializeField] [Range(0,1)]private float mainKnotGrowthChance = 0.15f;
    [SerializeField] private Vector2 localKnotScaleScalerMinMax = new Vector2(0.8f, 1.2f);
    [SerializeField] private Vector2 localBranchScaleScalerMinMax= new Vector2(0.9f, 1f);
    [SerializeField] private Vector2 knotsMinMaxRotation = new Vector2(-90, 90);
    [SerializeField] private Vector2 branchesMinMaxRotation= new Vector2(-30, 30);
    [SerializeField] private Vector2 startBranchesMinMaxAmount = Vector2.one;
    [SerializeField] [Range(0,1)]private float defaultGrowBranchesChance = 0.3f;
    [SerializeField] [Range(0,1)]private float defaultTwoBranchesChance = 0.3f;
    [SerializeField] [Range(0,1)]private float defaultLeavesGrowChance = 0.1f;
    [SerializeField] [Range(0,1)]private float defaultFruitsGrowChance = 0.025f;

    [Header("Nodes and parts in memory")] 
    [SerializeField] private List<PlantNode> plantNodes = new List<PlantNode>();
    [SerializeField] private List<PlantPart> newPlantParts = new List<PlantPart>();

    [Header("CollisionWithConstrainers")]
    [SerializeField] private LayerMask _layerMask;
    
    private int currentGrowthStep = 0;
    private Coroutine checkNodesForCollisionsCoroutine;

    private bool ableToDoNextGrowthStep = true;
    
    private void Start()
    {
        InteractiveObjectsManager.instance.proceduralPlants.Add(this);
        
    }

    public IEnumerator NextGrowthStep()
    {
        if (!ableToDoNextGrowthStep)
            yield break;

        ableToDoNextGrowthStep = false;
        
        currentGrowthStep++;
        
        if (checkNodesForCollisionsCoroutine == null)
            StartCoroutine(CheckNodesForCollisions());
        
        for (int i = 0; i < growthRate; i++)
        {
            yield return StartCoroutine(NextGrowthStepCoroutine());   
        }
    }

    public IEnumerator NextGrowthStepCoroutine()
    {
        if (plantNodes.Count == 0)
        {
            plantNodes.Add(new PlantNode());
            yield return StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count-1], transform.position, 
                1f, transform, Mathf.RoundToInt(Random.Range(startBranchesMinMaxAmount.x, startBranchesMinMaxAmount.y)), null));
        }
        else
        {
            if (Random.value < mainKnotGrowthChance)
            {
                if (scaleEveryNode)
                {
                    for (int i = 0; i < plantNodes.Count; i++)
                    {
                        if (newPlantParts.Contains(plantNodes[i].spawnedKnot))
                            continue;
                
                        StartCoroutine(ScalePlantPart(plantNodes[i].spawnedKnot, plantNodes[i].spawnedKnot.transform.localScale, plantNodes[i].spawnedKnot.transform.localScale * Random.Range(1f, maxKnotGrowScalerPerCycle)));
                    }   
                }
                else
                {
                    yield return StartCoroutine(ScalePlantPart(plantNodes[0].spawnedKnot, plantNodes[0].spawnedKnot.transform.localScale, 
                        plantNodes[0].spawnedKnot.transform.localScale * Random.Range(1f, maxKnotGrowScalerPerCycle)));
                }   
            }
            
            // GROW NEW NODES
            for (int i = newPlantParts.Count - 1; i >= 0; i--)
            {
                if (newPlantParts[i] == null)
                {
                    newPlantParts.RemoveAt(i);
                    continue;
                }
                
                if (Random.value > defaultGrowBranchesChance)
                    continue;
                
                plantNodes.Add(new PlantNode());
                int r = 1;
                if (Random.value < defaultTwoBranchesChance)
                    r = 2;
                
                yield return StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count-1], newPlantParts[i].partEndPoint.position, 
                    Random.Range(localKnotScaleScalerMinMax.x, localKnotScaleScalerMinMax.y), newPlantParts[i].transform, r, newPlantParts[i].ParentPlantNode));
                newPlantParts.RemoveAt(i);
            }
        }
        ableToDoNextGrowthStep = true;
    }


    IEnumerator GrowNewNode(PlantNode newNode, Vector3 originPos, float localScaleScaler, Transform knotParent, int branchesAmount, PlantNode parentNode)
    {
        if (parentNode != null)
            parentNode.closestChildNodes.Add(newNode);
        
        newNode.spawnedKnot = Instantiate(knots.plantPartPrefab[Random.Range(0, knots.plantPartPrefab.Count)], originPos, Quaternion.identity, knotParent);
        
        newNode.spawnedKnot.MasterPlant = this;
        //newNode.spawnedKnot.transform.parent = knotParent;
        newNode.spawnedKnot.transform.localEulerAngles = Vector3.zero;
        newNode.spawnedKnot.ParentPlantNode = newNode;
        if (plantNodes.Count > 1)
            newNode.spawnedKnot.transform.localEulerAngles += new Vector3(Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y), Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y), Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y));
        
        var startScale = Vector3.zero;
        var endScale = knotParent.localScale * localScaleScaler;
        newNode.spawnedKnot.transform.localScale = startScale;
        
        for (int i = 0; i < branchesAmount; i++)
        {
            CreateBrunch(newNode, i, newNode);
            yield return null;
        }
        
        StartCoroutine(ScalePlantPart(newNode.spawnedKnot, startScale, endScale));
    }

    void CreateBrunch(PlantNode _plantNode, int i, PlantNode parentPlantNode)
    {
        int prefabR = Random.Range(0, branches.plantPartPrefab.Count);
        _plantNode.spawnedBranches.Add(Instantiate(branches.plantPartPrefab[prefabR], _plantNode.spawnedKnot.partEndPoint.transform.position, Quaternion.identity, _plantNode.spawnedKnot.transform));
        //_plantNode.spawnedBranches[i].transform.parent = _plantNode.spawnedKnot.transform;
        _plantNode.spawnedBranches[i].MasterPlant = this;
        _plantNode.spawnedBranches[i].ParentPlantNode = parentPlantNode;
        if (i == 0)
            _plantNode.spawnedBranches[i].transform.localEulerAngles = Vector3.zero;
        else
            _plantNode.spawnedBranches[i].transform.localEulerAngles = _plantNode.spawnedBranches[i-1].transform.localEulerAngles;
            
        _plantNode.spawnedBranches[i].transform.localEulerAngles += new Vector3(Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y));
        
        _plantNode.spawnedBranches[i].transform.localScale = Vector3.one * Random.Range(localBranchScaleScalerMinMax.x, localBranchScaleScalerMinMax.y); 
            
        newPlantParts.Add(_plantNode.spawnedBranches[i]);
        CreateLeaves(_plantNode, _plantNode.spawnedBranches[i], parentPlantNode);
    }

    void CreateLeaves(PlantNode _plantNode, PlantPart branch, PlantNode parentPlantNode)
    {
        var points = branch.spawnPoints;
        if (points.Count <= 0)
            return;
        
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (currentGrowthStep < Random.Range(leaves.startGrowthOnStepMinMax.x, leaves.startGrowthOnStepMinMax.y))
                continue;

            if (Random.value < defaultLeavesGrowChance)
            {            
                int pointR = Random.Range(0, points.Count);
                int prefabR = Random.Range(0, leaves.plantPartPrefab.Count);
                var newLeaf = Instantiate(leaves.plantPartPrefab[prefabR], points[pointR].position, Quaternion.identity, points[pointR]);
                
                newLeaf.MasterPlant = this;
                
                _plantNode.spawnedLeaves.Add(newLeaf);
                newLeaf.ParentPlantNode = parentPlantNode;
                //newLeaf.transform.parent = points[pointR];
                newLeaf.transform.localEulerAngles = Vector3.zero;
                newLeaf.transform.localScale = Vector3.one;
                points.RemoveAt(pointR);
            }
            else
            {
                branch.fruitsSpawnPoints.Add(points[i]);
                points.RemoveAt(i);
            }
        }

        CreateFruits(_plantNode, branch, parentPlantNode);
    }
    
    void CreateFruits(PlantNode _plantNode, PlantPart branch, PlantNode parentPlantNode)
    {
        var points = branch.fruitsSpawnPoints;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (currentGrowthStep < Random.Range(fruits.startGrowthOnStepMinMax.x, fruits.startGrowthOnStepMinMax.y))
                continue;

            if (Random.value < defaultFruitsGrowChance)
            {            
                int pointR = Random.Range(0, points.Count);
                int prefabR = Random.Range(0, fruits.plantPartPrefab.Count);
                var newFruit = Instantiate(fruits.plantPartPrefab[prefabR], points[pointR].position, Quaternion.identity, points[pointR]);
                
                newFruit.MasterPlant = this;
                
                _plantNode.spawnedFruits.Add(newFruit);
                //newFruit.transform.parent = points[pointR];
                newFruit.ParentPlantNode = parentPlantNode;
                newFruit.transform.localEulerAngles = Vector3.zero;
                newFruit.transform.localScale = Vector3.one;
                points.RemoveAt(pointR);
            }
            else
            {
                points.RemoveAt(i);
            }
        }
    }
    
    IEnumerator ScalePlantPart(PlantPart part, Vector3 startLocalScale, Vector3 newLocalScale)
    {
        float t = 0;
        float tt = Random.Range(0.25f, 1f);
        if (!animateScale) tt = 0;
        
        if (part == null)
            yield break;
        
        Debug.Log(startLocalScale + "; " + newLocalScale);
        
        part.transform.localScale = startLocalScale;
        while (t < tt)
        {
            t += Time.deltaTime;
            part.transform.localScale = Vector3.Lerp(startLocalScale, newLocalScale, t/tt);
            yield return null;
        }
        part.transform.localScale = newLocalScale;
    }

    
    IEnumerator CheckNodesForCollisions()
    {
        Vector3 capsuleStart;
        Vector3 capsuleEnd;
        Vector3 direction;
        float capsuleRadius;

        int t = 0;

        while (true)
        {
            for (int i = 1; i < plantNodes.Count; i++)
            {
                
                if (plantNodes[i].spawnedKnot == null)
                {
                    plantNodes.RemoveAt(i);
                    continue;
                }

                if (plantNodes[i].spawnedKnot.isRotating == false)
                {
                    capsuleStart = plantNodes[i].spawnedKnot.partStartPoint.position;
                    capsuleEnd = plantNodes[i].spawnedKnot.partEndPoint.position;
                    direction = plantNodes[i].spawnedKnot.transform.forward;
                    capsuleRadius = plantNodes[i].spawnedKnot.capsuleCollider.radius;
                    CheckPlantPartForCollision(plantNodes[i].spawnedKnot, capsuleStart, capsuleEnd, capsuleRadius, direction);   
                }

                for (int j = 0; j < plantNodes[i].spawnedBranches.Count; j++)
                {
                    if (plantNodes[i].spawnedBranches[j].isRotating)
                        continue;
                    
                    capsuleStart = plantNodes[i].spawnedBranches[j].partStartPoint.position;
                    capsuleEnd = plantNodes[i].spawnedBranches[j].partEndPoint.position;
                    direction = plantNodes[i].spawnedBranches[j].transform.forward;
                    capsuleRadius = plantNodes[i].spawnedBranches[j].capsuleCollider.radius;
                    CheckPlantPartForCollision(plantNodes[i].spawnedBranches[j], capsuleStart, capsuleEnd, capsuleRadius, direction);
                }

                ++t;
                if (t == 1)
                {
                    t = 0;
                    yield return null;   
                }
            }
            yield return new WaitForSeconds(1);
            
        }
    }

    void CheckPlantPartForCollision(PlantPart part, Vector3 capsuleStart, Vector3 capsuleEnd, float capsuleRadius, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.SphereCast(capsuleStart, capsuleRadius, direction, out hit, Vector3.Distance(capsuleStart, capsuleEnd), _layerMask))
        {
            Vector3 temp = Vector3.Cross (part.transform.forward,hit.normal);
            if (!animateRotation)
                part.transform.rotation = Quaternion.LookRotation(-temp);
            else
                part.RotateAway(Quaternion.LookRotation(-temp));   
        }
    }

    public void ResetPlant()
    {
        StopAllCoroutines();
        Destroy(plantNodes[0].spawnedKnot.gameObject);
        plantNodes.Clear();
        newPlantParts.Clear();
        currentGrowthStep = 0;
    }

    public void RemovePlantPart(PlantPart part)
    {
        newPlantParts.Add(part.ParentPlantNode.spawnedKnot);
        
        for (int i = 0; i < part.ParentPlantNode.spawnedBranches.Count; i++)
        {
            newPlantParts.Add(part.ParentPlantNode.spawnedBranches[i]);
        }
        
        for (int i = plantNodes.Count - 1; i >= 0; i--)
        {
            if (plantNodes[i].spawnedKnot == part)
            {
                Destroy(part.gameObject);
                plantNodes.RemoveAt(i);
                break;
            }

            for (int j = 0; j < plantNodes[i].spawnedBranches.Count; j++)
            {
                if (plantNodes[i].spawnedBranches[j] == part)
                {
                    Destroy(part.gameObject);
                    plantNodes[i].spawnedBranches.RemoveAt(j);
                    break;
                }
            }
            for (int j = 0; j < plantNodes[i].spawnedFruits.Count; j++)
            {
                if (plantNodes[i].spawnedFruits[j] == part)
                {
                    Destroy(part.gameObject);
                    plantNodes[i].spawnedFruits.RemoveAt(j);
                    break;
                }
            }
            for (int j = 0; j < plantNodes[i].spawnedLeaves.Count; j++)
            {
                if (plantNodes[i].spawnedLeaves[j] == part)
                {
                    Destroy(part.gameObject);
                    plantNodes[i].spawnedLeaves.RemoveAt(j);
                    break;
                }
            }
        }
    }
}

[Serializable]
public class PlantNode
{
    // parent
    // child
    public PlantPart spawnedKnot;
    public List<PlantPart> spawnedBranches = new List<PlantPart>();
    public List<PlantPart> spawnedLeaves = new List<PlantPart>();
    public List<PlantPart> spawnedFruits = new List<PlantPart>();
    
    public List<PlantNode> closestChildNodes = new List<PlantNode>();

    public void SelectNode()
    {
        spawnedKnot.SelectPart();
        for (int i = 0; i < spawnedBranches.Count; i++)
        {
            spawnedBranches[i].SelectPart();
        }
        for (int i = 0; i < spawnedLeaves.Count; i++)
        {
            spawnedLeaves[i].SelectPart();
        }
        for (int i = 0; i < spawnedFruits.Count; i++)
        {
            spawnedFruits[i].SelectPart();
        }
        for (int i = 0; i < closestChildNodes.Count; i++)
        {
            closestChildNodes[i].SelectNode();
        }
    }

    public void UnselectNode()
    {
        spawnedKnot.UnselectPart();
        for (int i = 0; i < spawnedBranches.Count; i++)
        {
            spawnedBranches[i].UnselectPart();
        }
        for (int i = 0; i < spawnedLeaves.Count; i++)
        {
            spawnedLeaves[i].UnselectPart();
        }
        for (int i = 0; i < spawnedFruits.Count; i++)
        {
            spawnedFruits[i].UnselectPart();
        }
        for (int i = 0; i < closestChildNodes.Count; i++)
        {
            closestChildNodes[i].UnselectNode();
        }
    }
}

[Serializable]
public class PlantPartData
{
    public List<PlantPart> plantPartPrefab;
    [Header("Knots and branches ignore this:")]
    public Vector2 startGrowthOnStepMinMax = new Vector2(3,8);
}