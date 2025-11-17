using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BusesLevelAuthoring : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private Transform busesRoot;

#if UNITY_EDITOR
    [ContextMenu("Bake Buses From Scene")]
    private void BakeBusesFromScene()
    {
        if (levelData == null || busesRoot == null)
        {
            Debug.LogError("BusesLevelAuthoring: refs missing", this);
            return;
        }

        List<LevelData.BusData> list = new List<LevelData.BusData>();

        foreach (Transform child in busesRoot)
        {
            BusAuthoring a = child.GetComponent<BusAuthoring>();
            if (a == null)
                continue;

            LevelData.BusData data = new LevelData.BusData
            {
                color = a.Color,
                cells = a.GetCells()
            };


            list.Add(data);
        }

        levelData.SetBuses(list);
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();

        Debug.Log($"Buses baked: {list.Count}");
    }
#endif
}