using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WinPanel : MonoBehaviour
{
    public GameObject trophy;

    public GameObject circleRays;

    public GameObject restartButton;

    public Sprite blueRays;
    public Sprite redRays;

    private bool isDisabled;

    private TransitionInfo transitionInfo;

    bool restartButtonClicked = false;

    private AudioManager audioManager;

    void OnEnable()
    {
        transitionInfo = FindObjectOfType<TransitionInfo>();

        if (transitionInfo.currentInstanceRole == 0)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            NetworkManager.singleton.StopClient();
        }

        restartButton.SetActive(false);
        isDisabled = false;
        trophy.transform.localPosition = new Vector3(0f, 890f, 0f);
        StartCoroutine(SpinRays());
        LeanTween.moveLocal(trophy, Vector3.zero, 1f).setEaseOutBack().setDelay(1f).setOnComplete(ShowButton);

        void ShowButton()
        {
            var image = restartButton.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);

            restartButton.SetActive(true);

            StartCoroutine(FadeButtonInAndScale());

            IEnumerator FadeButtonInAndScale()
            {
                float alpha = 0f;

                while (alpha < 1)
                {
                    alpha += Time.deltaTime * 5f;

                    image.color = new Color(1f, 1f, 1f, alpha);

                    yield return null;
                }

                image.color = new Color(1f, 1f, 1f, 1f);
            }
        }
    }

    IEnumerator SpinRays()
    {
        while (!isDisabled)
        {
            circleRays.transform.Rotate(new Vector3(0f, 0f, 0.05f), Space.Self);
            yield return null;
        }
    }

    void OnDisable()
    {
        isDisabled = true;
    }

    public void RestartMouseClicked()
    {
        if (restartButtonClicked) { return; }

        restartButtonClicked = true;

        audioManager.PlaySound(1);

        if (NetworkServer.active)
        {
            Debug.Log("Network Server still active");
        }

        transitionInfo.Reset();
        transitionInfo.returningFromWin = true;

        StartCoroutine(FadeButtonOut());

        LeanTween.moveLocal(trophy, new Vector3(0f, 890f, 0f), 1f).setEaseInBack().setDelay(0.3f).setOnComplete(Finish);

        IEnumerator FadeButtonOut()
        {
            var image = restartButton.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 1f);

            float alpha = 1f;

            while (alpha > 0f)
            {
                alpha -= Time.deltaTime * 5f;

                image.color = new Color(1f, 1f, 1f, alpha);

                yield return null;
            }

            image.color = new Color(1f, 1f, 1f, 0f);

            restartButton.SetActive(false);
        }

        void Finish()
        {
            var sceneChangeManager = FindObjectOfType<SceneChangeManager>();

            sceneChangeManager.ChangeScene("Main");
        }
    }

    void Start()
    {
        transitionInfo = FindObjectOfType<TransitionInfo>();
        audioManager = FindObjectOfType<AudioManager>();

        if (transitionInfo.playerWonRole == null) { return; }

        if (transitionInfo.playerWonRole == 0)
        {
            circleRays.GetComponent<Image>().sprite = redRays;
            return;
        }

        circleRays.GetComponent<Image>().sprite = blueRays;
    }
}