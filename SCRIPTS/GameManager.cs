using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SyncListBoardObject : SyncList<BoardObject> { }

public class GameManager : NetworkBehaviour
{
	public SyncListBoardObject board = new SyncListBoardObject();

	//0 or 1
	[SyncVar]
	public int activeRole = 0;
	[SyncVar]
	public int currentRound;
	[SyncVar]
	public int playUpTo;

	[SyncVar]
	public bool turnsAlternateAfterEndOfRound;

	[SyncVar]
	public bool gameEnded = false;

	public GameObject x;
	public GameObject o;

	[SyncVar]
	public bool roundEnded = false;

	public bool currentTryPieceExists = false;

	private ModifiedNetworkManager networkManager;

	private GameConfiguration gameConfiguration;

	private enum CheckingState { HorizontalRight, VerticalUp, DTopRight, DTopLeft }

	private CheckingState currentState;

	public void SwitchActiveRole()
	{
		if (activeRole == 0)
		{
			activeRole = 1;
		}
		else
		{
			activeRole = 0;
		}
	}

	[ServerCallback]
	public void EndRound()
	{
		roundEnded = true;
	}

	[ServerCallback]
	public void ResetRound()
	{
		roundEnded = false;

		ResetBoard();
	}

	[ServerCallback]
	public void ResetBoard()
	{
		board.Clear();
	}

	private void Start()
	{
		activeRole = 0;

		networkManager = FindObjectOfType<ModifiedNetworkManager>();
		gameConfiguration = FindObjectOfType<GameConfiguration>();

		gameObject.SetActive(true);

		this.playUpTo = gameConfiguration.playUpTo;
		turnsAlternateAfterEndOfRound = gameConfiguration.turnsAlternateAfterEndOfRound;
	}

	public void AddPieceToBoard(int location, int value, GameObject player)
	{
		board.Add(new BoardObject
		{
			tileNumber = location,
				pieceValue = value
		});

		var won = CalculateBoardState(player);

		player.GetComponent<Player>().TargetContinueWithPiecePlacement(player.GetComponent<NetworkIdentity>().connectionToClient, turnsAlternateAfterEndOfRound, won);
	}

	public bool BoardContainsPiece(int value)
	{
		return board.Any(x => x.tileNumber == value);
	}

	[ServerCallback]
	public bool CalculateBoardState(GameObject player)
	{
		foreach (var piece in board)
		{
			currentState = CheckingState.HorizontalRight;

			if (piece.tileNumber == 1 || piece.tileNumber == 4 || piece.tileNumber == 7)
			{
				var is0 = board.Any(x => x.tileNumber == piece.tileNumber && x.pieceValue == 0);

				int rankValue = is0 ? 0 : 1;

				if (board.Any(x => x.tileNumber == piece.tileNumber + 1 && x.pieceValue == rankValue))
				{
					if (board.Any(x => x.tileNumber == piece.tileNumber + 2 && x.pieceValue == rankValue))
					{
						Debug.Log($"Rank {rankValue} won! Info: {currentState}");

						player.GetComponent<Player>().RpcGrayOutUnWonPieces(piece.tileNumber, piece.tileNumber + 1, piece.tileNumber + 2);
						player.GetComponent<Player>().WinThis();

						return true;
					}
				}
			}

			currentState = CheckingState.VerticalUp;

			if (piece.tileNumber == 1 || piece.tileNumber == 2 || piece.tileNumber == 3)
			{
				var is0 = board.Any(x => x.tileNumber == piece.tileNumber && x.pieceValue == 0);

				int rankValue = is0 ? 0 : 1;

				if (board.Any(x => x.tileNumber == piece.tileNumber + 3 && x.pieceValue == rankValue))
				{
					if (board.Any(x => x.tileNumber == piece.tileNumber + 6 && x.pieceValue == rankValue))
					{
						Debug.Log($"Rank {rankValue} won! Info: {currentState}");

						player.GetComponent<Player>().RpcGrayOutUnWonPieces(piece.tileNumber, piece.tileNumber + 3, piece.tileNumber + 6);
						player.GetComponent<Player>().WinThis();

						return true;
					}
				}
			}

			currentState = CheckingState.DTopRight;

			if (piece.tileNumber == 1)
			{
				var is0 = board.Any(x => x.tileNumber == piece.tileNumber && x.pieceValue == 0);

				int rankValue = is0 ? 0 : 1;

				if (board.Any(x => x.tileNumber == piece.tileNumber + 4 && x.pieceValue == rankValue))
				{
					if (board.Any(x => x.tileNumber == piece.tileNumber + 8 && x.pieceValue == rankValue))
					{
						Debug.Log($"Rank {rankValue} won! Info: {currentState}");

						player.GetComponent<Player>().RpcGrayOutUnWonPieces(piece.tileNumber, piece.tileNumber + 4, piece.tileNumber + 8);
						player.GetComponent<Player>().WinThis();

						return true;
					}
				}
			}

			currentState = CheckingState.DTopLeft;

			if (piece.tileNumber == 3)
			{
				var is0 = board.Any(x => x.tileNumber == piece.tileNumber && x.pieceValue == 0);

				int rankValue = is0 ? 0 : 1;

				if (board.Any(x => x.tileNumber == piece.tileNumber + 2 && x.pieceValue == rankValue))
				{
					if (board.Any(x => x.tileNumber == piece.tileNumber + 4 && x.pieceValue == rankValue))
					{
						Debug.Log($"Rank {rankValue} won! Info: {currentState}");

						player.GetComponent<Player>().RpcGrayOutUnWonPieces(piece.tileNumber, piece.tileNumber + 2, piece.tileNumber + 4);
						player.GetComponent<Player>().WinThis();

						return true;
					}
				}
			}
		}

		if (board.Count >= 9)
		{
			player.GetComponent<Player>().RpcGrayOutUnWonPieces(10, 10, 10);
			return false;
		}

		return false;
	}
}