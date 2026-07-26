namespace ElevatorSim;

/// <summary>
/// Assigns each passenger to the car with the lowest estimated cost:
/// ticks until pickup + trip distance. Prefers cars with spare reserved capacity.
/// </summary>
public sealed class CostBasedDispatcher : IDispatcher
{
    public string Name => "cost";

    public Elevator Assign(Passenger passenger, IReadOnlyList<Elevator> elevators, int now)
    {
        if (elevators.Count == 0)
            throw new InvalidOperationException("No elevators available.");

        var withCapacity = elevators.Where(e => e.HasSpareCapacity).ToList();
        var candidates = withCapacity.Count > 0 ? withCapacity : elevators.ToList();

        return candidates
            .OrderBy(e => EstimateCost(e, passenger))
            .ThenBy(e => e.Id)
            .First();
    }

    public static int EstimateCost(Elevator elevator, Passenger passenger)
    {
        var toPickup = elevator.EstimateTicksToFloor(passenger.Source);
        var trip = Math.Abs(passenger.Dest - passenger.Source);
        return toPickup + trip;
    }
}
