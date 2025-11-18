using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// GameManager.cs'de (Yeni bir script)
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private float uiDelayDuration;
    private float _levelTimeRemaining;
    private bool _isStartGame;
    private int _lvlCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        levelManager.OnLevelGoalAchieved += HandleLevelWin;
    }

    private void Update()
    {
        if (!_isStartGame)
            return;

        InitializeCurrentTimeText();
        if (_levelTimeRemaining >.1f)
            _levelTimeRemaining -= Time.deltaTime;
        else
            HandleLevelTimeOut();
       
            
        
    }

    private void InitializeCurrentTimeText()
    {
        int minutes = Mathf.FloorToInt(_levelTimeRemaining / 60);
        int seconds = Mathf.FloorToInt(_levelTimeRemaining % 60);
        string timeString = string.Format("{0:D2}:{1:D2}", minutes, seconds);

        UIManager.Instance.InitializeTimeText(timeString);
    }

    public void InitializeGameProperties(float duration, int level)
    {
        _levelTimeRemaining = duration;
        _lvlCount = level;
        _isStartGame = true;
    }

    public void InitializeUILevelText()
    {
        UIManager.Instance.InitializeLevelText(_lvlCount);
    }

    private void HandleLevelWin()
    {
        _isStartGame = false;
        StartCoroutine(ShowWinPanelAfterDelay());
        levelManager.OnLevelGoalAchieved -= HandleLevelWin;
    }

    private void HandleLevelTimeOut()
    {
        _isStartGame = false;
        StartCoroutine(ShowFailedPanelAfterDelay());
    }

    //Show Win Panel with Delay
    private IEnumerator ShowWinPanelAfterDelay()
    {
        yield return new WaitForSeconds(uiDelayDuration);
        UIManager.Instance.ShowLevelCompletePanel();
    }

    //Show Failed Panel with Delay
    private IEnumerator ShowFailedPanelAfterDelay()
    {
        yield return new WaitForSeconds(uiDelayDuration);
        UIManager.Instance.ShowLevelFailedPanel();
    }
}