using UnityEngine;

public class BusesSpawner : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject busPrefab;
    [SerializeField] private Transform root;

    private void Start()
    {
        if (levelData == null || gridManager == null || busPrefab == null)
            return;

        var buses = levelData.Buses;
        for (int i = 0; i < buses.Count; i++)
        {
            LevelData.BusData data = buses[i];
            if (data.cells == null || data.cells.Count == 0)
                continue;

            // head hücresi
            Vector2Int headCell = data.cells[0];
            Vector3 pos = gridManager.GridToWorld(headCell.x, headCell.y);

            // yönü path'ten hesapla (ilk iki hücre arası)
            Quaternion rot = Quaternion.identity;
            if (data.cells.Count >= 2)
            {
                Vector2Int dir = data.cells[0] - data.cells[1];
                float angle = 0f;

                if (dir == Vector2Int.up) angle = 0f;
                else if (dir == Vector2Int.right) angle = 90f;
                else if (dir == Vector2Int.down) angle = 180f;
                else if (dir == Vector2Int.left) angle = 270f;

                rot = Quaternion.Euler(0f, angle, 0f);
            }

            GameObject instance = Instantiate(busPrefab, pos, rot, root);

            /*BusController controller = instance.GetComponent<BusController>();
            if (controller != null)
            {
                controller.Initialize(data, gridManager);
            }*/
        }
    }
}