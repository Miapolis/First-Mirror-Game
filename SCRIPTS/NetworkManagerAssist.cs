using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerAssist : MonoBehaviour
{
    public GameObject networkManager;

    void Start()
    {
        var netManagers  = FindObjectsOfType<ModifiedNetworkManager>();

        if (netManagers.Length == 0)
        {
            networkManager.SetActive(true);
        }

        //Otherwise don't enable the disabled one
    }
}