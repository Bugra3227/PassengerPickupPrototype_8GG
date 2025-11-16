using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Grid Level", fileName = "GridLevel_")]
public class LevelData : ScriptableObject
{
    [Serializable]
    public class BlockData
    {
        public Vector2Int cell;
        public int blockId;
        public float rotationY;
    }

    [Serializable]
    public enum LevelWorldObjectType
    {
        Border,
        PassengerSpawn,
        Passage,
        Vehicle
    }

    [Serializable]
    public class WorldObjectData
    {
        public LevelWorldObjectType type;
        public int objectId;
        public Vector3 position;
        public float rotationY;
    }
    [Serializable]
    public enum BusColor
    {
        None,
        Blue,
        Red,
        Green
    }

    [Serializable]
    public class BusData
    {
        public Vector2Int cell;
        public int length;
        public BusColor color;
        public float rotationY;
    }


    [SerializeField] private int width = 5;
    [SerializeField] private int height = 7;
    [SerializeField] private List<BlockData> blocks = new List<BlockData>();
    [SerializeField] private List<WorldObjectData> worldObjects = new List<WorldObjectData>();
    [SerializeField] private List<BusData> buses = new List<BusData>();

    public int Width => width;
    public int Height => height;
    public IReadOnlyList<BlockData> Blocks => blocks;
    public IReadOnlyList<WorldObjectData> WorldObjects => worldObjects;
    public IReadOnlyList<BusData> Buses => buses;

#if UNITY_EDITOR
    public void SetSize(int w, int h)
    {
        width = Mathf.Max(1, w);
        height = Mathf.Max(1, h);
    }

    public void SetBlocks(List<BlockData> newBlocks)
    {
        blocks = newBlocks;
    }

    public void SetWorldObjects(List<WorldObjectData> newObjects)
    {
        worldObjects = newObjects;
    }
    public void SetBuses(List<BusData> newBuses)
    {
        buses = newBuses;
    }
#endif
}