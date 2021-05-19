using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class ObjectForPooling : MonoBehaviour
{
    public bool inPool = true;
    void OnDisable()
    {
        if (!ObjectPooling.instance.machinegunBulletsPool.Contains(this))
        {
            ObjectPooling.instance.machinegunBulletsPool.Add(this);   
        }

        inPool = true;
    }
}
