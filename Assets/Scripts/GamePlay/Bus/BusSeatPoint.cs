using UnityEngine;

public class BusSeatPoint : MonoBehaviour
{
    public bool IsOccupied { get; private set; }
    public Passenger Occupant { get; private set; }

    public void SetOccupied(Passenger passenger)
    {
        IsOccupied = passenger != null;
        Occupant = passenger;
    }
}