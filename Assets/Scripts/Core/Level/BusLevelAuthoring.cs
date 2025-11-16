using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BusesLevelAuthoring : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Transform busesRoot;

#if UNITY_EDITOR
    [ContextMenu("Bake Buses From Scene")]
    private void BakeBusesFromScene()
    {
        if (levelData == null || gridManager == null || busesRoot == null)
        {
            Debug.LogError("BusesLevelAuthoring: refs missing", this);
            return;
        }

        List<LevelData.BusData> list = new List<LevelData.BusData>();

        foreach (Transform child in busesRoot)
        {
            BusAuthoring bus = child.GetComponent<BusAuthoring>();
            if (bus == null)
                continue;

            Vector2Int cell = gridManager.WorldToGrid(child.position);

            LevelData.BusData data = new LevelData.BusData
            {
                cell = cell,
                length = bus.InitialLength,
                color = bus.Color,
                rotationY = child.eulerAngles.y
            };

            list.Add(data);
        }

        levelData.SetBuses(list);
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
    }
#endif
}