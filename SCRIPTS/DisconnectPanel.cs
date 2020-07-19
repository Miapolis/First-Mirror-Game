using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.UI;

public class DisconnectPanel : MonoBehaviour
{
    public GameObject cancelButton;
    public Image cancelButtonImage;

    bool cancelButtonFadingUp;

    private TransitionInfo transitionInfo;
    private SceneChangeManager sceneChangeManager;
    private AudioManager aumanager;
    private NetworkDiscovery networkDiscovery;

    [SerializeField] private float fadeTime;

    public void CancelButtonPressed()
    {
        aumanager.PlaySound(0);

        Color oc = cancelButtonImage.color;
        cancelButtonImage.color = new Color(oc.r, oc.g, oc.b, 1f);

        transitionInfo.overrideParamaters = false;
        transitionInfo.canceledHost = true;

        sceneChangeManager.ChangeScene("Main");
    }

    public void CancelButtonPointerUp()
    {
        Color oc = cancelButtonImage.color;
        cancelButtonImage.color = new Color(oc.r, oc.g, oc.b, 130 / 255f);
    }

    void CancelAndStopHosting()
    {
        if (networkDiscovery == null) { return; }

        networkDiscovery.StopDiscovery();

        NetworkManager.singleton.StopHost();
    }

    // Start is called before the first frame update
    void Start()
    {
        transitionInfo = FindObjectOfType<TransitionInfo>();
        sceneChangeManager = FindObjectOfType<SceneChangeManager>();
        aumanager = FindObjectOfType<AudioManager>();
        networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        SceneChangeManager.OnStopHostingCancel += CancelAndStopHosting;
    }
}