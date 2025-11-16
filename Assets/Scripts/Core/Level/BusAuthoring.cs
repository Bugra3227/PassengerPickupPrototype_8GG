using UnityEngine;

public class BusAuthoring : MonoBehaviour
{
    [SerializeField] private int initialLength = 4;
    [SerializeField] private LevelData.BusColor color = LevelData.BusColor.Blue;

    public int InitialLength => initialLength;
    public LevelData.BusColor Color => color;
}