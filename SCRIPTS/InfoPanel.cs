using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPanel : MonoBehaviour
{
    private string xStatement = "This is a standard X\n-\nThis is the default piece for the host";
    private string oStatement = "This is a standard O\n-\nThis is the default piece for the client joining";

    public GameObject panel;
    public GameObject section;

    public Sprite x;
    public Sprite o;

    public Image logoIcon;
    public TextMeshProUGUI text;
    [Space]
    public Sprite r1;
    public Sprite l1;

    public void SetParamsAndDisplay(int pieceHovered, int locationOfPiece)
    {
        if (pieceHovered == 0)
        {
            logoIcon.sprite = x;
            text.text = xStatement;
        }
        else
        {
            logoIcon.sprite = o;
            text.text = oStatement;
        }

        var y = locationOfPiece;

        var image = panel.GetComponent<Image>();

        panel.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        section.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        image.sprite = r1;
        panel.GetComponent<RectTransform>().pivot = new Vector2(1, 0);

        if (y == 3 || y == 6 || y == 9)
        {
            image.sprite = l1;
            panel.GetComponent<RectTransform>().pivot = new Vector2(0, 0);
        }

        if (y == 7 || y == 8 || y == 9)
        {
            panel.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            section.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        }

        panel.transform.localPosition = GetPosition(locationOfPiece);

        if (y == 7 || y == 8)
        {
            image.sprite = l1;
            panel.transform.localPosition = new Vector3(panel.transform.localPosition.x - 250f, panel.transform.localPosition.y, panel.transform.localPosition.z);
        }

        if (y == 9)
        {
            image.sprite = r1;
            panel.transform.localPosition = new Vector3(panel.transform.localPosition.x + 250f, panel.transform.localPosition.y, panel.transform.localPosition.z);
        }

        panel.SetActive(true);

        LeanTween.scale(panel, Standards.V3Equal(0.53f), 0.1f).setLoopPingPong(1);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    private Vector3 GetPosition(int pos)
    {
        switch (pos)
        {
            case 1:

                return new Vector3(-250, -250, 0);

            case 2:

                return new Vector3(0, -250, 0);

            case 3:

                return new Vector3(250, -250, 0);

            case 4:

                return new Vector3(-250, 0, 0);

            case 5:
                return new Vector3(0, 0, 0);

            case 6:

                return new Vector3(250, 0, 0);

            case 7:

                return new Vector3(-250, 250, 0);

            case 8:

                return new Vector3(0, 250, 0);

            case 9:

                return new Vector3(250, 250, 0);

            default:

                throw new IndexOutOfRangeException("Tile must be from 1-9");
        }
    }
}