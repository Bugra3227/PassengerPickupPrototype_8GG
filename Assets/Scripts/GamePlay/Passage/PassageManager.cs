using System.Collections.Generic;
using UnityEngine;

public class PassageManager : MonoBehaviour
{
    [SerializeField] private PassageController passageControllerPrefab;
    [SerializeField] private Transform passagesRoot;

    private readonly List<PassageController> _spawnedControllers = new List<PassageController>();

    private LevelData _currentLevel;
// Initializes the manager, saves the level data, clears existing passages, and spawns new ones.
    public void Initialize(LevelData levelData)
    {
        _currentLevel = levelData;
        ClearSpawned();
        SpawnFromLevel();
    }
// Destroys all currently spawned PassageController objects and clears the  list.
    private void ClearSpawned()
    {
        for (int i = _spawnedControllers.Count - 1; i >= 0; i--)
        {
            if (_spawnedControllers[i] != null)
                Destroy(_spawnedControllers[i].gameObject);
        }
        _spawnedControllers.Clear();
    }
// Reads the PassengerSpawns data from the current level and instantiates PassageController prefabs for each entry.
    private void SpawnFromLevel()
    {
        if (_currentLevel == null || passageControllerPrefab == null)
            return;

        var spawns = _currentLevel.PassengerSpawns;
        for (int i = 0; i < spawns.Count; i++)
        {
            LevelData.PassengerSpawnData data = spawns[i];

            // spawnRoot pozisyonu
            Vector3 pos = data.spawnPosition;
            Quaternion rot = Quaternion.identity;

            PassageController controller =
                Instantiate(passageControllerPrefab, pos, rot, passagesRoot);

            controller.InitializeFromData(data);
            _spawnedControllers.Add(controller);
        }
    }
}