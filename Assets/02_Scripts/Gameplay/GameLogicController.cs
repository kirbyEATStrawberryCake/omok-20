using System;
using UnityEngine;
using UnityEngine.Events;

public class GameLogicController : MonoBehaviour
{
    private GameLogic gameLogic;

    void Awake()
    {
        gameLogic = new GameLogic();

        var gamePlayManager = GamePlayManager.Instance;
        if (gamePlayManager != null)
        {
            gameLogic.Initialize(gamePlayManager);

            gamePlayManager.OnGameStart += gameLogic.RandomizePlayerStones;
            gamePlayManager.OnGameRestart += gameLogic.ResetGame;
        }
    }

    private void OnDisable()
    {
        gameLogic.Cleanup();
    }

    public event UnityAction<StoneType> OnPlayerTurnChanged
    {
        add => gameLogic.OnPlayerTurnChanged += value;
        remove => gameLogic.OnPlayerTurnChanged -= value;
    }

    public event UnityAction<GameResult> WinConditionChecked
    {
        add => gameLogic.WinConditionChecked += value;
        remove => gameLogic.WinConditionChecked -= value;
    }

    public PlayerType GetCurrentTurnPlayer()
    {
        return gameLogic.currentTurnPlayer;
    }

    public StoneType GetCurrentStone()
    {
        return gameLogic.currentStone;
    }
}