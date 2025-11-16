using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridLevelAuthoring : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private Transform blocksRoot;
    [SerializeField] private GridManager gridManager;

#if UNITY_EDITOR
    [ContextMenu("Bake Level From Scene")]
    public void BakeLevelFromScene()
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
#endif
}