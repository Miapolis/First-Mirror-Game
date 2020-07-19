using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using Mirror;
using UnityEngine;

public class CreateTicTacToePlayerMessage : MessageBase
{
	public int role;
}

public class ModifiedNetworkManager : NetworkManager
{
	public static event Action OnServerDisconnected;
	public static event Action OnClientDisconnected;
	

	private GameManager gameManager;

	public override void OnStartClient()
	{
		gameManager = FindObjectOfType<GameManager>();

		var spawnablePrefabs = Resources.LoadAll<GameObject>("NetworkPrefabs");

		foreach (var prefab in spawnablePrefabs)
		{
			ClientScene.RegisterPrefab(prefab);
		}

		base.OnStartClient();
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);

		var charMessage = new CreateTicTacToePlayerMessage
		{
			role = numPlayers == 0 ? 0 : 1
		};

		conn.Send(charMessage);
	}

	public void OnCreatePlayer(NetworkConnection conn, CreateTicTacToePlayerMessage message)
	{
		GameObject gameobject = Instantiate(playerPrefab, GameObject.FindGameObjectWithTag("Player Container").transform, false);

		NetworkServer.AddPlayerForConnection(conn, gameobject);
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		NetworkServer.RegisterHandler<CreateTicTacToePlayerMessage>(OnCreatePlayer);
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);

		OnServerDisconnected?.Invoke();
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		base.OnClientDisconnect(conn);

		OnClientDisconnected?.Invoke();
	}
}