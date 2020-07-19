using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Tile : NetworkBehaviour 
{
	private GameManager gameManager;

	[Client]
	public void GameTileClicked (int tileNum)
	{
		CmdSendClickToServer(netIdentity);

		Debug.Log("It's someone else's turn now!");
	}

	[Command]
	void CmdSendClickToServer (NetworkIdentity id)
	{
		
	}

	private void Start()
	{
		gameManager = FindObjectOfType<GameManager>();
	}
}
