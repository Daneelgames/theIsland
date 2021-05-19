using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling instance;

    public ObjectForPooling machinegunBulletPrefab;
    public List<ObjectForPooling> machinegunBulletsPool;
    public Transform objectPoolFolder;

    void Awake()
    {
        instance = this;
    }

    public GameObject GetBullet()
    {
        ObjectForPooling m = null;
        for (int i = machinegunBulletsPool.Count - 1; i >= 0; i--)
        {
            if (!machinegunBulletsPool[i].isActiveAndEnabled && machinegunBulletsPool[i].inPool)
            {
                m = machinegunBulletsPool[i]; 
                m.gameObject.SetActive(true);
                machinegunBulletsPool.RemoveAt(i);
                break;
            }
        } 
        if (m == null)
        {
            m = Instantiate(machinegunBulletPrefab);
            machinegunBulletsPool.Add(m);
            m.transform.parent = objectPoolFolder;
        }

        m.inPool = false;
        
        return m.gameObject;
    }
}
