using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChangeManager : MonoBehaviour
{
    public static event Action OnStopHostingCancel;

    private TransitionInfo transitionInfo;

    [SerializeField] private GameObject fadePanel;

    public void ChangeScene(string sceneName)
    {
        InternalChangeScene(sceneName);
    }

    // Start is called before the first frame update
    void Start()
    {
        transitionInfo = FindObjectOfType<TransitionInfo>();

        if (transitionInfo == null) { Debug.LogWarning("TransitionInfo was not found!"); }

        if (transitionInfo.overrideParamaters) { return; }

        #region  Startup in Some Scenes

        if ((transitionInfo.canceledHost || transitionInfo.returningFromWin) && SceneManager.GetActiveScene().name == "Main")
        {
            fadePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
            fadePanel.SetActive(true);

            transitionInfo.Reset();

            StartCoroutine(FadePanel(1));
        }

        if (SceneManager.GetActiveScene().name.Equals("Win"))
        {
            fadePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 1f);
            fadePanel.SetActive(true);
            StartCoroutine(FadePanel(2));
        }

        #endregion
    }

    void InternalChangeScene(string sceneName)
    {
        if (transitionInfo.overrideParamaters)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        if (transitionInfo.canceledHost || transitionInfo.returningFromWin)
        {
            fadePanel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            fadePanel.SetActive(true);
            StartCoroutine(FadePanel(0, "Main"));
            return;
        }
    }

    /// <summary>
    /// Fades Panel in and out depending on the scene
    /// </summary>
    /// <param name = "state">0 for fading in, 1 for fading the panel out.</param>
    /// <param name = "sceneName">The scene to load to. Not necessariy to provide.</param>
    IEnumerator FadePanel(int state, string sceneName = "")
    {
        var image = fadePanel.GetComponent<Image>();

        if (state == 0)
        {
            float alpha = 0f;

            while (alpha <= 1f)
            {
                image.color = new Color(0f, 0f, 0f, alpha);

                alpha += Time.deltaTime * transitionInfo.panelFadeTime;

                yield return null;
            }

            OnStopHostingCancel?.Invoke();

            SceneManager.LoadScene(sceneName);
        }
        else if (state == 1)
        {
            float alpha = 1f;

            while (alpha >= 0f)
            {
                image.color = new Color(0f, 0f, 0f, alpha);

                alpha -= Time.deltaTime * transitionInfo.panelFadeTime;

                yield return null;
            }

            fadePanel.SetActive(false);
        }
        else if (state == 2)
        {
            float alpha = 1f;

            while (alpha >= 0f)
            {
                image.color = new Color(0f, 0f, 0f, alpha);

                alpha -= Time.deltaTime * transitionInfo.panelFadeTime * 3f;

                yield return null;
            }

            fadePanel.SetActive(false);
        }
    }
}