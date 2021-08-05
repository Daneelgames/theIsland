using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantGrowthConstrainer : MonoBehaviour
{
    private int plantPartLayerIndex = 10;

    void OnCollisionEnter(Collision coll)
    {
        if (coll.gameObject.layer == plantPartLayerIndex)
        {
            foreach (var point in coll.contacts)
            {
                Debug.Log(point.otherCollider.gameObject.name);
            }
            //coll.collider.gameObject.SetActive(false);
        }
    }
}