using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RadarObjectListController : MonoBehaviour
{
    public HealthController ownShipHc;
    public float radarDistanceMax = 50;
    public List<ShipScreenButton> shipScreenButtons;
    public List<GameObject> listOfObjects = new List<GameObject>();
    
    IEnumerator Start()
    {
        while (true)
        {
            for (int i = 0; i < MobSpawnManager.instance.Units.Count; i++)
            {
                if (ownShipHc.gameObject == MobSpawnManager.instance.Units[i].gameObject)
                    continue;
                
                if (listOfObjects.Contains(MobSpawnManager.instance.Units[i].gameObject) == false)
                    listOfObjects.Add(MobSpawnManager.instance.Units[i].gameObject);
            }
            for (int i = 0; i < InteractiveObjectsManager.instance.shipInteractiveObjects.Count; i++)
            {
                if (listOfObjects.Contains(InteractiveObjectsManager.instance.shipInteractiveObjects[i].gameObject) == false)
                    listOfObjects.Add(InteractiveObjectsManager.instance.shipInteractiveObjects[i].gameObject);
            }
            yield return new WaitForSeconds(0.1f);

            if (listOfObjects.Count > 0)
            {
                for (int i = listOfObjects.Count - 1; i >= 0; i--)
                {
                    if (listOfObjects[i] == null)
                        listOfObjects.RemoveAt(i);
                }   
            }
            
            listOfObjects = new List<GameObject>(listOfObjects.OrderBy(point => Vector3.Distance(transform.position, point.transform.position)).ToArray());
            
            for (int i = 0; i < shipScreenButtons.Count; i++)
            {
                if (i >= listOfObjects.Count)
                {
                    shipScreenButtons[i].textField.text = String.Empty;
                    continue;
                }
                shipScreenButtons[i].textField.text = listOfObjects[i].gameObject.name;
            }
        }
    }
}