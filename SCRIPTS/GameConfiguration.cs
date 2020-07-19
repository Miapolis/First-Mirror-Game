using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfiguration : MonoBehaviour
{
    public int role;

    public bool turnsAlternateAfterEndOfRound;

    public int playUpTo;

    public void SetRole(int role)
    {
        this.role = role;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}