using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class GamePiece : NetworkBehaviour
{
	[SyncVar]
	public int tilePiecePosition;
	[SyncVar]
	public int rank;

	private GameManager gameManager;

	private InfoPanel infoPanel;

	public void PiecePointerDown()
	{
		if (gameManager.roundEnded) { return; }

		LeanTween.scale(gameObject, new Vector3(2.2f, 2.2f), 0.1f);

		Invoke("Toggle", 0.7f);
	}

	public void PiecePointerUp()
	{
		CancelInvoke("Toggle");
		LeanTween.scale(gameObject, new Vector3(2f, 2f), 0.07f);
		infoPanel.Hide();
	}

	void Toggle()
	{
		infoPanel.SetParamsAndDisplay(rank, tilePiecePosition);
	}

	private void Start()
	{
		gameManager = FindObjectOfType<GameManager>();
		infoPanel = FindObjectOfType<InfoPanel>();
	}
}