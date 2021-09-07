using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// With help from Boas en Darryl (3GD2)

public class VisualManager : MonoBehaviour 
{
    [SerializeField] private GameObject gridRoot;
    [SerializeField] private Sprite tileSprite;
    [SerializeField] private Color[] pieceColors;

    private LogicManager logicManager;
    private SpriteRenderer[,] spriteRenderers;

    private void Start() 
    {
        logicManager = FindObjectOfType<LogicManager>();
        logicManager.GameUpdate += HandleGameUpdate;
        logicManager.GameStateChange += HandleGameStateChange;

        spriteRenderers = new SpriteRenderer[logicManager.GridSize.y, logicManager.GridSize.x ];
        gridRoot = new GameObject("Grid");
        for (int x = 0 ; x < logicManager.GridSize.x ; x++)
        {
            for (int y = 0 ; y < logicManager.GridSize.y ; y++)
            {
                spriteRenderers[y, x] = new GameObject($"x: {x} y: {y}", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
                spriteRenderers[y, x].transform.SetParent(gridRoot.transform);
                spriteRenderers[y, x].transform.position = new Vector3(x + (-logicManager.GridSize.x + 1) * .5f, y);
                spriteRenderers[y, x].sprite = tileSprite;
            }
        }
    }

    private void HandleGameStateChange(LogicManager.GameState gameState) 
    {

    }

    private void HandleGameUpdate() 
    {
        for (int y = 0 ; y < logicManager.GridSize.y; y++)
        {
            for (int x = 0 ; x < logicManager.GridSize.x ; x++)
            {
                spriteRenderers[y, x].color = pieceColors[logicManager.FixedPieces[y, x]];
            }
        }

        //UpdateTileColors();
        //UpdateGhostPiece();
        UpdateActivePiece();
        //UpdatePreviewPiece();
        //UpdateScore();
    }

    private void UpdateActivePiece()
    {
        if (logicManager.ActivePiece != null)
        {
            DrawSinglePiece(logicManager.ActivePiece, logicManager.ActivePiecePosition, logicManager.ActivePieceRotation);
        }
    }

    private void DrawSinglePiece(int[,,] piece, Vector2Int piecePosition, int pieceRotation)
    {
        Vector2Int pieceSize = new Vector2Int(piece.GetLength(2), piece.GetLength(1));
        for (int y = 0 ; y < pieceSize.y ; y++)
        {
            for (int x = 0 ; x < pieceSize.x ; x++)
            {
                if (piece[pieceRotation, y, x] > 0)
                {
                    spriteRenderers[piecePosition.y - y + pieceSize.y, piecePosition.x + x].color = pieceColors[piece[pieceRotation, y, x]];
                }
            }
        }
    }
}
