using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusController : MonoBehaviour
{
    [SerializeField] private BusMovement busMovement;
  // Initialize Bus Config
    public void InitializeBusConfig(LevelData.BusData data)
    {
        busMovement.InitializeFromData(data);
    }
}
