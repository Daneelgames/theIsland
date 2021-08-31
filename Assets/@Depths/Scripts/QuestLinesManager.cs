using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestLinesManager : MonoBehaviour
{
    public static QuestLinesManager instance;
    public QuestLine mainQuestLine;
    [SerializeField] private int _mainQuestLineQuestsCompleted = -1;
    public List<QuestLine> sideQuestLines = new List<QuestLine>();

    private void Awake()
    {
        instance = this;
    }

    public int MainQuestLineQuestsCompleted
    {
        get { return _mainQuestLineQuestsCompleted; }
        set {_mainQuestLineQuestsCompleted = value; }
    }

    public void StartQuestLine(QuestLine questLineToStart)
    {
        if (questLineToStart == mainQuestLine && MainQuestLineQuestsCompleted == -1)
            MainQuestLineQuestsCompleted = 0;
    }
}
