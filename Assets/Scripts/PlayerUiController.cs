using System;
using System.Collections;
using System.Collections.Generic;
using PlayerControls;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerUiController : MonoBehaviour
{
    public static PlayerUiController instance;

    public Image selectedObjectIcon;
    public Image selectedObjectIconBackground;
    public Canvas canvas;
    
    public List<UiActionText> actionTextListUi;
    
    private bool selectedObject = false;
    private Vector3 selectedPosition;
    
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private Vector3 screenPos;
    private Vector3 uiFeedbackPosition;
    public void SelectedInteractableObject(Vector3 newPos)
    {
        if (!selectedObject)
        {
            selectedObject = true;
            StopCoroutine(AnimatePointer());
            
            StartCoroutine(AnimatePointer());
            StartCoroutine(MovePointer());
        }
        selectedPosition = newPos;
    }

    public void NoSelectedObject()
    {
        selectedObject = false;
        StopCoroutine(MovePointer());
    }

    IEnumerator AnimatePointer()
    {
        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            selectedObjectIcon.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t/0.5f);
            selectedObjectIconBackground.transform.localScale = selectedObjectIcon.transform.localScale;
            yield return null;
        }

        while (selectedObject)
        {
            selectedObjectIconBackground.transform.localScale = Vector3.one + new Vector3(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), 0);
            yield return null;
        }

        t = 0;
        
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            selectedObjectIcon.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero , t/0.5f);
            selectedObjectIconBackground.transform.localScale = selectedObjectIcon.transform.localScale;
            yield return null;
        }

    }
    
    IEnumerator MovePointer()
    {
        while (true)
        {
            screenPos = MouseLook.instance.mainCamera.WorldToScreenPoint(selectedPosition);
            screenPos.z = (canvas.transform.position - MouseLook.instance.handsCamera.transform.position).magnitude;
            uiFeedbackPosition = MouseLook.instance.handsCamera.ScreenToWorldPoint(screenPos);
        
            selectedObjectIcon.transform.position = uiFeedbackPosition;
            selectedObjectIconBackground.transform.position = selectedObjectIcon.transform.position;
            yield return null;
        }
    }
}

[Serializable]
public class UiActionText
{
    public Text uiText;
    public Image uiBackgroundImage;
}
