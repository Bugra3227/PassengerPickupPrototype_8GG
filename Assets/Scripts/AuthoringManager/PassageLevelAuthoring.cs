using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PassageLevelAuthoring : MonoBehaviour
{
    [SerializeField] private Transform passagesRoot;
   
    // // This is an editor utility script that takes Passage (spawn) data from the scene and saves it to the LevelData ScriptableObject.
    public void BakePassengersFromScene(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("PassageLevelAuthoring: LevelData is null", this);
            return;
        }

        if (passagesRoot == null)
        {
            Debug.LogError("PassageLevelAuthoring: passagesRoot is null", this);
            return;
        }

        List<LevelData.PassengerSpawnData> spawns = new List<LevelData.PassengerSpawnData>();


        PassageController[] controllers = passagesRoot.GetComponentsInChildren<PassageController>(true);
        foreach (var controller in controllers)
        {
            if (controller == null || controller.Data == null)
                continue;

            LevelData.PassengerSpawnData spawnData = new LevelData.PassengerSpawnData
            {
                spawnPosition = controller.transform.position,
                spawnRotation = controller.transform.rotation,
                groups = new List<LevelData.PassengerGroupData>()
            };


            foreach (var c in controller.Data.coloredPassageCounts)
            {
                LevelData.PassengerGroupData g = new LevelData.PassengerGroupData
                {
                    color = c.passageColor,
                    count = c.passageCount
                };
                spawnData.groups.Add(g);
            }

            spawns.Add(spawnData);
        }

        levelData.SetPassengerSpawns(spawns);
        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();

        Debug.Log($"Passenger spawns baked: {spawns.Count}", this);
    }
}