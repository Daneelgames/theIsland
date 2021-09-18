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
    public List<ObjectOnRadar> listOfObjects = new List<ObjectOnRadar>();

    public Color defaultColor = new Color(0f, 0.82f, 0f);
    public Color selectedColor = new Color(1f, 0.71f, 0f);
    
    public GameObject selectedObjectInWorld;
    public GameObject compass;
    
    #region find objects for radar cycle

    IEnumerator Start()
    {
        while (true)
        {
            for (int i = 0; i < MobSpawnManager.instance.Units.Count; i++)
            {
                if (ownShipHc.gameObject == MobSpawnManager.instance.Units[i].gameObject || MobSpawnManager.instance.Units[i].fraction == HealthController.Fraction.Fish)
                    continue;

                bool canAdd = true;
                for (int j = 0; j < listOfObjects.Count; j++)
                {
                    if (j >= MobSpawnManager.instance.Units.Count)
                    {
                        
                        break;   
                    }
                    
                    if (listOfObjects[j].go == MobSpawnManager.instance.Units[i].gameObject)
                    {
                        canAdd = false;
                        break;
                    }
                }

                if (canAdd)
                {
                    TryToAddNewObjectOnRadar(i, 0);
                }

                yield return null;
            }
            
            for (int i = 0; i < InteractiveObjectsManager.instance.shipInteractiveObjects.Count; i++)
            {
                if (ownShipHc.gameObject == InteractiveObjectsManager.instance.shipInteractiveObjects[i].gameObject)
                    continue;

                bool canAdd = true;
                for (int j = 0; j < listOfObjects.Count; j++)
                {
                    if (j >= InteractiveObjectsManager.instance.shipInteractiveObjects.Count)
                    {
                        break;   
                    }
                    
                    if (listOfObjects[j].go == InteractiveObjectsManager.instance.shipInteractiveObjects[i].gameObject)
                    {
                        canAdd = false;
                        break;
                    }
                }
                
                if (canAdd)
                {
                    TryToAddNewObjectOnRadar(i, 1);
                }
            }
            yield return new WaitForSeconds(0.1f);

            if (listOfObjects.Count > 0)
            {
                for (int i = listOfObjects.Count - 1; i >= 0; i--)
                {
                    if (listOfObjects[i].go == null)
                    {
                        listOfObjects.RemoveAt(i);
                        continue;
                    }

                    var newDistance = Vector3.Distance(transform.position, listOfObjects[i].go.transform.position);
                    if (newDistance > radarDistanceMax)
                    {
                        listOfObjects.RemoveAt(i);
                        continue;
                    }
                    
                    listOfObjects[i].distance = Mathf.RoundToInt(newDistance);
                }   
            }
            
            listOfObjects = new List<ObjectOnRadar>(listOfObjects.OrderBy(point => point.distance).ToArray());

            for (int i = 0; i < shipScreenButtons.Count; i++)
            {
                if (i >= listOfObjects.Count)
                {
                    shipScreenButtons[i].textField.text = String.Empty;
                    continue;
                }

                if (shipScreenButtons[i].assignedObject == selectedObjectInWorld)
                {
                    shipScreenButtons[i].textField.color = selectedColor;   
                    if (MouseLook.instance.aiming == false)
                        shipScreenButtons[i].textField.fontStyle = FontStyle.Bold;
                }
                else
                {
                    shipScreenButtons[i].textField.color = defaultColor;
                    if (MouseLook.instance.aiming == false)
                        shipScreenButtons[i].textField.fontStyle = FontStyle.Normal;
                }
                
                shipScreenButtons[i].textField.text = "-" + listOfObjects[i].go.name + " " + listOfObjects[i].distance + "m;";
                shipScreenButtons[i].AssignObject(listOfObjects[i].go);
            }
        }
    }


        #endregion
        
    public void Interact(ShipScreenButton selectedButton)
    {
        if (selectedButton == null)
            return;

        selectedObjectInWorld = selectedButton.assignedObject;
        for (var index = 0; index < shipScreenButtons.Count; index++)
        {
            var obj = shipScreenButtons[index];
            obj.textField.color = defaultColor;
        }

        selectedButton.textField.color = selectedColor;
        if (showTargetCoroutine != null)
        {
            StopCoroutine(showTargetCoroutine);
            showTargetCoroutine = null;
        }

        showTargetCoroutine = StartCoroutine(ShowTarget());
    }

    private Coroutine showTargetCoroutine;
    IEnumerator ShowTarget()
    {
        while (true)
        {
            if (selectedObjectInWorld == null)
                yield break;
            
            compass.transform.LookAt(selectedObjectInWorld.transform.position);
            yield return null;
        }
    }
    
    void TryToAddNewObjectOnRadar(int i, int type) // 0 - unit; 1 - interactable
    {
        float distance = 0;
        if (type == 0)
            distance = Vector3.Distance(transform.position, MobSpawnManager.instance.Units[i].gameObject.transform.position);
        else if (type == 1)
        {
            distance = Vector3.Distance(transform.position, InteractiveObjectsManager.instance.shipInteractiveObjects[i].gameObject.transform.position);
        }
        if (distance < radarDistanceMax)
        {
            listOfObjects.Add(new ObjectOnRadar());
            if (type == 0)
                listOfObjects[listOfObjects.Count - 1].go = MobSpawnManager.instance.Units[i].gameObject;  
            else if (type == 1)
                listOfObjects[listOfObjects.Count - 1].go = InteractiveObjectsManager.instance.shipInteractiveObjects[i].gameObject;  
            listOfObjects[listOfObjects.Count - 1].distance = Mathf.RoundToInt(distance);   
        }   
    }
}

[Serializable]
public class ObjectOnRadar
{
    public int distance = 0;
    public GameObject go;
}