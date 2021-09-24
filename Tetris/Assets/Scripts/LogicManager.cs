using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// With help from Boas, Justin, Celine and Darryl (3GD2)

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
    private List<int> randomBag;
    private float fallTimer = 0f;
    private float maxFallTimer = 1f;
    private int collectiveRowsCleared;

    public Vector2Int GridSize => gridSize;
    public int[,] FixedPieces { get; private set; }
    public int[,,] ActivePiece { get; private set; }
    public Vector2Int ActivePiecePosition { get; private set; }
    public int ActivePieceRotation { get; private set; }
    public GameState CurrentGameState { get; private set; }
    public int currentLevel { get; private set; }
    public int Score { get; private set; }

    public Vector2Int GetActivePieceHardDropPosition()
    {
        throw new System.NotImplementedException();
    }

    public int[,,] GetNextPieceInBag()
    {
        throw new System.NotImplementedException();
    }

    private void RandomizeNewBag()
    {
        List<int> options = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
        for (int i = 0 ; i < allPieces.Length ; i++)
        {
            int random = Random.Range(0, options.Count);
            randomBag.Add(options[random]);
            options.RemoveAt(random);
        }
    }

    private void CheckRows()
    {
        int _combo = 0; // used for combo's of multiple rows
        for (int y = 0 ; y < GridSize.y ; y++)
        {
            int _fullRow = 0;
            for (int x = 0 ; x < GridSize.x ; x++)
            {
                if (FixedPieces[y, x] > 0)
                    _fullRow++;

                if (_fullRow == GridSize.x)
                {
                    // move every other row one down
                    for (int gridY = y ; gridY < GridSize.y ; gridY++)
                    {
                        for (int gridX = 0 ; gridX < GridSize.x ; gridX++)
                        {
                            if (gridY + 1 < GridSize.y)
                                FixedPieces[gridY, gridX] = FixedPieces[gridY + 1, gridX];
                        }
                        GameUpdate?.Invoke();
                    }
                    _combo++;

                    y--;
                }
            }
        }
        UpdateLevel(_combo);
    } // made mostly by Justin

    private void UpdateLevel(int currentRowsCleared)
    {
        collectiveRowsCleared += currentRowsCleared;

        if (collectiveRowsCleared >= 0 && collectiveRowsCleared <= 9)
        {
            maxFallTimer = 1f;
            currentLevel = 1;
        }
        if (collectiveRowsCleared >= 10 && collectiveRowsCleared <= 19)
        {
            maxFallTimer = 0.8f;
            currentLevel = 2;
        }
        if (collectiveRowsCleared >= 20 && collectiveRowsCleared <= 29)
        {
            maxFallTimer = 0.6f;
            currentLevel = 3;
        }
        if (collectiveRowsCleared >= 30 && collectiveRowsCleared <= 39)
        {
            maxFallTimer = 0.4f;
            currentLevel = 4;
        }
        if (collectiveRowsCleared >= 40 && collectiveRowsCleared <= 49)
        {
            maxFallTimer = 0.2f;
            currentLevel = 5;
        }
        if (collectiveRowsCleared >= 50)
        {
            maxFallTimer = 0.1f;
            currentLevel = 6;
        }
    }

    private void Awake()
    {
        FixedPieces = new int[gridSize.y, gridSize.x];
        randomBag = new List<int>();
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
        RandomizeNewBag();
        ActivePiece = allPieces[randomBag[0]];
        randomBag.RemoveAt(0);
    }

    private void Update()
    {
        if (CurrentGameState == GameState.InPlay)
        {

            fallTimer = fallTimer + Time.deltaTime;

            if (Input.GetKeyDown(moveRightKey))
            {
                if (!CheckOverlap(ActivePiece, ActivePieceRotation, ActivePiecePosition + Vector2Int.right))
                {
                    ActivePiecePosition += Vector2Int.right;
                    GameUpdate?.Invoke();
                }
            }
            if (Input.GetKeyDown(moveLeftKey))
            {
                if (!CheckOverlap(ActivePiece, ActivePieceRotation, ActivePiecePosition + Vector2Int.left))
                {
                    ActivePiecePosition += Vector2Int.left;
                    GameUpdate?.Invoke();
                }
            }
            if (Input.GetKeyDown(softDropKey) || fallTimer >= maxFallTimer)
            {
                if (!CheckOverlap(ActivePiece, ActivePieceRotation, ActivePiecePosition + Vector2Int.down))
                {
                    ActivePiecePosition += Vector2Int.down;
                    GameUpdate?.Invoke();
                }
                else
                {
                    PlacePieceOnGrid(ActivePiece, ActivePiecePosition, ActivePieceRotation);
                    ActivePiece = allPieces[randomBag[0]];
                    randomBag.RemoveAt(0);
                    if (randomBag.Count == 0)
                    {
                        RandomizeNewBag();
                    }
                    ActivePiecePosition = new Vector2Int(4, 20);
                    ActivePieceRotation = 0;
                    if (CheckOverlap(ActivePiece, ActivePieceRotation, ActivePiecePosition))
                    {
                        ChangeGameState(GameState.GameOver);
                    }
                }
                fallTimer = 0f;
            }
            if (Input.GetKeyDown(rotateKey))
            {
                int nextPieceRotation = ActivePieceRotation + 1;
                if (nextPieceRotation >= ActivePiece.GetLength(0))
                {
                    nextPieceRotation = 0;
                }
                if (!CheckOverlap(ActivePiece, nextPieceRotation, ActivePiecePosition))
                {
                    ActivePieceRotation = nextPieceRotation;
                    GameUpdate?.Invoke();
                }
            }
            if (Input.GetKeyDown(pauseGameKey))
            {
                ChangeGameState(GameState.Paused);
            }
        }
        else if (Input.GetKeyDown(pauseGameKey) && CurrentGameState == GameState.Paused)
        {
            ChangeGameState(GameState.InPlay);

        }
        else if (Input.GetKeyDown(startGameKey) && CurrentGameState == GameState.PreGame)
        {
            ChangeGameState(GameState.InPlay);
        }
        else if (Input.GetKeyDown(startGameKey) && CurrentGameState == GameState.GameOver)
        {
            ResetGame();
            GameUpdate?.Invoke();
            RandomizeNewBag();
        }
    }

    private void ResetGame()
    {
        FixedPieces = new int[gridSize.y, gridSize.x];
        currentLevel = 1;
        ActivePiecePosition = new Vector2Int(4, 20);
        randomBag.Clear();
        collectiveRowsCleared = 0;
        ChangeGameState(GameState.PreGame);
    }

    private void ChangeGameState(GameState gameState)
    {
        if (CurrentGameState != gameState)
        {
            CurrentGameState = gameState;
            GameStateChange?.Invoke(CurrentGameState);
        }
    }

    public Vector2Int GetPieceSize(int[,,] piece)
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
                    {
                        return true;
                    }
                    if (x + 1 + piecePosition.x > GridSize.x)
                    {
                        return true;
                    }
                    if (piecePosition.y + pieceSize.y - y < 0)
                    {
                        return true;
                    }
                    if (FixedPieces[piecePosition.y - y + pieceSize.y, piecePosition.x + x] > 0)
                    {
                        return true;
                    }

                }
            }
        }
        return false;
    }

    private int GetPieceColor(int[,,] piece)
    {
        for (int i = 0 ; i < allPieces.Length ; i++)
        {
            if (piece == allPieces[i])
                return i + 1;
        }
        return 0;
    }

    private void PlacePieceOnGrid(int[,,] piece, Vector2Int piecePosition, int pieceRotation)
    {
        Vector2Int pieceSize = GetPieceSize(piece);
        int pieceColor = GetPieceColor(piece);
        for (int x = 0 ; x < pieceSize.x ; x++)
        {
            for (int y = 0 ; y < pieceSize.y ; y++)
            {
                if (piece[pieceRotation, y, x] > 0)
                {
                    FixedPieces[piecePosition.y - y + pieceSize.y, piecePosition.x + x] = pieceColor;
                }
            }
        }
        ActivePiece = null;
        CheckRows();
        GameUpdate?.Invoke();
    }
}