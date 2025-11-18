using System.Collections.Generic;
using UnityEngine;

public class BusManager : MonoBehaviour
{
    [SerializeField] private BusController busPrefab;
    [SerializeField] private Transform busesRoot;

    private readonly List<BusController> _spawnedBuses = new List<BusController>();

    public IReadOnlyList<BusController> SpawnedBuses => _spawnedBuses;

    private GridManager _gridManager;

    // Initializes the manager,  spawns new buses based on the level data.
    public void Initialize(LevelData levelData)
    {
        _gridManager = GridManager.Instance;
        ClearSpawnedBuses();

        if (levelData == null)
        {
            Debug.LogError("BusManager.Initialize: levelData is null", this);
            return;
        }


        var buses = levelData.Buses;
        if (buses == null || buses.Count == 0)
            return;

        for (int i = 0; i < buses.Count; i++)
        {
            SpawnBus(buses[i]);
        }
    }
    // Instantiates a bus prefab at the correct grid position
    private void SpawnBus(LevelData.BusData data)
    {
        Vector3 headWorldPos = _gridManager.GridToWorld(data.headBusCell.x, data.headBusCell.y);


        headWorldPos.y = 0.3f;

        Quaternion rot = Quaternion.Euler(0f, data.rotationY, 0f);


        BusController movement =
            Instantiate(busPrefab, headWorldPos, rot, busesRoot);

        movement.InitializeBusConfig(data);

        _spawnedBuses.Add(movement);
    }
   // Destroys all currently spawned bus objects and clears the tracking list.
    private void ClearSpawnedBuses()
    {
        for (int i = _spawnedBuses.Count - 1; i >= 0; i--)
        {
            if (_spawnedBuses[i] != null)
                Destroy(_spawnedBuses[i].gameObject);
        }

        _spawnedBuses.Clear();
    }
}