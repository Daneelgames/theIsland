using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class ProceduralPlant : MonoBehaviour
{
    public List<PlantPart> knotsPrefabs;
    [SerializeField] private Vector2 knotsMinMaxRotation;
    public List<PlantPart> branchesPrefabs;
    [SerializeField] private Vector2 branchesMinMaxRotation;
    [SerializeField] private Vector2 startBranchesMinMax;
    [SerializeField] private Vector2 defaultBranchesMinMax;
    public List<PlantPart> leavesPrefabs;
    public List<PlantPart> fruitsPrefabs;

    [SerializeField] private List<PlantNode> plantNodes = new List<PlantNode>();
    [SerializeField] private List<PlantPart> newPlantParts = new List<PlantPart>();
    public IEnumerator NextGrowStep()
    {
        if (plantNodes.Count == 0)
        {
            plantNodes.Add(new PlantNode());
            StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count-1], transform.position, 0.1f, transform, Mathf.RoundToInt(Random.Range(startBranchesMinMax.x, startBranchesMinMax.y))));
        }
        else
        {
            for (int i = 0; i < plantNodes.Count; i++)
            {
                if (newPlantParts.Contains(plantNodes[i].knot))
                    continue;
                
                StartCoroutine(ScalePlantPart(plantNodes[i].knot.transform, plantNodes[i].knot.transform.localScale, plantNodes[i].knot.transform.localScale * Random.Range(1f, 1.2f)));
            }
            
            for (int i = newPlantParts.Count - 1; i >= 0; i--)
            {
                plantNodes.Add(new PlantNode());
                StartCoroutine(GrowNewNode(plantNodes[plantNodes.Count-1], newPlantParts[i].partEndPoint.position, Random.Range(0.9f, 1f), newPlantParts[i].transform, Mathf.RoundToInt(Random.Range(defaultBranchesMinMax.x, defaultBranchesMinMax.y))));
                newPlantParts.RemoveAt(i);
                yield return null;
            }
        }
    }

    public IEnumerator GrowNewNode(PlantNode _plantNode, Vector3 originPos, float localScaleScaler, Transform knotParent, int branchesAmount)
    {
        _plantNode.knot = Instantiate(knotsPrefabs[Random.Range(0, knotsPrefabs.Count)], originPos, Quaternion.identity);
        _plantNode.knot.transform.parent = knotParent;
        _plantNode.knot.transform.localEulerAngles = Vector3.zero;
        _plantNode.knot.transform.localEulerAngles += new Vector3(Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y), Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y), Random.Range(knotsMinMaxRotation.x, knotsMinMaxRotation.y));
        
        var startScale = Vector3.zero;
        var endScale = knotParent.localScale * localScaleScaler;
        StartCoroutine(ScalePlantPart(_plantNode.knot.transform, startScale, endScale));
        
        for (int i = 0; i < branchesAmount; i++)
        {
            int prefabR = Random.Range(0, branchesPrefabs.Count);
            _plantNode.branches.Add(Instantiate(branchesPrefabs[prefabR], _plantNode.knot.partEndPoint.transform.position, Quaternion.identity));
            _plantNode.branches[i].transform.parent = _plantNode.knot.transform;
            if (i == 0)
                _plantNode.branches[i].transform.localEulerAngles = Vector3.zero;
            else
                _plantNode.branches[i].transform.localEulerAngles = _plantNode.branches[i-1].transform.localEulerAngles;
            
            _plantNode.branches[i].transform.localEulerAngles += new Vector3(Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y), Random.Range(branchesMinMaxRotation.x, branchesMinMaxRotation.y));
            _plantNode.branches[i].transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f); 
            
            newPlantParts.Add(_plantNode.branches[i]);
            yield return null;
        }
    }

    IEnumerator ScalePlantPart(Transform part,Vector3 startLocalScale, Vector3 newLocalScale)
    {
        float t = 0;
        float tt = Random.Range(0.25f, 1f);
        part.localScale = startLocalScale;
        while (t < tt)
        {
            t += Time.deltaTime;
            part.localScale = Vector3.Lerp(startLocalScale, newLocalScale, t/tt);
            yield return null;
        }
        part.localScale = newLocalScale;
    }

    public void ResetPlant()
    {
        StopAllCoroutines();
        Destroy(plantNodes[0].knot.gameObject);
        plantNodes.Clear();
        newPlantParts.Clear();
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