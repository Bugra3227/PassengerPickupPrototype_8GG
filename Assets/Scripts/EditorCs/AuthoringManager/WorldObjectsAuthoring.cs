using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WorldObjectsAuthoring : MonoBehaviour
{
    [SerializeField] private Transform worldObjectsRoot;

// This is an editor utility script that takes world objects (spawn) data from the scene and saves it to the LevelData ScriptableObject.
    public void BakeWorldObjectsFromScene(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is null!", this);
            return;
        }

        if (worldObjectsRoot == null)
        {
            Debug.LogError("WorldObjectsRoot is null!", this);
            return;
        }

        List<LevelData.WorldObjectData> list = new List<LevelData.WorldObjectData>();

        foreach (Transform child in worldObjectsRoot)
        {
            LevelWorldObjectAuthoring a = child.GetComponent<LevelWorldObjectAuthoring>();
            if (a == null)
                continue;

            LevelData.WorldObjectData data = new LevelData.WorldObjectData
            {
                type = a.Type,
                objectId = a.ObjectId,
                position = child.position,
                rotationY = child.eulerAngles.y
            };

            list.Add(data);
        }

        levelData.SetWorldObjects(list);

        EditorUtility.SetDirty(levelData);
        AssetDatabase.SaveAssets();

        Debug.Log($"World Objects Baked: {list.Count} obj");
    }
}