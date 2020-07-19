using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfigAssistant : MonoBehaviour
{
    public GameObject gameConfig;

    void Awake()
    {
        var configs = FindObjectsOfType<GameConfiguration>();

        if (configs.Length == 0)
        {
            gameConfig.SetActive(true);
        }
    }
}