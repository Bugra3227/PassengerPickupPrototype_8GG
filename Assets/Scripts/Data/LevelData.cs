using System;
using System.Collections.Generic;
using UnityEngine;

// ScriptableObject containing all configuration data required to load and run a single level in the game,
// including grid size, obstacle blocks, world objects (like borders and spawns), and initial bus positions.

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
        public Vector2Int headBusCell;
        public BusAndPassageColorManager.BusPassageColors busColor;
        public List<Vector2Int> cells = new List<Vector2Int>();
        public float rotationY; 
    }
    [Serializable]
    public struct PassengerGroupData
    {
        public BusAndPassageColorManager.BusPassageColors color;
        public int count;
    }

    [Serializable]
    public class PassengerSpawnData
    {
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public List<PassengerGroupData> groups = new List<PassengerGroupData>();
    }


    [SerializeField] private int width = 5;
    [SerializeField] private int height = 7;
    [SerializeField] private float levelDuration;
    [SerializeField] private List<BlockData> blocks = new List<BlockData>();
    [SerializeField] private List<WorldObjectData> worldObjects = new List<WorldObjectData>();
    [SerializeField] private List<PassengerSpawnData> passengerSpawns = new List<PassengerSpawnData>();
    [SerializeField] private List<BusData> buses = new List<BusData>();

    public int Width => width;
    public int Height => height;
    
    public float LevelDuration => levelDuration;
    public IReadOnlyList<BlockData> Blocks => blocks;
    public IReadOnlyList<WorldObjectData> WorldObjects => worldObjects;
    public IReadOnlyList<PassengerSpawnData> PassengerSpawns => passengerSpawns;
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
    public void SetPassengerSpawns(List<PassengerSpawnData> newSpawns)
    {
        passengerSpawns = newSpawns;
    }
    public void SetLevelDuration(float duration) 
    {
        levelDuration = duration;
    }
#endif
}