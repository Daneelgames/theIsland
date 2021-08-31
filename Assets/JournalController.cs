using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JournalController : MonoBehaviour
{
    public Animator journalAnim;
    
    public List<Text> questLineListStrings;
    public Text activeQuestLineNameString;
    public Text activeQuestLineNameDescription;
    public Text activeQuestNameString;
    public Text activeQuestDescriptionString;

    private bool active = false;
    private static readonly int Active = Animator.StringToHash("Active");

    private float journalCooldown = 0.2f;
    private float journalCooldownCurrent = 0f;

    private void Update()
    {
        if (Input.GetButtonDown("Journal"))
            ToggleJournal();
    }

    public void ToggleJournal()
    {
        
        if (QuestLinesManager.instance.MainQuestLineQuestsCompleted < 0)
            return;
        
        if (journalCooldownCurrent > 0)
            return;
        
        StartCoroutine(JournalCooldownCoroutine());
        
        active = !active;

        Debug.Log("1");
        if (active)
        {
            UpdateJournalText();
        }
        Debug.Log("2");
        journalAnim.SetBool(Active, active);
    }

    void UpdateJournalText()
    {
        UpdateQuestsText();
    }

    void UpdateQuestsText()
    {
        for (int i = 0; i < questLineListStrings.Count; i++)
        {
            if (i == 0)
            {
                questLineListStrings[i].text = QuestLinesManager.instance.mainQuestLine.questLineName[GameManager.instance.gameLanguage];   
            }
            else
            {
                questLineListStrings[i].text = String.Empty;
            }
        }

        activeQuestLineNameString.text = QuestLinesManager.instance.mainQuestLine.questLineName[GameManager.instance.gameLanguage];
        activeQuestLineNameDescription.text = QuestLinesManager.instance.mainQuestLine.questLineDescription[GameManager.instance.gameLanguage];
        
        activeQuestNameString.text = QuestLinesManager.instance.mainQuestLine.quests[QuestLinesManager.instance.MainQuestLineQuestsCompleted].questName[GameManager.instance.gameLanguage];
        activeQuestDescriptionString.text = QuestLinesManager.instance.mainQuestLine.quests[QuestLinesManager.instance.MainQuestLineQuestsCompleted].questDescription[GameManager.instance.gameLanguage];
    }

    IEnumerator JournalCooldownCoroutine()
    {
        journalCooldownCurrent = journalCooldown;
        yield return new WaitForSeconds(journalCooldown);
        journalCooldownCurrent = 0;
    }
    
}
