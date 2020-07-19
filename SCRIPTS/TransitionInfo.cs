using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionInfo : MonoBehaviour
{
    [Header("General")]
    [Tooltip("Makes the TransitionInfo persist between scenes")]
    public bool dontDestroyOnLoad = true;

    [Space]

    [Header("Case Specific Attributes And Scenes")]
    [Tooltip("Set this to true if you want to ignore below parameters")]
    public bool overrideParamaters = false;
    [Space]
    public bool canceledHost;
    public bool returningFromWin;

    [Header("Game Parameters")]
    public int? currentInstanceRole = null;

    [Header("Other")]
    [Range(0f, 5f)]
    public float panelFadeTime;
    public int? playerWonRole = null;

    void Start()
    {
        if (dontDestroyOnLoad) { DontDestroyOnLoad(gameObject); }
    }

    public void Reset()
    {
        overrideParamaters = false;
        canceledHost = false;
        returningFromWin = false;
        currentInstanceRole = null;
        playerWonRole = null;
    }
}