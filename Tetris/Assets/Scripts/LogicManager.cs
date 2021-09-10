using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogicManager : MonoBehaviour, ILogicManager 
{
	public enum GameState 
	{
		None, PreGame, InPlay, Paused, GameOver
	}

	public delegate void GameStateChangeEventHandler(GameState gameState);
	public delegate void GameUpdateEventHandler();

	public event GameStateChangeEventHandler GameStateChange;
	public event GameUpdateEventHandler GameUpdate;

	[SerializeField] private Vector2Int gridSize = new Vector2Int(10, 24);
	
	[Header("Controls")]
	[SerializeField] private KeyCode moveLeftKey = KeyCode.LeftArrow;
	[SerializeField] private KeyCode moveRightKey = KeyCode.RightArrow;
	[SerializeField] private KeyCode softDropKey = KeyCode.DownArrow;
	[SerializeField] private KeyCode hardDropKey = KeyCode.Space;
	[SerializeField] private KeyCode rotateKey = KeyCode.UpArrow;
	[SerializeField] private KeyCode startGameKey = KeyCode.Space;
	[SerializeField] private KeyCode pauseGameKey = KeyCode.P;

	private int[,,] iPiece = new int[,,] 
	{
		{
			{ 0, 0, 0, 0 },
			{ 1, 1, 1, 1 },
			{ 0, 0, 0, 0 },
			{ 0, 0, 0, 0 }
		}, 
		{
			{ 0, 0, 1, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 1, 0 },
			{ 0, 0, 1, 0 }
		}
	};
	private int[,,] jPiece = new int[,,] 
	{
		{
			{ 2, 0, 0 },
			{ 2, 2, 2 },
			{ 0, 0, 0 }
		}, 
		{
			{ 0, 2, 2 },
			{ 0, 2, 0 },
			{ 0, 2, 0 }
		}, 
		{
			{ 0, 0, 0 },
			{ 2, 2, 2 },
			{ 0, 0, 2 }
		}, 
		{
			{ 0, 2, 0 },
			{ 0, 2, 0 },
			{ 2, 2, 0 }
		}
	};
	private int[,,] lPiece = new int[,,]
	{
		{
			{ 0, 0, 3 },
			{ 3, 3, 3 },
			{ 0, 0, 0 }
		},
		{
			{ 3, 3, 0 },
			{ 0, 3, 0 },
			{ 0, 3, 0 }
		},
		{
			{ 0, 0, 0 },
			{ 3, 3, 3 },
			{ 3, 0, 0 }
		},
		{
			{ 0, 3, 0 },
			{ 0, 3, 0 },
			{ 0, 3, 3 }
		}
	};
	private int[,,] oPiece = new int[,,]
	{
		{
			{ 4, 4 },
			{ 4, 4 }
		}
	};
	private int[,,] sPiece = new int[,,]
	{
		{
			{ 0, 5, 5 },
			{ 5, 5, 0 },
			{ 0, 0, 0 }
		},
		{
			{ 0, 5, 0 },
			{ 0, 5, 5 },
			{ 0, 0, 5 }
		}
	};
	private int[,,] zPiece = new int[,,]
	{
		{
			{ 6, 6, 0 },
			{ 0, 6, 6 },
			{ 0, 0, 0 }
		},
		{
			{ 0, 0, 6 },
			{ 0, 6, 6 },
			{ 0, 6, 0 }
		}
	};
	private int[,,] tPiece = new int[,,]
	{
		{
			{ 0, 7, 0 },
			{ 7, 7, 7 },
			{ 0, 0, 0 }
		},
		{
			{ 0, 7, 0 },
			{ 0, 7, 7 },
			{ 0, 7, 0 }
		},
		{
			{ 0, 0, 0 },
			{ 7, 7, 7 },
			{ 0, 7, 0 }
		},
		{
			{ 0, 7, 0 },
			{ 7, 7, 0 },
			{ 0, 7, 0 }
		}
	};
	private int[][,,] allPieces;

	private float fallTimer = 0f;

	public Vector2Int GridSize => gridSize;
	public int[,] FixedPieces { get; private set; }
	public int[,,] ActivePiece { get; private set; }
	public Vector2Int ActivePiecePosition { get; private set; }
	public int ActivePieceRotation { get; private set; }
	public GameState CurrentGameState { get; private set; }
	public int Level => throw new System.NotImplementedException();
	public int Score => throw new System.NotImplementedException();
	
	public Vector2Int GetActivePieceHardDropPosition() 
	{
		throw new System.NotImplementedException();
	}

	public int[,,] GetNextPieceInBag() 
	{
		throw new System.NotImplementedException();
	}
	private void Awake() 
	{
		FixedPieces = new int[gridSize.y, gridSize.x];

		allPieces = new int[][,,] 
		{
			iPiece,
			jPiece,
			lPiece,
			oPiece,
			sPiece,
			zPiece,
			tPiece
		};
		ResetGame();
		ActivePiece = allPieces[1];
	}

	private void Update() 
	{
		GameUpdate();

		fallTimer = fallTimer + Time.deltaTime;

		if (Input.GetKeyDown(moveRightKey))
        {
			if (!CheckOverlap(ActivePiece, ActivePieceRotation, ActivePiecePosition + Vector2Int.right))
			{
				ActivePiecePosition += Vector2Int.right;
			}
		}
		if (Input.GetKeyDown(moveLeftKey))
		{
			if (!CheckOverlap(ActivePiece, ActivePieceRotation, ActivePiecePosition + Vector2Int.left))
			{
				ActivePiecePosition += Vector2Int.left;
			}
		}
		if (Input.GetKeyDown(softDropKey) || fallTimer >= 1f)
		{
			if (ActivePiecePosition.y == 0)
            {
				return;
            }
			ActivePiecePosition += Vector2Int.down;
			fallTimer = 0f;
		}
	}

	private void ResetGame() 
	{
		FixedPieces = new int[gridSize.y, gridSize.x];
		ChangeGameState(GameState.PreGame);

		ActivePiecePosition = ActivePiecePosition + Vector2Int.up * 20;
		ActivePiecePosition = ActivePiecePosition + Vector2Int.right * 4;

		ChangeGameState(GameState.InPlay);
	}

	private void ChangeGameState(GameState gameState) 
	{
		if (CurrentGameState != gameState) 
		{
			CurrentGameState = gameState;
			GameStateChange?.Invoke(CurrentGameState);
		}
	}

	private Vector2Int GetPieceSize(int[,,] piece) 
	{
		return new Vector2Int(piece.GetLength(2), piece.GetLength(1));
	}

	private bool CheckOverlap(int[,,] piece, int pieceRotation, Vector2Int piecePosition)
	{
		Vector2Int pieceSize = GetPieceSize(piece);
		for (int x = 0 ; x < pieceSize.x ; x++)
		{
			for (int y = 0 ; y < pieceSize.y ; y++)
			{
				if (piece[pieceRotation, y, x] > 0)
				{
					if (x + piecePosition.x < 0)
					{ // check of links buiten arena
						return true;
					}
				}
			}
		}
    return false;
	}
}

