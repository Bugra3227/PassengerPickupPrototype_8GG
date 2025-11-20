using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BusesLevelAuthoring : MonoBehaviour
{
    [SerializeField] private Transform busesRoot;
    [SerializeField] private GridManager grid;

// This is an editor utility script that takes Bus (spawn) data from the scene and saves it to the LevelData ScriptableObject.
#if UNITY_EDITOR
    public void BakeBusesFromScene(LevelData levelData)
    {
        if (levelData == null || busesRoot == null)
        {
            Debug.LogError("BusesLevelAuthoring: refs missing", this);
            return;
        }

        List<LevelData.BusData> list = new List<LevelData.BusData>();

        foreach (Transform child in busesRoot)
        {
            BusConfig config = child.GetComponent<BusConfig>();
            if (config == null)
                continue;

            LevelData.BusData data = new LevelData.BusData();

            Vector3 worldPos = child.position;
            Vector2Int headCell = grid.WorldToGrid(worldPos);
            data.headBusCell = headCell;
            
            Debug.Log($"Bus {child.name} World Pos: ({worldPos.x:F2}, {worldPos.z:F2}) -> Grid Cell: ({headCell.x}, {headCell.y})", child);
            
            data.busColor = config.BusPassageColorEnums;

            data.cells = new List<Vector2Int>();
            foreach (var seg in config.InitialSegments)
                data.cells.Add(seg.gridOffset);

            data.rotationY = child.eulerAngles.y;

            list.Add(data);
        }

        levelData.SetBuses(list);

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();

        Debug.Log($"Buses baked: {list.Count}");
    }
#endif
}