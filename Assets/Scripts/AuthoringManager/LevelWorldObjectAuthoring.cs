using UnityEngine;

public class LevelWorldObjectAuthoring : MonoBehaviour
{
    [SerializeField] private LevelData.LevelWorldObjectType type;
    [SerializeField] private int objectId;

    public LevelData.LevelWorldObjectType Type => type;
    public int ObjectId => objectId;
}