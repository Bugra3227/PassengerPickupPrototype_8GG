using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusPickUpTrigger : MonoBehaviour
{
   public BusMovement BusMovement => busMovement;
   public BusSeats BusSeat => busSeats;
   [SerializeField] private BusMovement busMovement;
   [SerializeField] private BusSeats busSeats;
   // Initialize Bus Movement
   public void InitializeBusMovement(BusMovement movementBus)
   {
      busMovement = movementBus;
   }
   // Initialize Bus Seat
   
   public void InitializeBusSeat(BusSeats seatBus)
   {
      busSeats = seatBus;
   }
}
