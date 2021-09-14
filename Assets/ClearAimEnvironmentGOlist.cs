using System.Collections;
using System.Collections.Generic;
using Polarith.AI.Move;
using UnityEngine;

public class ClearAimEnvironmentGOlist : MonoBehaviour
{
    public AIMEnvironment _aimEnvironment;

    [ContextMenu("Clear Nulls in List")]
    public void ClearNullsList()
    {
        for (int i = _aimEnvironment.GameObjects.Count - 1; i >= 0; i--)
        {
            if (_aimEnvironment.GameObjects[i] == null)
            {
                _aimEnvironment.GameObjects.RemoveAt(i);
            }
        }
    }
}
