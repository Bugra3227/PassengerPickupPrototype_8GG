using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AuthoringManager : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private GridLevelAuthoring gridLevelAuthoring;
    [SerializeField] private WorldObjectsAuthoring worldObjectsAuthoring;
    [SerializeField] private BusesLevelAuthoring busAuthoring;
    [SerializeField] private PassageLevelAuthoring passageLevelAuthoring;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private float timeDurationToBake=60f;
// This is an editor utility script all authoring.
#if UNITY_EDITOR
    [ContextMenu("Bake Level Block In Scene")]
    public void BakeLevelBlockFromScene()
    {
        gridLevelAuthoring.BakeLevelFromScene(levelData);
    }

    [ContextMenu("Bake World Objects In Scene")]
    private void BakeWorldObjectsFromScene()
    {
        worldObjectsAuthoring.BakeWorldObjectsFromScene(levelData);
    }

    [ContextMenu("ReBuild Grid From Scene")]
    public void BakeLevelGridFromScene()
    {
        gridManager.InitializeLevelData(levelData);
        gridManager.CreateGridState();
    }

    [ContextMenu("ReBuild Bus In Scene")]
    public void BakeLevelFromInScene()
    {
        gridManager.InitializeLevelData(levelData);
        gridManager.RebuildGrid();
        busAuthoring.BakeBusesFromScene(levelData);
    }

    [ContextMenu("Bake Passengers From Scene")]
    public void BakePassengersFromScene()
    {
        passageLevelAuthoring.BakePassengersFromScene(levelData);
    }
    [ContextMenu("Bake Level Time")]
    public void BakeLevelTime()
    {
        if (levelData == null)
            return;
        
        // 1. LevelData'ya zamanı kaydet
        levelData.SetLevelDuration(timeDurationToBake);

        // 2. ScriptableObject'in değiştiğini editöre bildir
        EditorUtility.SetDirty(levelData);
        
        // 3. Değişikliği kalıcı olarak kaydet
        AssetDatabase.SaveAssets(); 
        
        Debug.Log($"Level Time Baked: {timeDurationToBake} seconds");
    }

#endif
}