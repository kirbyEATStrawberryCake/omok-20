using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance()
    {
        return instance;
    }

    public TMP_Text timerText;

    public bool isGameOver = false;
    public bool isMyTurn = false;

    public float timer = 30.0f;
    public float currentTime = 30.0f;

    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (!isGameOver)
        {
            Timer();
        }
    }

    public void Timer()
    {
        currentTime -= Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);

        timerText.text = string.Format("{0:00} : {1:000}", timeSpan.Seconds, timeSpan.Milliseconds);

        if (currentTime <= 0.0f)
        {
            currentTime = 20.0f;
            isMyTurn = (isMyTurn) ? false : true;
        }
    }

    public void GameStart()
    {
        isGameOver = false;
        isMyTurn = true;
        currentTime = timer;
    }

    public void GameStop()
    {
        isGameOver = true;
        currentTime = timer;
        timerText.text = "20 : 000";
    }

    public void Next()
    {
        if (isMyTurn)
        {
            isMyTurn = false;
            currentTime = timer;
        }
    }
}