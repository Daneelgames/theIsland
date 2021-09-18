using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource musicSource;
    public Text textField;
    
    public void TryToToggleMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Stop();
            textField.text = "MUSIC OFF";
        }
        else
        {
            textField.text = "MUSIC ON";
            musicSource.pitch = Random.Range(0.75f, 1f);
            musicSource.Play();
        }
    }
}
