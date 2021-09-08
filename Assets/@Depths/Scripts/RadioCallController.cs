using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RadioCallController : MonoBehaviour
{
    public Text speakerNameString;
    public Text speakerPhraseString;
    public Image speakerPhoto;

    public AudioSource auNewMessage;
    public AudioSource auSpeakerPhrase;

    public ShipLightFeedback lightFeedback;
    
    public void SetMessage(RadioMessage message)
    {
        auNewMessage.pitch = Random.Range(0.75f, 1.25f);
        auNewMessage.Play();
        
        if (message == null)
        {
            lightFeedback.Blink(Color.red);
            speakerPhoto.sprite = null;
            speakerPhoto.color = Color.clear;
            speakerNameString.text = String.Empty;
            speakerPhraseString.text = String.Empty;
        }
        else
        {
            lightFeedback.Blink(Color.green);
            speakerPhoto.sprite = message.speakerImage;
            speakerPhoto.color = Color.white;
            speakerNameString.text = message.speakerName.ToUpper();
            speakerPhraseString.text = message.speakerPhrase[GameManager.instance.gameLanguage];
        }
        
    }
}