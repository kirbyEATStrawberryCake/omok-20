using System;
using UnityEngine;
using UnityEngine.Events;

public class GameLogicController : MonoBehaviour
{
    private GameLogic gameLogic;

    void Awake()
    {
        gameLogic = new GameLogic();
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

    public void Initialize(GamePlayManager gamePlayManager)
    {
        gameLogic.Initialize(gamePlayManager);
    }

    public void RandomizePlayerStones()
    {
        gameLogic.RandomizePlayerStones();
    }

    public void ResetGame()
    {
        gameLogic.ResetGame();
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