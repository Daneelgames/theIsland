using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetObjectsToPositionsWithOffset : MonoBehaviour
{

    public GameObject prefabToInstantiate;
    public List<Transform> targetTransforms = new List<Transform>();
    public Vector3 positionOffset = Vector3.zero;
    public Vector3 eulerOffset = Vector3.zero;
    
    [ContextMenu("InstantiateObjectsFromList")]
    public void InstantiateObjectsFromList()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            Vector3 newPos = targetTransforms[i].transform.position + positionOffset;
            Vector3 newEuler = targetTransforms[i].transform.localEulerAngles + eulerOffset;

            GameObject newObj = PrefabUtility.InstantiatePrefab(prefabToInstantiate) as GameObject;
            newObj.transform.position = newPos;
            newObj.transform.parent = transform;
            newObj.transform.localEulerAngles = newEuler;
        }
    }
}
