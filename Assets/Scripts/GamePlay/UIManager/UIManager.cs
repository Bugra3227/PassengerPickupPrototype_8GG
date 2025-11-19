using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [SerializeField] private Transform levelCompletePanel;
    [SerializeField] private Transform levelFailedPanel;
    [SerializeField] private TextMeshProUGUI levelTimeTextMeshProUGUI;
    [SerializeField] private TextMeshProUGUI levelTextMeshProUGUI;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowLevelCompletePanel()
    {
        levelCompletePanel.gameObject.SetActive(true);
    }

    public void HideLevelCompletePanel()
    {
        levelCompletePanel.gameObject.SetActive(false);
    }
    public void ShowLevelFailedPanel()
    {
        levelFailedPanel.gameObject.SetActive(true);
    }
    public void HideLevelFailedPanel()
    {
        levelFailedPanel.gameObject.SetActive(false);
    }

    public void InitializeTimeText(string time)
    {
        levelTimeTextMeshProUGUI.text = time;
    }
    public void InitializeLevelText(int lvl)
    {
        int levelCount = lvl + 1;
        levelTextMeshProUGUI.text = "level "+levelCount;
    }
    
}
