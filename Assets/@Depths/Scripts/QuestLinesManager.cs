using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLinesManager : MonoBehaviour
{
    public QuestLine mainQuestLine;
    
    public List<QuestLine> sideQuestLines = new List<QuestLine>();
}

[CreateAssetMenu(fileName = "QuestLineData", menuName = "ScriptableObjects/New QuestLine Data", order = 0)]
[Serializable]
public class QuestLine: ScriptableObject
{
    [Header("Player couldn't see these:")]
    public string questLineDevName = "New Quest Line";
    
    [Header("Player could see these:")]
    public List<string> questLineName = new List<string>();
    public List<string> questLineDescription = new List<string>();
    
    public List<Quest> quests = new List<Quest>();
}

[Serializable]
public class Quest
{
    [Header("Player couldn't see these:")]
    public string questDevName = "New Quest";
    
    [Header("Player could see these:")]
    public List<string> questName = new List<string>();
    public List<string> questDescription = new List<string>();
}
