using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridLevelAuthoring : MonoBehaviour
{
    [SerializeField] private Transform blocksRoot;
    [SerializeField] private GridManager gridManager;

// This is an editor utility script that takes Grid (spawn) data from the scene and saves it to the LevelData ScriptableObject.
    public void BakeLevelFromScene(LevelData levelData)
    {
        if (levelData == null || blocksRoot == null)
            return;

        List<LevelData.BlockData> list = new List<LevelData.BlockData>();

        foreach (Transform child in blocksRoot)
        {
            GridBlockAuthoring block = child.GetComponent<GridBlockAuthoring>();
            if (block == null)
                continue;

            int id = block.BlockId;


            Vector2Int cell = gridManager.WorldToGrid(child.position);

            LevelData.BlockData data = new LevelData.BlockData
            {
                cell = cell,
                blockId = id,
                rotationY = child.eulerAngles.y
            };

            list.Add(data);
        }

        levelData.SetBlocks(list);
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Grid Objects Baked: {list.Count} obj");
    }

    [ContextMenu("Clear All Blocks In Scene")]
    private void ClearAllBlocksInScene()
    {
        if (blocksRoot == null)
            return;

        for (int i = blocksRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = blocksRoot.GetChild(i);
            DestroyImmediate(child.gameObject);
        }

        EditorUtility.SetDirty(this);
    }
}