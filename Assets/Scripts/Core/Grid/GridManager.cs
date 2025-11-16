using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Serializable]
    private struct BlockPrefabEntry
    {
        public int id;
        public GameObject prefab;
    }

    [Serializable]
    private struct WorldPrefabEntry
    {
        public LevelData.LevelWorldObjectType type;
        public int id;
        public GameObject prefab;
    }

    [SerializeField] private LevelData levelData;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform cellsRoot;
    [SerializeField] private Transform blocksRoot;
    [SerializeField] private Transform worldObjectsRoot;
    [SerializeField] private List<BlockPrefabEntry> blockPrefabs = new List<BlockPrefabEntry>();
    [SerializeField] private List<WorldPrefabEntry> worldPrefabs = new List<WorldPrefabEntry>();

    public int Width => levelData != null ? levelData.Width : 0;
    public int Height => levelData != null ? levelData.Height : 0;
    public float CellSize => cellSize;
    public LevelData LevelData => levelData;

    private float _offsetX;
    private float _offsetZ;

    private Dictionary<int, GameObject> _blockPrefabLookup;
    private Dictionary<(LevelData.LevelWorldObjectType, int), GameObject> _worldPrefabLookup;

    private void OnEnable()
    {
        Instance = this;
        BuildPrefabLookups();

        if (Application.isPlaying)
        {
            RebuildGrid();
            SpawnBlocks();
            SpawnWorldObjects();
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildPrefabLookups()
    {
        if (_blockPrefabLookup == null)
            _blockPrefabLookup = new Dictionary<int, GameObject>();
        else
            _blockPrefabLookup.Clear();

        for (int i = 0; i < blockPrefabs.Count; i++)
        {
            BlockPrefabEntry entry = blockPrefabs[i];
            if (entry.prefab == null)
                continue;

            if (!_blockPrefabLookup.ContainsKey(entry.id))
                _blockPrefabLookup.Add(entry.id, entry.prefab);
        }

        if (_worldPrefabLookup == null)
            _worldPrefabLookup = new Dictionary<(LevelData.LevelWorldObjectType, int), GameObject>();
        else
            _worldPrefabLookup.Clear();

        for (int i = 0; i < worldPrefabs.Count; i++)
        {
            WorldPrefabEntry entry = worldPrefabs[i];
            if (entry.prefab == null)
                continue;

            var key = (entry.type, entry.id);
            if (!_worldPrefabLookup.ContainsKey(key))
                _worldPrefabLookup.Add(key, entry.prefab);
        }
    }

    public void RebuildGrid()
    {
        if (levelData == null || cellPrefab == null || cellsRoot == null)
            return;

        ClearChildren(cellsRoot);

        _offsetX = -(levelData.Width - 1) * 0.5f * cellSize;
        _offsetZ = -(levelData.Height - 1) * 0.5f * cellSize;

        for (int y = 0; y < levelData.Height; y++)
        {
            for (int x = 0; x < levelData.Width; x++)
            {
                Vector3 pos = GridToWorld(x, y);

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(cellPrefab, cellsRoot);
                    instance.transform.position = pos;
                }
                else
                {
                    Instantiate(cellPrefab, pos, Quaternion.identity, cellsRoot);
                }
#else
                Instantiate(cellPrefab, pos, Quaternion.identity, cellsRoot);
#endif
            }
        }
    }

    public void SpawnBlocks()
    {
        if (levelData == null)
            return;

        ClearChildren(blocksRoot);

        IReadOnlyList<LevelData.BlockData> blocks = levelData.Blocks;
        for (int i = 0; i < blocks.Count; i++)
        {
            LevelData.BlockData b = blocks[i];
            if (!_blockPrefabLookup.TryGetValue(b.blockId, out GameObject prefab) || prefab == null)
                continue;

            Vector3 pos = GridToWorld(b.cell.x, b.cell.y);
            Quaternion rot = Quaternion.Euler(0f, b.rotationY, 0f) * prefab.transform.rotation;
            Instantiate(prefab, pos, rot, blocksRoot);
        }
    }

    public void SpawnWorldObjects()
    {
        if (levelData == null)
            return;

        ClearChildren(worldObjectsRoot);

        IReadOnlyList<LevelData.WorldObjectData> objs = levelData.WorldObjects;
        for (int i = 0; i < objs.Count; i++)
        {
            LevelData.WorldObjectData o = objs[i];
            var key = (o.type, o.objectId);

            if (!_worldPrefabLookup.TryGetValue(key, out GameObject prefab) || prefab == null)
                continue;

            Vector3 pos = o.position;
            Quaternion rot = Quaternion.Euler(0f, o.rotationY, 0f) * prefab.transform.rotation;
            Instantiate(prefab, pos, rot, worldObjectsRoot);
        }
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(child.gameObject);
            else
                Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    public Vector3 GridToWorld(int x, int y)
    {
        Vector3 local = new Vector3(_offsetX + x * cellSize, 0f, _offsetZ + y * cellSize);
        return transform.position + local;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 local = worldPos - transform.position;
        float gx = (local.x - _offsetX) / cellSize;
        float gy = (local.z - _offsetZ) / cellSize;

        int ix = Mathf.RoundToInt(gx);
        int iy = Mathf.RoundToInt(gy);

        ix = Mathf.Clamp(ix, 0, levelData.Width - 1);
        iy = Mathf.Clamp(iy, 0, levelData.Height - 1);

        return new Vector2Int(ix, iy);
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Grid From LevelData")]
    private void RebuildGridContext()
    {
        BuildPrefabLookups();
        RebuildGrid();
    }
#endif
}
