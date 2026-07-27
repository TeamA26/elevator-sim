namespace ElevatorSim;

public interface IDispatcher
{
    string Name { get; }

    /// <summary>
    /// Immediately assigns <paramref name="passenger"/> to an elevator.
    /// Must always succeed (every request gets a car).
    /// </summary>
    Elevator Assign(Passenger passenger, IReadOnlyList<Elevator> elevators, int now);
}
