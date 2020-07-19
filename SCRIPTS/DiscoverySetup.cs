using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

[DisallowMultipleComponent]
public class DiscoverySetup : MonoBehaviour
{
    public GameObject loadingPanel;

    public GameObject searchPanel;

    private GameConfiguration gameConfiguration;

    public NetworkDiscovery networkDiscovery;

    private SearchGlass searchGlass;

    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

#if UNITY_EDITOR
    void OnValidate()
    {
        if (networkDiscovery == null)
        {
            networkDiscovery = GetComponent<NetworkDiscovery>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
            UnityEditor.Undo.RecordObjects(new Object[] { this, networkDiscovery }, "Set NetworkDiscovery");
        }
    }
#endif

    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;
    }

    IEnumerator UpdateAndAttemptConnection()
    {
        bool connected = false;

        while (!connected)
        {
            if (discoveredServers.Count != 0)
            {
                foreach (var info in discoveredServers.Values)
                {
                    Connect(info);

                    searchGlass = FindObjectOfType<SearchGlass>();
                    searchGlass.DisablePanel();

                    connected = true;
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    void Connect(ServerResponse info)
    {
        NetworkManager.singleton.StartClient(info.uri);
    }

    void Awake()
    {
        loadingPanel.SetActive(true);

        networkDiscovery.transport = FindObjectOfType<TelepathyTransport>();
    }

    void Start()
    {
        searchPanel.SetActive(false);
        gameConfiguration = FindObjectOfType<GameConfiguration>();
        searchGlass = FindObjectOfType<SearchGlass>();

        StartCoroutine(HidePanel());

        IEnumerator HidePanel()
        {
            yield return new WaitForSeconds(1f);

            switch (gameConfiguration.role)
            {
                case 0:
                    NetworkManager.singleton.StartHost();
                    networkDiscovery.AdvertiseServer();

                    loadingPanel.SetActive(false);

                    break;

                case 1:
                    searchPanel.SetActive(true);
                    loadingPanel.SetActive(false);
                    networkDiscovery.StartDiscovery();
                    StartCoroutine(UpdateAndAttemptConnection());

                    break;
            }

        }
    }
}