using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoveryHelper : MonoBehaviour
{
    public GameObject discoveryPrefab;
    [Space]
    public GameObject loadingPanel;
    public GameObject searchPanel;

    void Start()
    {
        Instantiate(discoveryPrefab, discoveryPrefab.transform.position, Quaternion.identity);
    }
}