using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLoadData : MonoBehaviour
{
    public static GameLoadData Instance{ get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void LoadData()
    {
        MainSaveLoad.Load();
    }
        public void IncreaseLevelCount()
    {
        MainSaveLoad.instance.levelCount++;
        MainSaveLoad.Save();
    }
}
