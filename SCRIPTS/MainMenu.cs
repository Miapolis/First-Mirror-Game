using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject banner;

    public GameObject button1;
    public GameObject button2;

    public GameObject loadingBar;
    public Image loadingBarFill;

    private GameConfiguration gameConfiguration;

    [Header("Hosting Menu")]
    public GameObject hostMenu;
    public TextMeshProUGUI pointsUpToText;
    public GameObject check;
    public GameObject hostButton;
    public GameObject hostCancelButton;

    bool checkPresent;

    bool hostMenuFullyDisplayed;

    private ModifiedNetworkManager networkManager;

    private AudioManager manager;

    protected bool mainMenuSoundsMuted;

    bool hostButtonPressed = false;
    bool hostCanvasInitiated = false;
    bool joinButtonPressed = false;
    bool startHostingButtonPressed = false;

    bool completedHostSetupProcess = false;

    const string HOST_DEFAULT_POINT_WIN_VALUE_KEY = "hostDefaultPointWinValue";
    const string HOST_ALTERNATE_TURNS_SCORING_KEY = "hostAlternateTurnsAfterScoring";

    protected bool rapidNumberChangeNeedsToStop = false;

    // Start is called before the first frame update
    void Start()
    {
        networkManager = FindObjectOfType<ModifiedNetworkManager>();
        gameConfiguration = FindObjectOfType<GameConfiguration>();
        manager = FindObjectOfType<AudioManager>();

        mainMenuSoundsMuted = false;

        gameConfiguration.playUpTo = PlayerPrefs.GetInt(HOST_DEFAULT_POINT_WIN_VALUE_KEY, 10);
        gameConfiguration.turnsAlternateAfterEndOfRound = PlayerPrefs.GetInt(HOST_ALTERNATE_TURNS_SCORING_KEY, 1) == 1 ? true : false;

        check.SetActive(gameConfiguration.turnsAlternateAfterEndOfRound);
        pointsUpToText.text = gameConfiguration.playUpTo.ToString();

        hostMenuFullyDisplayed = false;

        ScaleUp();

        void ScaleUp()
        {
            LeanTween.scale(banner, Standards.V3Equal(1.1f), 2.5f).setEaseInOutSine().setLoopPingPong();
        }
    }

    public void StartHosting()
    {
        if (startHostingButtonPressed) { return; }
        startHostingButtonPressed = true;

        manager.PlaySound(1);

        PlayerPrefs.SetInt(HOST_DEFAULT_POINT_WIN_VALUE_KEY, gameConfiguration.playUpTo);
        PlayerPrefs.SetInt(HOST_ALTERNATE_TURNS_SCORING_KEY, gameConfiguration.turnsAlternateAfterEndOfRound == true ? 1 : 0);

        LeanTween.moveLocal(hostMenu, new Vector3(0f, 975f, 0f), 0.3f).setOnComplete(DisableMenu).setEaseInBack();

        mainMenuSoundsMuted = true;

        void DisableMenu()
        {
            hostMenu.SetActive(false);
            hostMenuFullyDisplayed = false;
            hostCancelButton.transform.localScale = Vector3.one;
            SetUpCanvas(2);
        }
    }

    //0 for host, 1 for join
    public void SetUpCanvas(int state)
    {
        if (state == 1)
        {
            MoveAllButtonsAndLoad();
        }
        else if (state == 2)
        {
            MoveAllButtonsAndLoad();
        }
        else
        {
            if (hostCanvasInitiated) { return; }

            hostCanvasInitiated = true;

            //Enable Panel
            hostMenu.transform.localPosition = new Vector3(0f, 975f, 0f);
            hostMenu.SetActive(true);

            LeanTween.moveLocal(hostMenu, Vector3.zero, 1f).setEaseOutBounce().setOnComplete(FullyDisplay);

            void FullyDisplay()
            {
                hostButtonPressed = false;
                hostCanvasInitiated = false;
                hostMenuFullyDisplayed = true;
            }

            return;
        }

        void MoveAllButtonsAndLoad()
        {
            completedHostSetupProcess = true;

            mainMenuSoundsMuted = true;
            bool initiated = false;

            LeanTween.moveLocal(button2, new Vector3(0f, -545f, 0f), 0.5f).setEaseInOutBack();
            LeanTween.moveLocal(button1, new Vector3(0f, -545f, 0f), 0.5f).setEaseInOutBack().setDelay(0.15f);
            LeanTween.moveLocal(banner, Vector3.zero, 0.4f).setEaseOutSine().setDelay(0.4f);
            StartCoroutine(ShowLoadingBar());

            IEnumerator ShowLoadingBar()
            {
                yield return new WaitForSeconds(2f);

                loadingBar.SetActive(true);

                bool loadingBarDone = false;

                while (!loadingBarDone)
                {
                    float randomNumerToIncreaseBy = Random.Range(0.01f, 0.05f);
                    loadingBarFill.fillAmount += randomNumerToIncreaseBy;

                    if (!initiated && loadingBarFill.fillAmount >= 0.5f)
                    {
                        initiated = true;
                        InitiateTrigger();
                    }

                    if (loadingBarFill.fillAmount >= 1f)
                    {
                        loadingBarDone = true;
                    }

                    yield return new WaitForSeconds(0.15f);
                }

                SceneManager.LoadScene(1);
            }

            void InitiateTrigger()
            {
                LeanTween.moveLocal(banner, new Vector3(0f, 700f, 0f), 0.5f).setEaseInSine();
                LeanTween.moveLocal(loadingBar, Vector3.zero, 0.5f).setEaseOutSine().setDelay(0.5f);
            }
        }
    }

    public void ButtonClick(int buttonNum)
    {
        if (buttonNum == 1)
        {
            if (completedHostSetupProcess) { return; }
            if (hostButtonPressed) { return; }

            hostButtonPressed = true;

            if (!mainMenuSoundsMuted)
                manager.PlaySound(1);

            var button = button1;
            button.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            gameConfiguration.role = 0;
        }
        else
        {
            if (joinButtonPressed) { return; }

            joinButtonPressed = true;

            if (!mainMenuSoundsMuted)
                manager.PlaySound(1);

            var button = button2;
            button.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            gameConfiguration.role = 1;
        }
    }

    public void ButtonPointerUp(int buttonNum)
    {
        if (buttonNum == 1)
        {
            var button = button1;
            button.GetComponent<Image>().color = Standards.RGBEqual(233f / 255f);
        }
        else
        {
            var button = button2;
            button.GetComponent<Image>().color = Standards.RGBEqual(233f / 255f);
        }
    }

    #region Host Menu 

    public void CheckClicked()
    {
        if (!mainMenuSoundsMuted)
            manager.PlaySound(1);

        if (check.activeSelf)
        {
            check.SetActive(false);
            gameConfiguration.turnsAlternateAfterEndOfRound = false;
        }
        else
        {
            check.SetActive(true);
            gameConfiguration.turnsAlternateAfterEndOfRound = true;
        }
    }

    //0 = down, 1 = up
    public void ArrowClicked(GameObject arrow)
    {
        if (!mainMenuSoundsMuted)
            manager.PlaySound(1);

        arrow.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }

    public void ArrowClickedChangeNumber(int value)
    {
        if (value == 0)
        {
            if (gameConfiguration.playUpTo <= 1) { return; }

            gameConfiguration.playUpTo--;
            Invoke("StartDecreaseNumber", 0.7f);
        }
        else
        {
            if (gameConfiguration.playUpTo >= 99) { return; }

            gameConfiguration.playUpTo++;
            Invoke("StartIncreaseNumber", 0.7f);
        }

        var playUpTo = gameConfiguration.playUpTo;
        pointsUpToText.text = playUpTo.ToString();
    }

    void StartIncreaseNumber() => StartCoroutine(IncreaseNumber());

    void StartDecreaseNumber() => StartCoroutine(DecreaseNumber());

    IEnumerator IncreaseNumber()
    {
        rapidNumberChangeNeedsToStop = false;

        while (!rapidNumberChangeNeedsToStop && gameConfiguration.playUpTo < 99)
        {
            gameConfiguration.playUpTo++;

            pointsUpToText.text = gameConfiguration.playUpTo.ToString();

            manager.PlaySound(1);

            yield return new WaitForSeconds(0.09f);
        }
    }

    IEnumerator DecreaseNumber()
    {
        rapidNumberChangeNeedsToStop = false;

        while (!rapidNumberChangeNeedsToStop && gameConfiguration.playUpTo > 1)
        {
            gameConfiguration.playUpTo--;

            pointsUpToText.text = gameConfiguration.playUpTo.ToString();

            manager.PlaySound(1);

            yield return new WaitForSeconds(0.09f);
        }
    }

    public void ArrowPointerUp(GameObject arrow)
    {
        rapidNumberChangeNeedsToStop = true;
        arrow.transform.localScale = Vector3.one;

        CancelInvoke("StartIncreaseNumber");
        CancelInvoke("StartDecreaseNumber");
    }

    public void HideHostMenu()
    {
        PlayerPrefs.SetInt(HOST_DEFAULT_POINT_WIN_VALUE_KEY, gameConfiguration.playUpTo);
        PlayerPrefs.SetInt(HOST_ALTERNATE_TURNS_SCORING_KEY, gameConfiguration.turnsAlternateAfterEndOfRound == true ? 1 : 0);

        LeanTween.moveLocal(hostMenu, new Vector3(0f, 975f, 0f), 0.3f).setOnComplete(DisableMenu).setEaseInBack();

        void DisableMenu()
        {
            hostMenu.SetActive(false);
            hostMenuFullyDisplayed = false;
            hostCancelButton.transform.localScale = Vector3.one;
        }
    }

    public void PressCancelButton()
    {
        if (!mainMenuSoundsMuted)
            manager.PlaySound(0);
        hostCancelButton.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }

    public void ReleaseCancelButton()
    {
        hostCancelButton.transform.localScale = Vector3.one;
    }

    #endregion
}