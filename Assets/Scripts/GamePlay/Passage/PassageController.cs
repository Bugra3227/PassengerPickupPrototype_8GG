using System;
using System.Collections.Generic;
using UnityEngine;

public class PassageController : MonoBehaviour
{
    [SerializeField] private PassageData passageData;
    [SerializeField] private GameObject passengerPrefab;

    [Header("Spawn Settings")] [SerializeField]
    private Transform spawnRoot;

    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, -1f);

    private BusAndPassageColorManager _busAndPassageColorManager;

    [Serializable]
    public struct ColoredPassageCount
    {
        public BusAndPassageColorManager.BusPassageColors passageColor;
        public int passageCount;
    }

    [Serializable]
    public class PassageData
    {
        public List<ColoredPassageCount> coloredPassageCounts = new List<ColoredPassageCount>();
    }

    private void Start()
    {
        _busAndPassageColorManager = BusAndPassageColorManager.Instance;

        if (passengerPrefab == null || spawnRoot == null || _busAndPassageColorManager == null)
        {
            Debug.LogError("Passenger Prefab, Miss Spawn Root!");
            return;
        }

        SpawnPassengers();
    }

    private void SpawnPassengers()
    {
        Vector3 currentSpawnPosition = spawnRoot.position;


        foreach (var group in passageData.coloredPassageCounts)
        {
            Color targetColor = _busAndPassageColorManager.GetColor(group.passageColor);


            for (int i = 0; i < group.passageCount; i++)
            {
                GameObject newPassenger = Instantiate(
                    passengerPrefab,
                    currentSpawnPosition,
                    Quaternion.identity,
                    spawnRoot
                );


                ApplyColorToPassenger(newPassenger, targetColor);
                currentSpawnPosition -= spawnOffset;
            }
        }
    }

    private void ApplyColorToPassenger(GameObject passenger, Color color)
    {
        Renderer renderer = passenger.GetComponentInChildren<Renderer>();
        renderer.material = new Material(renderer.material);
        renderer.material.color = color;
    }
}