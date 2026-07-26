namespace ElevatorSim;

public sealed class Passenger
{
    public Passenger(string id, int source, int dest, int requestTime)
    {
        Id = id;
        Source = source;
        Dest = dest;
        RequestTime = requestTime;
    }

    public string Id { get; }
    public int Source { get; }
    public int Dest { get; }
    public int RequestTime { get; }
    public int? PickupTime { get; set; }
    public int? DropoffTime { get; set; }

    public int WaitTime =>
        PickupTime is int pickup
            ? pickup - RequestTime
            : throw new InvalidOperationException($"Passenger {Id} has not been picked up.");

    public int TotalTime =>
        DropoffTime is int dropoff
            ? dropoff - RequestTime
            : throw new InvalidOperationException($"Passenger {Id} has not been dropped off.");

    public int TravelDirection => Dest >= Source ? 1 : -1;
}
