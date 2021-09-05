using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RadioCallData", menuName = "ScriptableObjects/New RadioCall Data", order = 0)]
public class RadioCallData : ScriptableObject
{
    public List<RadioMessage> messagess;
}

[Serializable]
public class RadioMessage
{
    public string speakerName;
    public Sprite speakerImage;
    public List<string> speakerPhrase;
}
