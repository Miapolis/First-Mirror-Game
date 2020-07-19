using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InvokePlayer : MonoBehaviour
{
    public void EndHostAndLoadScene(int role)
    {
        var networkDiscovery = FindObjectOfType<NetworkDiscovery>();
        networkDiscovery.StopDiscovery();

        if (!NetworkServer.active) { return; }

        NetworkManager.singleton.ServerChangeScene("Win");
    }
}