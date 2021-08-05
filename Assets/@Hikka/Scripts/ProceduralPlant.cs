using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class ProceduralPlant : MonoBehaviour
{
    [Header("Plants parts")]
    [SerializeField] private List<PlantPart> knotsPrefabs;
    [SerializeField] private List<PlantPart> branchesPrefabs;
    [SerializeField] private List<PlantPart> leavesPrefabs;
    [SerializeField] private List<PlantPart> fruitsPrefabs;
    
    [Header("Growth settings")]
    [SerializeField] private bool animateGrowth = true;
    [SerializeField] private bool scaleEveryNode = false;
    [SerializeField] private Vector2 localKnotScaleScalerMinMax;
    [SerializeField] private Vector2 localBranchScaleScalerMinMax;
    [SerializeField] private Vector2 knotsMinMaxRotation;
    [SerializeField] [Range(1,1.5f)] private float maxKnotGrowScalerPerCycle = 1.05f;
    [SerializeField] private Vector2 branchesMinMaxRotation;
    [SerializeField] private Vector2 startBranchesMinMax;
    [SerializeField] private Vector2 defaultBranchesMinMax;
    
    [Header("Nodes and parts in memory")]
    [SerializeField] private List<PlantNode> plantNodes = new List<PlantNode>();
    [SerializeField] private List<PlantPart> newPlantParts = new List<PlantPart>();
    
    public IEnumerator NextGrowStep()
    {
        if (plantNodes.Count == 0)
        {
            plantNodes.Add(new PlantNode());
            StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count-1], transform.position, 1f, transform, Mathf.RoundToInt(Random.Range(startBranchesMinMax.x, startBranchesMinMax.y))));
        }
        else
        {
            if (scaleEveryNode)
            {
                for (int i = 0; i < plantNodes.Count; i++)
                {
                    if (newPlantParts.Contains(plantNodes[i].knot))
                        continue;
                
                    StartCoroutine(ScalePlantPart(plantNodes[i].knot, plantNodes[i].knot.transform.localScale, plantNodes[i].knot.transform.localScale * Random.Range(1f, maxKnotGrowScalerPerCycle)));
                }   
            }
            else
            {
                StartCoroutine(ScalePlantPart(plantNodes[0].knot, plantNodes[0].knot.transform.localScale, plantNodes[0].knot.transform.localScale * Random.Range(1f, maxKnotGrowScalerPerCycle)));
            }
            
            for (int i = newPlantParts.Count - 1; i >= 0; i--)
            {
                plantNodes.Add(new PlantNode());
                StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count-1], newPlantParts[i].partEndPoint.position, Random.Range(localKnotScaleScalerMinMax.x, localKnotScaleScalerMinMax.y), newPlantParts[i].transform, Mathf.RoundToInt(Random.Range(defaultBranchesMinMax.x, defaultBranchesMinMax.y))));
                newPlantParts.RemoveAt(i);
                yield return null;
            }
        }
    }

    IEnumerator GrowNewNode(PlantNode _plantNode, Vector3 originPos, float localScaleScaler, Transform knotParent, int branchesAmount)
    {
        _plantNode.knot = Instantiate(knotsPrefabs[Random.Range(0, knotsPrefabs.Count)], originPos, Quaternion.identity);
        _plantNode.knot.transform.parent = knotParent;
        _plantNode.knot.transform.localEulerAngles = Vector3.zero;
        if (plantNodes.Count > 1)
            _plantNode.knot.transform.localEulerAngles += new Vector3(Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y), Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y), Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y));
        
        var startScale = Vector3.zero;
        var endScale = knotParent.localScale * localScaleScaler;
        _plantNode.knot.transform.localScale = startScale;
        
        for (int i = 0; i < branchesAmount; i++)
        {
            CreateBrunch(_plantNode, i);
            yield return null;
        }
        
        StartCoroutine(ScalePlantPart(_plantNode.knot, startScale, endScale));
    }

    void CreateBrunch(PlantNode _plantNode, int i)
    {
        int prefabR = Random.Range(0, branchesPrefabs.Count);
        _plantNode.branches.Add(Instantiate(branchesPrefabs[prefabR], _plantNode.knot.partEndPoint.transform.position, Quaternion.identity));
        _plantNode.branches[i].transform.parent = _plantNode.knot.transform;
        if (i == 0)
            _plantNode.branches[i].transform.localEulerAngles = Vector3.zero;
        else
            _plantNode.branches[i].transform.localEulerAngles = _plantNode.branches[i-1].transform.localEulerAngles;
            
        _plantNode.branches[i].transform.localEulerAngles += new Vector3(Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y));
        _plantNode.branches[i].transform.localScale = Vector3.one * Random.Range(localBranchScaleScalerMinMax.x, localBranchScaleScalerMinMax.y); 
            
        newPlantParts.Add(_plantNode.branches[i]);
    }
    
    IEnumerator ScalePlantPart(PlantPart part,Vector3 startLocalScale, Vector3 newLocalScale)
    {
        float t = 0;
        float tt = Random.Range(0.25f, 1f);
        if (!animateGrowth) tt = 0;
        part.transform.localScale = startLocalScale;
        while (t < tt)
        {
            t += Time.deltaTime;
            part.transform.localScale = Vector3.Lerp(startLocalScale, newLocalScale, t/tt);
            yield return null;
        }
        part.transform.localScale = newLocalScale;
    }

    public void ResetPlant()
    {
        StopAllCoroutines();
        Destroy(plantNodes[0].knot.gameObject);
        plantNodes.Clear();
        newPlantParts.Clear();
    }

    public void RemovePlantPart(PlantPart part)
    {
        for (int i = plantNodes.Count - 1; i >= 0; i--)
        {
            if (plantNodes[i].knot == part)
            {
                Destroy(part.gameObject);
                plantNodes.RemoveAt(i);
                break;
            }

            for (int j = 0; j < plantNodes[i].branches.Count; j++)
            {
                if (plantNodes[i].branches[j] == part)
                {
                    Destroy(part.gameObject);
                    plantNodes[i].branches.RemoveAt(j);
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
    public PlantPart knot;
    public List<PlantPart> branches = new List<PlantPart>();
    public List<int> childNodesIndexes = new List<int>();
}