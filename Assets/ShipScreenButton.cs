using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipScreenButton : MonoBehaviour
{
    private bool empty = true;
    private bool selected = false; 
    public Text textField;
    public AudioSource au;

    public GameObject assignedObject;
    
    [ContextMenu("GetText")]
    public void GetText()
    {
        textField = gameObject.GetComponent<Text>();
        au = gameObject.GetComponent<AudioSource>();
    }
    
    public void SelectTextField(bool _select)
    {
        if (!selected && _select)
        {   
            // play select audio
            au.pitch = Random.Range(0.75f, 1.25f);
            au.Play();
        }

        if (_select != selected)
        {
            selected = _select;
            textField.fontStyle = selected ? FontStyle.Bold : FontStyle.Normal;   
        }
    }

    public void AssignObject(GameObject go)
    {
        assignedObject = go;
    }
}