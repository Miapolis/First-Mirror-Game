using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
	[Header("UI")]
	public GameObject canvas;
	public GameObject overlayCanvas;
	public Camera mainCamera;
	public GameObject notTurnText;

	public TextMeshProUGUI localPlayerScore;
	public TextMeshProUGUI opponentPlayerScore;

	public Image currentPlayer;
	public Image dots;

	public GameObject lineRendererObj;
	public LineRenderer lineRenderer;

	public Sprite d1;
	public Sprite d2;
	public Sprite d3;

	public GameObject waitingForPlayerPanel;

	public GameObject clickCover;

	public Color blueLineColor;
	public Color redLineColor;

	private NetworkDiscovery networkDiscovery;

	[Header("Player Stats")]
	[SerializeField] private float dotsSpeed = 0.4f;
	// role 0 is x, role 1 is o
	[SyncVar]
	public int role;
	[SyncVar]
	public int currentScore;
	[SyncVar]
	public int opponentScore;

	[Header("Other")]
	[HideInInspector] public GameManager gameManager;
	private GameObject x;
	private GameObject o;

	private GameObject universalCanvas;

	public event Action ScaleUpFinished;
	public event Action DotsScaleDownFinished;

	[SyncVar]
	public bool dotsCoroutineStopped = false;

	private bool wrongTurnAlreadyShowing = false;

	protected bool actionMade = false;

	private SceneChangeManager sceneChangeManager;
	private TransitionInfo transitionInfo;

	private AudioManager audioManager;

	private InfoPanel infoPanel;

	[Space]
	[Header("Disconnection")]
	public GameObject disconnectPanel;

	[ClientCallback]
	public void GameTileClicked(int tileNum)
	{
		// Authorization
		if (!hasAuthority) { return; }

		if (gameManager.roundEnded) { return; }

		if (actionMade) { return; }

		if (role == gameManager.activeRole)
		{
			if (CheckPiece(tileNum)) { return; }

			actionMade = true;

			PlacePiece(tileNum);

			CmdGameManagerProcessPiece(tileNum);

			//REST TO BE DONE IN SEPERATE CONTINUE METHOD : 

		}
		else
		{
			if (!wrongTurnAlreadyShowing)
			{
				wrongTurnAlreadyShowing = true;

				var turnText = notTurnText;

				RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayCanvas.transform as RectTransform, Input.mousePosition, mainCamera, out Vector2 pos);
				turnText.transform.position = overlayCanvas.transform.TransformPoint(pos);
				turnText.transform.localScale = Vector3.one;

				Action<float> fadeAlpha = FadeAlpha;
				Action setWrongTurnFalse = WrongTurnFalse;

				turnText.SetActive(true);

				LeanTween.value(turnText, 1f, 0f, 0.7f).setOnUpdate(fadeAlpha);
				LeanTween.moveY(turnText, turnText.transform.position.y + 1f, 0.7f).setOnComplete(setWrongTurnFalse);

				void FadeAlpha(float value)
				{
					var mainColor = turnText.GetComponent<Text>().color;
					mainColor.a = value;
					turnText.GetComponent<Text>().color = mainColor;
				}

				void WrongTurnFalse()
				{
					turnText.SetActive(false);
					wrongTurnAlreadyShowing = false;
				}
			}
		}
	}

	[TargetRpc]
	public void TargetContinueWithPiecePlacement(NetworkConnection conn, bool alt, bool won)
	{
		if (alt)
		{
			CmdGameManagerSwitchRole();

			CmdShowDots();

			CmdHideDotsForOpponent();
		}
		else if (!won)
		{
			CmdGameManagerSwitchRole();

			CmdShowDots();

			CmdHideDotsForOpponent();
		}

		actionMade = false;

		return;
	}

	public bool CheckPiece(int tileNum)
	{
		if (gameManager.BoardContainsPiece(tileNum)) { return true; }

		return false;
	}

	public void PlacePiece(int tileNum)
	{
		int currentRole = role;

		switch (tileNum)
		{
			case 1:
				CmdPlaceObject(new Vector3(-250, -250, 0), currentRole, tileNum);

				break;
			case 2:

				CmdPlaceObject(new Vector3(0, -250, 0), currentRole, tileNum);
				break;

			case 3:

				CmdPlaceObject(new Vector3(250, -250, 0), currentRole, tileNum);
				break;

			case 4:

				CmdPlaceObject(new Vector3(-250, 0, 0), currentRole, tileNum);
				break;

			case 5:

				CmdPlaceObject(new Vector3(0, 0, 0), currentRole, tileNum);
				break;

			case 6:

				CmdPlaceObject(new Vector3(250, 0, 0), currentRole, tileNum);
				break;

			case 7:

				CmdPlaceObject(new Vector3(-250, 250, 0), currentRole, tileNum);
				break;

			case 8:

				CmdPlaceObject(new Vector3(0, 250, 0), currentRole, tileNum);
				break;

			case 9:

				CmdPlaceObject(new Vector3(250, 250, 0), currentRole, tileNum);
				break;
		}
	}

	[Command]
	public void CmdPlaceObject(Vector3 coordinates, int role, int tileNum)
	{
		GameObject gameObj = role == 0 ? Instantiate(x, coordinates, Quaternion.identity) : Instantiate(o, coordinates, Quaternion.identity);

		gameObj.GetComponent<GamePiece>().tilePiecePosition = tileNum;
		gameObj.GetComponent<GamePiece>().rank = role;

		NetworkServer.Spawn(gameObj);

		RpcScalePlacedObject(gameObj);
		RpcAssignCanvasParent(gameObj);
	}

	[ClientRpc]
	public void RpcScalePlacedObject(GameObject obj)
	{
		obj.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);

		LeanTween.scale(obj, new Vector3(2f, 2f, 2f), 0.3f).setOnUpdate(Scale);

		audioManager.PlaySound(3);

		void Scale(float val)
		{
			if (gameManager.roundEnded)
			{
				obj.transform.localScale = new Vector3(2f, 2f, 2f);
				return;
			}
		}
	}

	[Command]
	public void CmdGameManagerSwitchRole()
	{
		gameManager.SwitchActiveRole();
		RpcUpdateCurrentPlayerRep();
	}

	[Command]
	public void CmdGameManagerProcessPiece(int location)
	{
		gameManager.AddPieceToBoard(location, role, gameObject);
	}

	#region TEXT AND SCORES

	/// <summary>
	///	Called from GameManager to a specific instance of a player object
	/// to add one point to that instance of the player. Calls CmdWinThis afterwards.
	/// </summary>
	public void WinThis() => TargetWinThis(gameObject.GetComponent<NetworkIdentity>().connectionToClient);

	[TargetRpc]
	public void TargetWinThis(NetworkConnection conn) => CmdWinThis();

	[Command]
	public void CmdWinThis()
	{
		//SET SCORE FOR THIS
		currentScore++;
		RpcDisplayScoreForMe(gameObject, currentScore);

		foreach (var player in FindObjectsOfType<Player>())
		{
			if (player != this)
			{
				RpcDisplayMyScoreForOpponent(player.gameObject, this.currentScore);
			}
		}

		if (currentScore >= gameManager.playUpTo)
		{
			RpcDisplayWinnerWithThrophy(role);
		}
	}

	[ClientRpc]
	public void RpcDisplayWinnerWithThrophy(int playerWonRole)
	{
		foreach (var player in FindObjectsOfType<Player>())
		{
			player.EnableSprite(playerWonRole);
		}
	}

	public void EnableSprite(int playerWonRole)
	{
		StartCoroutine(WaitToEnable());

		IEnumerator WaitToEnable()
		{
			clickCover.SetActive(true);

			var image = clickCover.GetComponent<Image>();

			float alpha = 0f;

			while (alpha <= 1f)
			{
				image.color = new Color(0f, 0f, 0f, alpha);

				alpha += Time.deltaTime * 2.5f;

				yield return null;
			}

			transitionInfo.Reset();
			transitionInfo.playerWonRole = playerWonRole;
			transitionInfo.currentInstanceRole = this.role;

			yield return new WaitForSeconds(0.3f);

			var invokePlayer = FindObjectOfType<InvokePlayer>();

			invokePlayer.EndHostAndLoadScene(this.role);
		}
	}

	[ClientRpc]
	public void RpcDisplayScoreForMe(GameObject playerObj, int score)
	{
		var player = playerObj.GetComponent<Player>();
		player.localPlayerScore.text = score.ToString();
	}

	[ClientRpc]
	public void RpcDisplayMyScoreForOpponent(GameObject playerObj, int score)
	{
		var player = playerObj.GetComponent<Player>();
		player.opponentScore = score;
		player.SetTextFromOp(player.opponentScore);
	}

	public void SetTextFromOp(int level) => CmdSetTextFromOp(level);
	public void CmdSetTextFromOp(int level) => opponentPlayerScore.text = opponentScore.ToString();

	public void CleanUpAndReset() => CmdCleanUpAndReset();

	public void CmdCleanUpAndReset()
	{
		gameManager.ResetRound();
	}

	#endregion

	#region X/O switch

	[ClientRpc]
	public void RpcUpdateCurrentPlayerRep()
	{
		foreach (var player in FindObjectsOfType<Player>())
		{
			if (player.hasAuthority)
				player.UpdateThisRep();
		}
	}

	public void UpdateThisRep() => CmdUpdateThisRep();

	[Command]
	public void CmdUpdateThisRep() => RpcInitialScale();

	[ClientRpc]
	public void RpcInitialScale()
	{
		LeanTween.scale(currentPlayer.gameObject, new Vector3(6.75f, 1.08f), 0.15f).setOnComplete(ScaleUpFinished);
	}

	public void ScaleDone()
	{
		if (hasAuthority)
			CmdScaleDone();
	}

	[Command]
	public void CmdScaleDone() => RpcUpdateSprite();

	[ClientRpc]
	public void RpcUpdateSprite()
	{
		currentPlayer.sprite = gameManager.activeRole == 0 ? x.GetComponent<Image>().sprite : o.GetComponent<Image>().sprite;

		LeanTween.scale(currentPlayer.gameObject, new Vector3(6.25f, 1f), 0.15f);
	}

	#endregion

	[Command]
	public void CmdHideDotsForOpponent()
	{
		foreach (var player in FindObjectsOfType<Player>())
		{
			if (player != this)
			{
				TargetHideDotsForOpponent(player.gameObject.GetComponent<NetworkIdentity>().connectionToClient);

				return;
			}
		}
	}

	[TargetRpc]
	public void TargetHideDotsForOpponent(NetworkConnection conn)
	{
		foreach (var player in FindObjectsOfType<Player>())
		{
			if (player.hasAuthority)
			{
				player.dotsCoroutineStopped = true;
			}
		}
	}

	[Command]
	public void CmdShowDots() => RpcShowDots();

	[ClientRpc]
	public void RpcShowDots()
	{
		foreach (var player in FindObjectsOfType<Player>())
		{
			if (player == this)
			{
				dotsCoroutineStopped = false;
				dots.gameObject.SetActive(true);
				StartCoroutine(ShowPlayerTakingTurn());
			}
		}
	}

	#region DotsIENumerators+

	private IEnumerator ShowPlayerTakingTurn()
	{
		dots.sprite = d1;
		dots.gameObject.SetActive(true);

		int currentDot = 1;

		while (!dotsCoroutineStopped)
		{
			dots.gameObject.SetActive(true);

			var val = GetDot(currentDot);

			if (val != null)
			{
				dots.sprite = val;
			}
			else
			{
				dots.gameObject.SetActive(false);
			}

			if (currentDot != 4)
			{
				currentDot++;
			}
			else
			{
				currentDot = 1;
			}

			yield return new WaitForSeconds(dotsSpeed);
		}

		LeanTween.scale(dots.gameObject, new Vector3(1f, 0.16f, 0.16f), 0.4f).setEase(LeanTweenType.easeInBack).setOnComplete(DotsScaleDownFinished);
	}

	public void DisbaleDotsAndReset()
	{
		dots.gameObject.SetActive(false);

		dots.gameObject.transform.localScale = new Vector3(7.41f, 1.1856f, 1.1856f);
	}

	private Sprite GetDot(int dotNum)
	{
		if (dotNum == 1)
		{
			return d1;
		}
		else if (dotNum == 2)
		{
			return d2;
		}
		else if (dotNum == 3)
		{
			return d3;
		}

		return null;
	}

	#endregion

	public void GrayOutPieces(int tile1, int tile2, int tile3)
	{
		CmdGrayOutUnWonPieces(tile1, tile2, tile3);
	}

	[Command]
	public void CmdGrayOutUnWonPieces(int tile1, int tile2, int tile3)
	{
		RpcGrayOutUnWonPieces(tile1, tile2, tile3);
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

	[ClientRpc]
	public void RpcGrayOutUnWonPieces(int tile1, int tile2, int tile3)
	{
		if (tile1 != 10)
		{
			Vector3 tileOnePos = Vector3.zero;
			Vector3 tileThreePos = Vector3.zero;

			foreach (Transform child in universalCanvas.transform)
			{
				var tile = child.gameObject.GetComponent<GamePiece>().tilePiecePosition;

				if (!(tile == tile1 || tile == tile2 || tile == tile3))
				{
					child.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.3f);
				}
				else if (tile == tile1)
				{
					foreach (var player in FindObjectsOfType<Player>())
					{
						tileOnePos = GetPosition(tile);
						player.lineRenderer.SetPosition(0, tileOnePos);
					}
				}
				else if (tile == tile3)
				{
					foreach (var player in FindObjectsOfType<Player>())
					{
						tileThreePos = GetPosition(tile);
					}
				}
			}

			foreach (var player in FindObjectsOfType<Player>())
			{
				var color = this.role == 0 ? redLineColor : blueLineColor;

				player.lineRenderer.startColor = color;
				player.lineRenderer.endColor = color;

				player.DrawLineStart(tileOnePos, tileThreePos);
			}

			StartWait(0);
		}
		else
		{
			foreach (Transform child in universalCanvas.transform)
			{
				child.gameObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.3f);
			}

			StartWait(1);
		}

		void StartWait(int state)
		{
			gameManager.EndRound();
			StartCoroutine(WaitToClearPieces(state == 0 ? 2 : 1));
		}

		IEnumerator WaitToClearPieces(int seconds)
		{
			yield return new WaitForSeconds(seconds);

			ClearAllPieces();
		}

		void ClearAllPieces()
		{
			foreach (var player in FindObjectsOfType<Player>())
			{
				player.lineRendererObj.SetActive(false);
			}

			foreach (Transform child in universalCanvas.transform)
			{
				NetworkServer.Destroy(child.gameObject);
			}

			gameManager.ResetRound();

			infoPanel.Hide();
		}
	}

	void DrawLineStart(Vector3 tile1, Vector3 tile3)
	{
		StartCoroutine(DrawLine(tile1, tile3));
	}

	IEnumerator DrawLine(Vector3 origin, Vector3 destination)
	{
		lineRendererObj.SetActive(true);

		var distance = Vector3.Distance(origin, destination);
		float counter = 0;

		while (counter < distance)
		{
			counter += Time.deltaTime * 4.5f;

			float x = Mathf.Lerp(0, -distance, counter);

			Vector3 linePoint = x * Vector3.Normalize(origin - destination) + origin;

			lineRenderer.SetPosition(1, linePoint);

			yield return null;
		}
	}

	#region Disconnection Handling 

	void ServerDisconnected()
	{
		if (transitionInfo.playerWonRole != null) { return; }

		if (transitionInfo.canceledHost) { return; }

		if (clickCover == null) { return; }

		clickCover.SetActive(true);
		disconnectPanel.SetActive(true);
	}

	void ClientDisconnected()
	{
		if (transitionInfo.playerWonRole != null) { return; }

		if (transitionInfo.canceledHost) { return; }

		SceneManager.LoadScene(3);
	}

	#endregion

	#region Startup

	public override void OnStartLocalPlayer()
	{
		base.OnStartLocalPlayer();

		mainCamera = FindObjectOfType<Camera>();

		canvas.SetActive(true);
		overlayCanvas.SetActive(true);

		if (role == 0)
		{
			waitingForPlayerPanel.SetActive(true);
		}

		if (role == 1)
		{
			CmdDisableWaitPanel();

			CmdShowDots();
		}
	}

	[Command]
	public void CmdDisableWaitPanel()
	{
		foreach (var player in FindObjectsOfType<Player>())
		{
			if (player != this)
			{
				player.RpcDisableWaitPanel();
			}
		}
	}

	[ClientRpc]
	public void RpcDisableWaitPanel()
	{
		waitingForPlayerPanel.SetActive(false);
	}

	private void Start()
	{
		canvas.GetComponent<Canvas>().worldCamera = mainCamera;
		overlayCanvas.GetComponent<Canvas>().worldCamera = mainCamera;

		universalCanvas = GameObject.FindGameObjectWithTag("Canvas");

		networkDiscovery = FindObjectOfType<NetworkDiscovery>();

		gameManager = FindObjectOfType<GameManager>();
		sceneChangeManager = FindObjectOfType<SceneChangeManager>();
		transitionInfo = FindObjectOfType<TransitionInfo>();
		audioManager = FindObjectOfType<AudioManager>();

		infoPanel = FindObjectOfType<InfoPanel>();

		x = gameManager.x;
		o = gameManager.o;

		ScaleUpFinished += ScaleDone;
		DotsScaleDownFinished += DisbaleDotsAndReset;

		ModifiedNetworkManager.OnServerDisconnected += ServerDisconnected;
		ModifiedNetworkManager.OnClientDisconnected += ClientDisconnected;
	}

	public override void OnStartServer()
	{
		base.OnStartServer();

		role = NetworkManager.singleton.numPlayers == 1 ? 0 : 1;
		currentScore = 0;
	}

	[ClientRpc]
	public void RpcAssignCanvasParent(GameObject obj)
	{
		obj.transform.SetParent(universalCanvas.transform, false);
	}

	#endregion
}