using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchGlass : MonoBehaviour
{
    private float rotateSpeed = 2.5f;
    private float radius = 0.4f;
    public bool stopRotating = false;
    public GameObject waitText;
    public GameObject underBar;
    public GameObject background;
    public GameObject parent;
    public float fadeSpeedText = 5f;

    private TransitionInfo transitionInfo;
    private SceneChangeManager sceneChangeManager;
    private AudioManager aumanager;
    private NetworkDiscovery networkDiscovery;

    public GameObject cancelButton;
    public Image cancelButtonImage;

    bool cancelButtonFadingUp;
    [SerializeField] private float fadeTime;

    private bool panelShouldBeHidden = false;

    void RotateGlass()
    {
        Vector2 center = transform.position;
        float angle = 0;

        angle += rotateSpeed * Time.deltaTime;

        var offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;

        var newPos = center + offset;

        LeanTween.move(gameObject, newPos, 0.5f).setEaseInBack().setOnComplete(StartContinue).setEaseOutExpo();

        void StartContinue()
        {
            StartCoroutine(ContinueRotateGlass(center));
        }
    }

    IEnumerator ContinueRotateGlass(Vector2 center)
    {
        float angle = 0;

        while (!stopRotating)
        {
            angle += rotateSpeed * Time.deltaTime;

            var offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius;

            transform.position = center + offset;

            yield return null;
        }
    }

    void OnEnable()
    {
        stopRotating = false;
        waitText.SetActive(false);
        transform.position = Vector3.zero;

        underBar.GetComponent<Image>().fillAmount = 0f;

        LeanTween.moveLocal(gameObject, new Vector3(-360f, 0f, 0f), 0.5f).setEaseOutBack().setOnComplete(FadeTextIn);

        void FadeTextIn()
        {
            waitText.GetComponent<TextMeshProUGUI>().color = new Color(1f, 1f, 1f, 0f);
            StartCoroutine(FadeText());
            StartCoroutine(DelayRotationOfGlass());
        }

        IEnumerator DelayRotationOfGlass()
        {
            yield return new WaitForSeconds(0.5f);

            RotateGlass();
        }

        IEnumerator FadeText()
        {
            var underBarImage = underBar.GetComponent<Image>();

            waitText.SetActive(true);
            var text = waitText.GetComponent<TextMeshProUGUI>();

            float alpha = 0f;

            while (text.color.a < 1f)
            {
                text.color = new Color(1f, 1f, 1f, alpha);
                alpha += fadeSpeedText * Time.deltaTime;
                underBarImage.fillAmount += fadeSpeedText * Time.deltaTime;

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            if (!panelShouldBeHidden)
                cancelButton.SetActive(true);
        }
    }

    public void DisablePanel()
    {
        panelShouldBeHidden = true;

        stopRotating = true;

        cancelButton.SetActive(false);

        StartCoroutine(FadeGlass());

        IEnumerator FadeGlass()
        {
            float alpha = 1f;

            var image = gameObject.GetComponent<Image>();

            while (alpha >= 0f)
            {
                alpha -= Time.deltaTime * 4f;

                image.color = new Color(1f, 1f, 1f, alpha);

                yield return null;
            }
        }

        StartCoroutine(StartHidingRest());

        IEnumerator StartHidingRest()
        {
            yield return new WaitForSeconds(0.3f);
            StartCoroutine(HideUnderBarAndText());
        }
    }

    IEnumerator HideUnderBarAndText()
    {
        var underBarImage = underBar.GetComponent<Image>();

        LeanTween.moveLocal(waitText, new Vector3(1400f, waitText.transform.localPosition.y, 0f), 0.5f).setEaseInBack();

        while (underBarImage.fillAmount > 0f)
        {
            underBarImage.fillAmount -= 6f * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        float alpha = 1f;

        var image = background.GetComponent<Image>();

        while (alpha >= 0f)
        {
            alpha -= Time.deltaTime * 4f;

            image.color = new Color(1f, 1f, 1f, alpha);

            yield return null;
        }

        parent.SetActive(false);
    }

    void OnDisable()
    {
        stopRotating = true;
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

    void CancelAndStopHosting()
    {
        if (networkDiscovery == null) { return; }

        networkDiscovery.StopDiscovery();

        NetworkManager.singleton.StopClient();
    }

    void Start()
    {
        transitionInfo = FindObjectOfType<TransitionInfo>();
        sceneChangeManager = FindObjectOfType<SceneChangeManager>();
        aumanager = FindObjectOfType<AudioManager>();
        networkDiscovery = FindObjectOfType<NetworkDiscovery>();

        SceneChangeManager.OnStopHostingCancel += CancelAndStopHosting;

        cancelButton.SetActive(false);
    }
}