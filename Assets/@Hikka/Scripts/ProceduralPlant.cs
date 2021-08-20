using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GPUInstancer;
using PlayerControls;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class ProceduralPlant : MonoBehaviour
{
    public PlantData plantData;

    [Header("Plants parts")] 
    [SerializeField] private PlantPartData knots;
    [SerializeField] private PlantPartData attackingKnots;

    [SerializeField] private PlantPartData branches;
    [SerializeField] private PlantPartData attackingBranches;
    [SerializeField] private PlantPartData leaves;
    [SerializeField] private PlantPartData fruits;

    [Header("Age And Health")]
    [Tooltip("Increases if actions are right; Decreases if actions are wrong or the plant is dying")]
    [SerializeField]
    private int currentHealth = 3;

    public int CurrentHealth
    {
        get { return currentHealth; }
        set { currentHealth = value; }
    }

    [SerializeField] private int minHealthForFruitsSpawn = 6;

    [SerializeField] private int currentAgeInDays = 0;

    public int CurrentAge
    {
        get { return currentAgeInDays; }
    }

    [SerializeField] private Vector2Int startDyingOnDayMinMax = new Vector2Int(5, 5);
    [SerializeField] private int currentDyingAge = 5;

    [Header("Growth settings")] [SerializeField]
    private int growthRate = 1;

    [SerializeField] private bool animateRotation = false;
    [SerializeField] private bool animateScale = true;
    [SerializeField] private bool scaleEveryNode = false;
    [SerializeField] [Range(1, 1.5f)] private float maxKnotGrowScalerPerCycle = 1.2f;
    [SerializeField] [Range(0, 1)] private float mainKnotGrowthChance = 0.15f;
    [SerializeField] private Vector2 localKnotScaleScalerMinMax = new Vector2(0.8f, 1.2f);
    [SerializeField] private Vector2 localBranchScaleScalerMinMax = new Vector2(0.9f, 1f);
    [SerializeField] private Vector2 knotsMinMaxRotation = new Vector2(-90, 90);
    [SerializeField] private Vector2 branchesMinMaxRotation = new Vector2(-30, 30);
    [SerializeField] private Vector2 startBranchesMinMaxAmount = Vector2.one;
    [SerializeField] [Range(0, 1)] private float defaultGrowBranchesChance = 0.3f;
    [SerializeField] [Range(0, 1)] private float defaultTwoBranchesChance = 0.3f;
    [SerializeField] [Range(0, 1)] private float defaultLeavesGrowChance = 0.1f;
    [SerializeField] [Range(0, 1)] private float defaultFruitsGrowChance = 0.025f;

    [Header("Nodes and parts in memory")] [SerializeField]
    private List<PlantNode> plantNodes = new List<PlantNode>();

    [SerializeField] private List<PlantPart> newPlantParts = new List<PlantPart>();

    [Header("CollisionWithConstrainers")] [SerializeField]
    private LayerMask _layerMask;

    private int currentGrowthStep = 0;
    private Coroutine checkNodesForCollisionsCoroutine;

    private bool ableToDoNextGrowthStep = true;
    private PlantController _plantController;

    public ProceduralPlantStatsFeedback statsFeedback;

    private void Start()
    {
        ProceduralPlantsManager.instance.AddProceduralPlant(this);

        currentDyingAge = Random.Range(startDyingOnDayMinMax.x, startDyingOnDayMinMax.y);
    }

    public void PlantBorn(PlantController controller)
    {
        _plantController = controller;

        statsFeedback.UpdateText(this);

        StartCoroutine(NextGrowthStep());
    }

    public void NewDay()
    {
        currentAgeInDays++;

        int hpOffset = _plantController.CompareActionsWithRequirements();
        currentHealth += hpOffset;

        statsFeedback.UpdateText(this);

        if (currentAgeInDays >= currentDyingAge)
        {
            // start coroutine NextStepDying
            currentHealth -= 2;
            StartCoroutine(NextDyingStep());
        }
        else if (hpOffset > 0)
        {
            StartCoroutine(NextGrowthStep());
        }
    }



    IEnumerator NextDyingStep()
    {
        int t = 0;
        for (int i = 0; i < plantNodes.Count; i++)
        {
            if (plantNodes[i] == null)
            {
                plantNodes.RemoveAt(i);
                continue;
            }

            if (plantNodes[i].spawnedKnot && plantNodes[i].spawnedKnot.transform.lossyScale.x < 0.1f)
            {
                continue;
            }

            plantNodes[i].spawnedKnot.transform.localScale *= 0.9f;

            if (plantNodes[i].spawnedLeaves.Count > 0)
            {
                StartCoroutine(DropLeaves(plantNodes[i]));
            }

            t++;

            if (t == 5)
            {
                t = 0;
                yield return null;
            }
        }

        if (currentHealth <= 0 || plantNodes[0].spawnedKnot && plantNodes[0].spawnedKnot.transform.localScale.x < 0.1f)
        {
            PlantDeath();
        }
    }

    IEnumerator DropLeaves(PlantNode node)
    {
        RaycastHit hit;
        for (int j = 0; j < node.spawnedLeaves.Count; j++)
        {
            var leaf = node.spawnedLeaves[j];
            leaf.transform.parent = null;

            if (Physics.Raycast(leaf.transform.position, Vector3.down, out hit, 10, _layerMask))
            {
                leaf.transform.position = hit.point;
            }

            node.spawnedLeaves.RemoveAt(j);
            yield return null;
        }
    }

    public void PlantDeath()
    {
        ProceduralPlantsManager.instance.RemovePlant(this);
        AssetSpawner.instance.Spawn(ProceduralPlantsManager.instance.ToolPickUpsReferences[plantData.inventoryIndex],
            transform.position, Quaternion.identity, AssetSpawner.ObjectType.Tool);
        Destroy(gameObject);
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
            yield return StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count - 1], transform.position,
                1f, transform, Mathf.RoundToInt(Random.Range(startBranchesMinMaxAmount.x, startBranchesMinMaxAmount.y)),
                null));
        }
        else
        {
            if (Random.value <= mainKnotGrowthChance)
            {
                if (scaleEveryNode)
                {
                    for (int i = 0; i < plantNodes.Count; i++)
                    {
                        if (newPlantParts.Contains(plantNodes[i].spawnedKnot))
                            continue;

                        StartCoroutine(ScalePlantPart(plantNodes[i].spawnedKnot,
                            plantNodes[i].spawnedKnot.transform.localScale,
                            plantNodes[i].spawnedKnot.transform.localScale *
                            Random.Range(1f, maxKnotGrowScalerPerCycle)));
                    }
                }
                else
                {
                    yield return StartCoroutine(ScalePlantPart(plantNodes[0].spawnedKnot,
                        plantNodes[0].spawnedKnot.transform.localScale,
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
                plantNodes[plantNodes.Count - 1].attackingNode = newPlantParts[i].ParentPlantNode.attackingNode;
                
                int r = 1;
                if (Random.value < defaultTwoBranchesChance)
                    r = 2;

                yield return StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count - 1],
                    newPlantParts[i].partEndPoint.position,
                    Random.Range(localKnotScaleScalerMinMax.x, localKnotScaleScalerMinMax.y),
                    newPlantParts[i].transform, r, newPlantParts[i].ParentPlantNode));
                newPlantParts.RemoveAt(i);
            }
        }

        ableToDoNextGrowthStep = true;
    }


    IEnumerator GrowNewNode(PlantNode newNode, Vector3 originPos, float localScaleScaler, Transform knotParent,
        int branchesAmount, PlantNode parentNode)
    {
        if (parentNode != null)
            parentNode.closestChildNodes.Add(newNode);
        
        newNode.ProceduralPlant = this;

        if (!newNode.attackingNode)
            newNode.spawnedKnot = Instantiate(knots.plantPartPrefab[Random.Range(0, knots.plantPartPrefab.Count)], originPos, Quaternion.identity);
        else
            newNode.spawnedKnot = Instantiate(attackingKnots.plantPartPrefab[Random.Range(0, attackingKnots.plantPartPrefab.Count)], originPos, Quaternion.identity);

        newNode.spawnedKnot.MasterPlant = this;
        newNode.spawnedKnot.transform.parent = knotParent;
        newNode.spawnedKnot.transform.localEulerAngles = Vector3.zero;
        newNode.spawnedKnot.ParentPlantNode = newNode;
        if (plantNodes.Count > 1)
            newNode.spawnedKnot.transform.localEulerAngles += new Vector3(
                Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y),
                Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y),
                Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y));

        var startScale = Vector3.zero;
        var endScale = knotParent.localScale * localScaleScaler;
        newNode.spawnedKnot.transform.localScale = startScale;

        for (int i = 0; i < branchesAmount; i++)
        {
            if (Random.value < 0.1f ||newNode.attackingNode)
                CreateAttackingBrunch(newNode, i);
            else
                CreateBrunch(newNode, i);
            yield return null;
        }

        StartCoroutine(ScalePlantPart(newNode.spawnedKnot, startScale, endScale));
    }

    void CreateAttackingBrunch(PlantNode parentNode, int i)
    {
        int prefabR = Random.Range(0, attackingBranches.plantPartPrefab.Count);
        Debug.Log("attackingBranches.plantPartPrefab.count: " + attackingBranches.plantPartPrefab.Count + "; prefabR: " + prefabR);
        parentNode.attackingNode = true;
        parentNode.spawnedAttackingBranches.Add(Instantiate(attackingBranches.plantPartPrefab[prefabR], parentNode.spawnedKnot.partEndPoint.transform.position, Quaternion.identity, parentNode.spawnedKnot.transform));
        parentNode.spawnedAttackingBranches[i].MasterPlant = this;
        parentNode.spawnedAttackingBranches[i].ParentPlantNode = parentNode;
        if (i == 0)
            parentNode.spawnedAttackingBranches[i].transform.localEulerAngles = Vector3.zero;
        else
        {
            Debug.Log("spawnedAttackingBranches: " + parentNode.spawnedAttackingBranches.Count + "; i: " + i + "; i-1 = " + (i-1).ToString());
            parentNode.spawnedAttackingBranches[i].transform.localEulerAngles = parentNode.spawnedAttackingBranches[i-1].transform.localEulerAngles;
            
            Debug.Log("XXX");
        }
            
        
        parentNode.spawnedAttackingBranches[i].transform.localEulerAngles += new Vector3(Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y));
        
        parentNode.spawnedAttackingBranches[i].transform.localScale = Vector3.one * Random.Range(localBranchScaleScalerMinMax.x, localBranchScaleScalerMinMax.y); 
            
        newPlantParts.Add(parentNode.spawnedAttackingBranches[i]);
    }

    void CreateBrunch(PlantNode parentNode, int i)
    {
        int prefabR = Random.Range(0, branches.plantPartPrefab.Count);
        parentNode.spawnedBranches.Add(Instantiate(branches.plantPartPrefab[prefabR], parentNode.spawnedKnot.partEndPoint.transform.position, Quaternion.identity, parentNode.spawnedKnot.transform));
        //_plantNode.spawnedBranches[i].transform.parent = _plantNode.spawnedKnot.transform;
        parentNode.spawnedBranches[i].MasterPlant = this;
        parentNode.spawnedBranches[i].ParentPlantNode = parentNode;
        if (i == 0)
            parentNode.spawnedBranches[i].transform.localEulerAngles = Vector3.zero;
        else
            parentNode.spawnedBranches[i].transform.localEulerAngles = parentNode.spawnedBranches[i-1].transform.localEulerAngles;
            
        parentNode.spawnedBranches[i].transform.localEulerAngles += new Vector3(Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y));
        
        parentNode.spawnedBranches[i].transform.localScale = Vector3.one * Random.Range(localBranchScaleScalerMinMax.x, localBranchScaleScalerMinMax.y); 
            
        newPlantParts.Add(parentNode.spawnedBranches[i]);
        CreateLeaves(parentNode, parentNode.spawnedBranches[i], parentNode);
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
        if (currentHealth <= minHealthForFruitsSpawn)
            return;
        
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
            for (int i = 0; i < plantNodes.Count; i++)
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
                    if (plantNodes[i].spawnedBranches[j].isRotating || plantNodes[i].spawnedBranches[j].transform.lossyScale.x < 0.1f)
                        continue;
                    
                    capsuleStart = plantNodes[i].spawnedBranches[j].partStartPoint.position;
                    capsuleEnd = plantNodes[i].spawnedBranches[j].partEndPoint.position;
                    direction = plantNodes[i].spawnedBranches[j].transform.forward;
                    capsuleRadius = plantNodes[i].spawnedBranches[j].capsuleCollider.radius;
                    CheckPlantPartForCollision(plantNodes[i].spawnedBranches[j], capsuleStart, capsuleEnd, capsuleRadius, direction);
                }

                ++t;
                if (t == 2)
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
        Debug.DrawLine(capsuleStart, capsuleEnd, Color.red, duration: 1f);
        RaycastHit hit;
        //if (Physics.SphereCast(capsuleStart, capsuleRadius, direction, out hit, Vector3.Distance(capsuleStart, capsuleEnd), _layerMask))
        //if (Physics.Raycast(capsuleStart, direction, out hit, Vector3.Distance(capsuleStart, capsuleEnd), _layerMask))
        if (Physics.Linecast(capsuleStart, capsuleEnd, out hit, _layerMask))
        {
            Debug.Log("ProceduralPlant.CheckNodesForCollisions");

            part.transform.localEulerAngles += new Vector3(Random.Range(-5f, 5f), Random.Range(-5f, 5f), Random.Range(-5f, 5f));
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

            for (int j = 0; j < plantNodes[i].spawnedAttackingBranches.Count; j++)
            {
                if (plantNodes[i].spawnedAttackingBranches[j] == part)
                {
                    Destroy(part.gameObject);
                    plantNodes[i].spawnedAttackingBranches.RemoveAt(j);
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

    #region selection

    private PlantNode nodeSelected;
    public void SelectNode(PlantNode node)
    {
        if (node == nodeSelected)
            return;

        nodeSelected?.ShowSpawnedKnot();

        nodeSelected = node;

        if (selectedNodeFeedbackCoroutine == null)
        {
            selectedNodeFeedbackCoroutine = StartCoroutine(SelectedNodeFeedbackCoroutine());
            StartCoroutine(GetDistanceToPlayer());
        }
    }
    
    public void UnselectNode(PlantNode node)
    {
        node?.ShowSpawnedKnot();
    }

    public void StopSelectionCoroutine()
    {
        if (selectedNodeFeedbackCoroutine != null)
            StopCoroutine(selectedNodeFeedbackCoroutine);
    }
    
    private Coroutine selectedNodeFeedbackCoroutine;
    public IEnumerator SelectedNodeFeedbackCoroutine()
    {
        while (true)
        {
            if (PlayerToolsController.instance.selectedToolIndex != 3)
            {
                nodeSelected.ShowSpawnedKnot();
                nodeSelected = null;
            }
            
            if (nodeSelected == null)
                yield break;
            
            nodeSelected.HideSpawnedKnot();
            yield return new WaitForSeconds(0.2f);

            if (nodeSelected == null)
                yield break;

            nodeSelected.ShowSpawnedKnot();   
            yield return new WaitForSeconds(0.2f);
        }
    }

    IEnumerator GetDistanceToPlayer()
    {
        float distance = 0;
        while (distance < 2)
        {
            yield return new WaitForSeconds(1);
            if (nodeSelected == null || nodeSelected.spawnedKnot == null)
                break;
            
            distance = Vector3.Distance(nodeSelected.spawnedKnot.transform.position, PlayerMovement.instance.transform.position);
        }
        
        if (selectedNodeFeedbackCoroutine != null)
            StopCoroutine(selectedNodeFeedbackCoroutine);

        nodeSelected?.ShowSpawnedKnot();

        nodeSelected = null;
    }
    #endregion
}

[Serializable]
public class PlantNode
{
    public bool attackingNode = false;
    
    // parent
    // child
    private ProceduralPlant _proceduralPlant;
    public PlantPart spawnedKnot;
    public List<PlantPart> spawnedBranches = new List<PlantPart>();
    public List<PlantPart> spawnedAttackingBranches = new List<PlantPart>();
    public List<PlantPart> spawnedLeaves = new List<PlantPart>();
    public List<PlantPart> spawnedFruits = new List<PlantPart>();
    
    public List<PlantNode> closestChildNodes = new List<PlantNode>();


    public ProceduralPlant ProceduralPlant
    {
        get { return _proceduralPlant; }
        set { _proceduralPlant = value; }
    }
    
    public void HideSpawnedKnot()
    {
        if (spawnedKnot)
            spawnedKnot.gameObject.SetActive(false);
    }

    public void ShowSpawnedKnot()
    {
        if (spawnedKnot)
            spawnedKnot.gameObject.SetActive(true);
    }
}

[Serializable]
public class PlantPartData
{
    public List<PlantPart> plantPartPrefab;
    [Header("Knots and branches ignore this:")]
    public Vector2 startGrowthOnStepMinMax = new Vector2(3,8);
}
