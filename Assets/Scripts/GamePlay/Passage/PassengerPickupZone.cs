using System;
using System.Collections;
using UnityEngine;

public class PassengerPickupZone : MonoBehaviour
{
    [SerializeField] private PassageController passageController;
    [SerializeField] private float jumpPower = 1.5f;
    [SerializeField] private float jumpDuration = 0.45f;
    [SerializeField] private float pickupDelay = 0.1f;

    private bool _isPicking;
    private BusPickUpTrigger _currentBusTrigger;
    private Coroutine _pickupRoutine;
    // Checks if a bus is ready for pickup and starts the passenger pickup process.
    private void Update()
    {
        if (_currentBusTrigger != null && !_isPicking)
        {
            if (_currentBusTrigger.BusMovement != null && _currentBusTrigger.BusMovement.IsDragging)
                return;
            _pickupRoutine =
                StartCoroutine(PickPassengersRoutine(_currentBusTrigger.BusSeat, _currentBusTrigger.BusMovement));
        }
    }

    //  collider enters the pickup zone.
    private void OnTriggerEnter(Collider other)
    {
        if (_isPicking)
            return;

        BusPickUpTrigger pickUpTrigger = other.GetComponentInParent<BusPickUpTrigger>();
        if (!pickUpTrigger)
            return;

        _currentBusTrigger = pickUpTrigger;
       
    }
    //  collider exit the pickup zone.
    private void OnTriggerExit(Collider other)
    {
        BusPickUpTrigger pickUpTrigger = other.GetComponentInParent<BusPickUpTrigger>();
        if (pickUpTrigger != _currentBusTrigger)
            return;

        _currentBusTrigger = null;

        if (_pickupRoutine != null)
        {
            StopCoroutine(_pickupRoutine);
            _pickupRoutine = null;
        }

        if (_currentBusTrigger != null && _currentBusTrigger.BusMovement != null)
        {
            _currentBusTrigger.BusMovement.IsBlockMove = false;
        }

        _isPicking = false;
    }
    // Finds an empty seat and makes the passenger jump into the seat.
    private IEnumerator PickPassengersRoutine(BusSeats busSeats, BusMovement busMovement)
    {
        _isPicking = true;
        while (true)
        {
            if (busMovement != null && busMovement.IsDragging)
            {
                yield return null;
                continue;
            }

            BusSeatPoint seat = busSeats.FindEmptySeat();
            if (seat == null)
                break;


            
            Passenger passenger = passageController.GetNextPassengerForColor(busSeats.BusPassageColorEnum);
            if (passenger == null)
                break;

           // busMovement.IsBlockMove = true;
            passenger.PlayJumpAnimation();
            passenger.PlaySittingAnimation();
            passenger.JumpIntoSeat(seat, jumpPower, jumpDuration);

            yield return new WaitForSeconds(jumpDuration + pickupDelay);
        }

        _isPicking = false;
        busMovement.IsBlockMove = false;
    }
}