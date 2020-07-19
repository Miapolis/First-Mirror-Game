using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerAssistant : MonoBehaviour
{
    public GameObject audioManager;

    void Awake()
    {
        var managers = FindObjectsOfType<AudioManager>();

        if (managers.Length == 0)
        {
            audioManager.SetActive(true);
        }
    }
}
