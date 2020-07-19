using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips = new AudioClip[4];

    AudioSource src;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        src = gameObject.GetComponent<AudioSource>();
    }

    ///<param name = "audioClipNumber">0 for button hover, 1 for button click, 2 for tile hover, 3 for tile click</param>
    public void PlaySound(int audioClipNumber)
    {
        src.clip = audioClips[audioClipNumber];
        src.Play();
    }
}