using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioCallsManager : MonoBehaviour
{
    public RadioCallController radioCallController;

    private RadioCallData currentCall;
    
    int currentPhrase = 0;
    
    public void RadioCallToPlay(RadioCallData radioCall)
    {
        currentCall = radioCall;
        currentPhrase = -1;
        StartCoroutine(PlayNewPhrase());
    }

    IEnumerator PlayNewPhrase()
    {
        currentPhrase++;
        radioCallController.speakerPhoto = currentCall.messagess[currentPhrase].speakerImage;
        radioCallController.speakerNameString.text = currentCall.messagess[currentPhrase].speakerName.ToUpper();
        radioCallController.speakerPhraseString.text = currentCall.messagess[currentPhrase].speakerPhrase[GameManager.instance.gameLanguage];

        yield return null;
    }
}
