using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private List<LevelData> allLevelData = new List<LevelData>();
    
    [SerializeField] private GridManager gridManager;

    [SerializeField] private BusManager busManager;

    [SerializeField] private PassageManager passageManager;

    private int _lvlCount;
    private LevelData _currentLevelData;


    private void OnEnable()
    {
        GetLoadData();
        InitializeLevelData();
        InitializeGridManagerState();
        InitializeBusConfig();
        InitializePassageConfig();
    }


    // Determines the current level index and loads the corresponding LevelData.
    private void InitializeLevelData()
    {
        _lvlCount = MainSaveLoad.instance.levelCount;
        _currentLevelData = allLevelData[_lvlCount];
    }

    // Loads the general game progress and settings from persistent storage.
    private void GetLoadData()
    {
        GameLoadData.Instance.LoadData();
    }

    // Sets up the grid based on the current level data.
    private void InitializeGridManagerState()
    {
        gridManager.InitializeLevelData(_currentLevelData);
        gridManager.CreateGridState();
    }

    // Passes the level configuration to the BusManager.
    private void InitializeBusConfig()
    {
        busManager.Initialize(_currentLevelData);
    }

    // Passes the level configuration to the PassageManager (for passengers).
    private void InitializePassageConfig()
    {
        passageManager.Initialize(_currentLevelData);
    }
}