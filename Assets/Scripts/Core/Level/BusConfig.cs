using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct BusSegmentData
{
    public Vector2Int gridOffset;
}

public class BusConfig : MonoBehaviour
{
    public BusAndPassageColorManager.BusPassageColors BusPassageColorEnums => busPassageColorEnums;
    [Header("Color")]
    [SerializeField] private BusAndPassageColorManager.BusPassageColors busPassageColorEnums;

    [Header("Segments")]
    [SerializeField] private BusSegment segmentPrefab;
    [SerializeField] private List<BusSegmentData> initialSegments = new List<BusSegmentData>();

    private BusAndPassageColorManager busAndPassageColorManager;

    public Color BusColor => busAndPassageColorManager.GetColor(busPassageColorEnums);
    public BusSegment SegmentPrefab => segmentPrefab;
    public IReadOnlyList<BusSegmentData> InitialSegments => initialSegments;

    private void OnEnable()
    {
        busAndPassageColorManager = BusAndPassageColorManager.Instance;
    }

    public void InitializeBusBusPassageColorEnums(BusAndPassageColorManager.BusPassageColors busPassageColors)
    {
        busPassageColorEnums = busPassageColors;
    }
}