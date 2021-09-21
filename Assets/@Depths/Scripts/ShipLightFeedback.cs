using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipLightFeedback : MonoBehaviour
{
    public float blinkT = 1f;
    public MeshRenderer mesh;
    public Color defaultColor = Color.black;
    public Light light;
    
    public void Blink(Color color)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(BlinkCoroutine(color));
    }

    IEnumerator BlinkCoroutine(Color color)
    {
        mesh.material.color = color;
        light.color = color;
        light.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(blinkT);
        mesh.material.color = defaultColor;
        light.color = defaultColor;
        light.gameObject.SetActive(false);
    }
}