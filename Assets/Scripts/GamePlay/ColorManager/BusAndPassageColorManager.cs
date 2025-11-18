using UnityEngine;
using System.Collections.Generic;

public class BusAndPassageColorManager : MonoBehaviour
{
    // Defines the common colors used for both buses and passenger passages.
    public enum BusPassageColors
    {
        Red = 0,
        Yellow = 1,
        Green = 2,
        Purple = 3
    }

    public static BusAndPassageColorManager Instance;

    [Header("Color Definitions (Inspector)")] [SerializeField]
    private Color redColor;

    [SerializeField] private Color yellowColor;
    [SerializeField] private Color greenColor;
    [SerializeField] private Color purpleColor;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Returns the defined Unity Color object corresponding to the provided BusPassageColors enum value.
    public Color GetColor(BusPassageColors colorType)
    {
        switch (colorType)
        {
            case BusPassageColors.Red:
                return redColor;
            case BusPassageColors.Yellow:
                return yellowColor;
            case BusPassageColors.Green:
                return greenColor;
            case BusPassageColors.Purple:
                return purpleColor;
            default:
                return Color.black; 
        }
    }
}