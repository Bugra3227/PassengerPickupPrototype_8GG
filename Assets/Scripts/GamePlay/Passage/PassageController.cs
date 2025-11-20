using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PassageController : MonoBehaviour
{
    [Header("Passage Data Settings")] [SerializeField]
    private PassageData passageData;

    [SerializeField] private Passenger passengerPrefab;

    [SerializeField] private MeshRenderer passageColorRenderer;

    [Header("Spawn Settings")] [SerializeField]
    private Transform spawnRoot;

    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 0, -1f);

    private BusAndPassageColorManager _busAndPassageColorManager;
    private readonly List<Passenger> _spawnedPassengers = new List<Passenger>();
    public PassageData Data => passageData;

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


    public void InitializeFromData(LevelData.PassengerSpawnData data)
    {
        _busAndPassageColorManager = BusAndPassageColorManager.Instance;


        transform.position = data.spawnPosition;
        transform.rotation = data.spawnRotation;


        passageData = new PassageData
        {
            coloredPassageCounts = new List<ColoredPassageCount>()
        };

        for (int i = 0; i < data.groups.Count; i++)
        {
            var p = data.groups[i];
            passageData.coloredPassageCounts.Add(new ColoredPassageCount
            {
                passageColor = p.color,
                passageCount = p.count
            });
        }


        SpawnPassengers();
    }

    private void SpawnPassengers()
    {
        Vector3 currentSpawnPosition = spawnRoot.position;
        _spawnedPassengers.Clear();

        foreach (var group in passageData.coloredPassageCounts)
        {
            Color targetColor = _busAndPassageColorManager.GetColor(group.passageColor);

            Quaternion rotation = transform.rotation;
            for (int i = 0; i < group.passageCount; i++)
            {
                Passenger newPassenger = Instantiate(
                    passengerPrefab,
                    currentSpawnPosition,
                    rotation,
                    spawnRoot
                );

                newPassenger.InitializePassengerColor(group.passageColor, targetColor);
                _spawnedPassengers.Add(newPassenger);
               
                currentSpawnPosition += -spawnRoot.forward * spawnOffset.magnitude;
            }
        }

        passageColorRenderer.materials[1].color = _spawnedPassengers[0].PassengerColor;
    }

    public Passenger GetNextPassengerForColor(BusAndPassageColorManager.BusPassageColors colorType)
    {
        if (_spawnedPassengers.Count == 0)
            return null;


        Passenger frontPassenger = _spawnedPassengers[0];


        if (frontPassenger.PassageColorType == colorType)
        {
            _spawnedPassengers.RemoveAt(0);


            RebuildQueuePositions();

            return frontPassenger;
        }


        return null;
    }

    private void RebuildQueuePositions()
    {
        Vector3 currentTargetPos = spawnRoot.position;


        for (int i = 0; i < _spawnedPassengers.Count; i++)
        {
            Passenger passenger = _spawnedPassengers[i];
            passenger.transform.DOKill();
            passenger.PlayWalkAnimation();
            passenger.transform.DOMove(currentTargetPos, 0.25f)
                .SetEase(Ease.InQuad).OnComplete(() => { passenger.StopWalkAnimation(); });
            currentTargetPos += -spawnRoot.forward * spawnOffset.magnitude;
        }

        if (_spawnedPassengers.Count > 0)
            passageColorRenderer.materials[1].color = _spawnedPassengers[0].PassengerColor;
        else 
            passageColorRenderer.materials[1].color = Color.grey;
    }
}