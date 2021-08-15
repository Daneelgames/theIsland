using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjectsManager : MonoBehaviour
{
    public static InteractiveObjectsManager instance;
    public List<InteractiveObject> potsInteractiveObjects = new List<InteractiveObject>();
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }


    public InteractiveObject GetClosestInteractiveObject(Vector3 pos, List<InteractiveObject> tempList)
    {
        float distance = 1000;
        float newDistance = 0;

        InteractiveObject closestIO = null;
        
        for (int i = 0; i < tempList.Count; i++)
        {
            if (i < tempList.Count && tempList[i] != null)
            {
                newDistance = Vector3.Distance(pos, tempList[i].transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    closestIO = tempList[i];
                }
            }
        }

        return closestIO; 
    }

}
