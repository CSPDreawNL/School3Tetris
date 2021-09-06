using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualManager : MonoBehaviour {

    private LogicManager logicManager;

    private void Start() {
        logicManager = FindObjectOfType<LogicManager>();
        logicManager.GameUpdate += HandleGameUpdate;
        logicManager.GameStateChange += HandleGameStateChange;
    }

    private void HandleGameStateChange(LogicManager.GameState gameState) {
    }

    private void HandleGameUpdate() {

    }
}
