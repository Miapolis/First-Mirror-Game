using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mirror
{
    public class HostPlayerJoinMenu : MonoBehaviour
    {
        public GameObject cancelButton;
        public Image cancelButtonImage;

        public GameObject glass;
        public GameObject text;

        private ModifiedNetworkManager networkManager;

        private NetworkDiscovery networkDiscovery;
        bool cancelButtonFadingUp;

        private TransitionInfo transitionInfo;

        private SceneChangeManager sceneChangeManager;

        bool waitTextAlreadyScaling = false;

        private AudioManager aumanager;

        [SerializeField] private float fadeTime;

        void CancelAndStopHosting()
        {
            if (networkDiscovery == null) { return; }

            networkDiscovery.StopDiscovery();

            NetworkManager.singleton.StopHost();
        }

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

        public void ScaleTextOnClick()
        {
            if (waitTextAlreadyScaling) { return; }
            waitTextAlreadyScaling = true;

            LeanTween.scale(text, new Vector3(1.05f, 1.05f, 1f), 0.1f).setLoopPingPong(1).setOnComplete(SetToFalse);

            void SetToFalse()
            {
                waitTextAlreadyScaling = false;
            }
        }

        void Start()
        {
            networkManager = FindObjectOfType<ModifiedNetworkManager>();
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
            transitionInfo = FindObjectOfType<TransitionInfo>();
            sceneChangeManager = FindObjectOfType<SceneChangeManager>();
            aumanager = FindObjectOfType<AudioManager>();

            SceneChangeManager.OnStopHostingCancel += CancelAndStopHosting;

            LeanTween.scale(glass, new Vector3(1.1f, 1.1f, 1.1f), 2f).setEaseInOutSine().setLoopPingPong();
        }
    }
}