using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RadioCallController : MonoBehaviour
{
    public Animator anim;
    public Text speakerNameString;
    public Text speakerPhraseString;
    public Image speakerPhoto;

    public AudioSource auNewMessage;
    public AudioSource auSpeakerPhrase;

    public ShipLightFeedback lightFeedback;
    private static readonly int Update = Animator.StringToHash("Update");

    public void SetMessage(RadioMessage message)
    {
        auNewMessage.pitch = Random.Range(0.75f, 1.25f);
        auNewMessage.Play();
        anim.SetTrigger(Update);
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