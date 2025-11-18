using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusSegment : MonoBehaviour
{
    [SerializeField] private BusPickUpTrigger busPickUpTrigger;
 // Initialize Bus Movement
    public void InitializeBusMovement(BusMovement busMovement)
    {
        busPickUpTrigger.InitializeBusMovement(busMovement);
    }
    // Initialize Bus Seat
    public void InitializeBusSeat(BusSeats busSeats)
    {
        busPickUpTrigger.InitializeBusSeat(busSeats);
    }
}
