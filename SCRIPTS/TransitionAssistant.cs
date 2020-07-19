using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionAssistant : MonoBehaviour
{
    public GameObject transitionInfo;

    void Awake()
    {
        var transitionInfos = FindObjectsOfType<TransitionInfo>();

        if (transitionInfos.Length == 0)
        {
            transitionInfo.SetActive(true);
        }

        //Otherwise don't enable the disabled one
    }
}
