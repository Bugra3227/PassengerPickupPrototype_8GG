using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BusAuthoring : MonoBehaviour
{
    [SerializeField] private LevelData.BusColor color = LevelData.BusColor.Red;
    [SerializeField] private int tailCount = 3;           
    [SerializeField] private GameObject segmentPrefab;    
    [SerializeField] private Transform previewRoot;       
    [SerializeField] private GridManager gridManager;    

    public LevelData.BusColor Color => color;
    public int TailCount => Mathf.Max(0, tailCount);

   
    public List<Vector2Int> GetCells()
    {
        if (gridManager == null)
            return new List<Vector2Int>();

        List<Vector2Int> cells = new List<Vector2Int>();

        Vector2Int dir = GetGridDirection();
        Vector2Int headCell = gridManager.WorldToGrid(transform.position);

       
        cells.Add(headCell);

      
        Vector2Int backDir = -dir;
        for (int i = 1; i <= TailCount; i++)
        {
            Vector2Int cell = headCell + backDir * i;
            cells.Add(cell);
        }

        return cells;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            RebuildPreview();
    }

    [ContextMenu("Rebuild Bus Preview")]
    private void RebuildPreview()
    {
        if (gridManager == null || segmentPrefab == null || previewRoot == null)
            return;

        ClearChildrenImmediate(previewRoot);

        List<Vector2Int> cells = GetCells();
        Quaternion rot = Quaternion.Euler(0f, Mathf.Round(transform.eulerAngles.y / 90f) * 90f, 0f);

       
        for (int i = 1; i < cells.Count; i++)
        {
            Vector3 pos = gridManager.GridToWorld(cells[i].x, cells[i].y);
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(segmentPrefab, previewRoot);
            inst.transform.position = pos;
            inst.transform.rotation = rot;
        }
    }

    private void ClearChildrenImmediate(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var c = root.GetChild(i);
            if (Application.isPlaying)
                Destroy(c.gameObject);
            else
                DestroyImmediate(c.gameObject);
        }
    }
#endif

    private Vector2Int GetGridDirection()
    {
       
        float y = Mathf.Repeat(transform.eulerAngles.y, 360f);
        int step = Mathf.RoundToInt(y / 90f) % 4;

        switch (step)
        {
            case 0:  return Vector2Int.up;
            case 1:  return Vector2Int.right;
            case 2:  return Vector2Int.down;
            case 3:  return Vector2Int.left;
            default: return Vector2Int.up;
        }
    }
}
