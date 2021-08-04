using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Random = UnityEngine.Random;

public class ProceduralPlant : MonoBehaviour
{
    public List<PlantPart> branchesPrefabs;
    public List<PlantPart> knotsPrefabs;
    public List<PlantPart> leavesPrefabs;
    public List<PlantPart> fruitsPrefabs;

    [SerializeField] 
    private PlantNode plantNodes;
    public void NextGrowStep()
    {
        if (plantNodes.knot == null)
        {
            plantNodes = new PlantNode();
            GrowNewNode(plantNodes, transform.position, transform, 1);
        }
        else if (plantNodes.childNodes.Count == 0)
        {
            plantNodes.childNodes.Add(new PlantNode());
            GrowNewNode(plantNodes.childNodes[0], plantNodes.branches[0].partEndPoint.position, plantNodes.branches[0].transform, 2);
        }
        else
        {
            for (int i = 0; i < plantNodes.childNodes[plantNodes.childNodes.Count-1].branches.Count; i++)
            {
                plantNodes.childNodes[plantNodes.childNodes.Count-1].childNodes.Add(new PlantNode());
                GrowNewNode(plantNodes.childNodes[plantNodes.childNodes.Count-1].childNodes[i], 
                            plantNodes.childNodes[plantNodes.childNodes.Count-1].branches[i].partEndPoint.position, 
                            plantNodes.childNodes[plantNodes.childNodes.Count-1].branches[i].transform, Random.Range(1,4));
            }
        }
    }

    public void GrowNewNode(PlantNode _plantNode, Vector3 originPos, Transform knotParent, int branchesAmount)
    {
        _plantNode.knot = Instantiate(knotsPrefabs[Random.Range(0, knotsPrefabs.Count)], originPos, Quaternion.identity);
        _plantNode.knot.transform.localEulerAngles = new Vector3(Random.Range(290, 250), Random.Range(0, 360), Random.Range(0, 360));
        _plantNode.knot.transform.parent = knotParent;
        _plantNode.knot.transform.localScale = knotParent.localScale * 0.9f; 
        for (int i = 0; i < branchesAmount; i++)
        {
            _plantNode.branches.Add(Instantiate(branchesPrefabs[Random.Range(0, branchesPrefabs.Count)], _plantNode.knot.partEndPoint.transform.position, Quaternion.identity));
            _plantNode.branches[i].transform.localEulerAngles = new Vector3(Random.Range(290, 250), Random.Range(0, 360), Random.Range(0, 360));
            _plantNode.branches[i].transform.parent = _plantNode.knot.transform;
            _plantNode.branches[i].transform.localScale = _plantNode.knot.transform.localScale * Random.Range(0.9f, 1.1f);   
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
    public List<PlantNode> childNodes = new List<PlantNode>();
}