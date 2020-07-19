using System.Collections;
using UnityEngine;

public class Screenshot : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitToTakeScreenshot());

        IEnumerator WaitToTakeScreenshot()
        {
            yield return new WaitForSeconds(0.3f);
            ScreenCapture.CaptureScreenshot("LEVEL MENU SCREENSHOT", 4);
            Debug.Log($"Screenshot taken at {Application.persistentDataPath}");
        }

    }
}