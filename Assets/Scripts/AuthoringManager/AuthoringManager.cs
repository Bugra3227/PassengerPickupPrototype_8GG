using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AuthoringManager : MonoBehaviour
{
    [SerializeField] private LevelData levelData;
    [SerializeField] private GridLevelAuthoring gridLevelAuthoring;
    [SerializeField] private WorldObjectsAuthoring worldObjectsAuthoring;
    [SerializeField] private BusesLevelAuthoring busAuthoring;
    [SerializeField] private PassageLevelAuthoring passageLevelAuthoring;
    [SerializeField] private GridManager gridManager;
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

#endif
}