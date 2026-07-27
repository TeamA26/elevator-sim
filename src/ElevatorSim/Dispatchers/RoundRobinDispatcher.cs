namespace ElevatorSim;

/// <summary>
/// Cycles through elevators, skipping cars at reserved capacity when possible.
/// </summary>
public sealed class RoundRobinDispatcher : IDispatcher
{
    private int _nextIndex;

    public string Name => "roundrobin";

    public Elevator Assign(Passenger passenger, IReadOnlyList<Elevator> elevators, int now)
    {
        if (elevators.Count == 0)
            throw new InvalidOperationException("No elevators available.");

        // Walk the list once starting at _nextIndex (wrapping with %). Prefer the first
        // car with spare capacity, then advance _nextIndex past it so the next call
        // continues the rotation instead of always hitting the same elevator.
        for (var attempt = 0; attempt < elevators.Count; attempt++)
        {
            var index = (_nextIndex + attempt) % elevators.Count;
            var elevator = elevators[index];
            if (elevator.HasSpareCapacity)
            {
                _nextIndex = (index + 1) % elevators.Count; // circular wrap
                return elevator;
            }
        }

        // All reserved-full: still assign (immediate assignment required).
        var fallback = elevators[_nextIndex % elevators.Count];
        _nextIndex = (_nextIndex + 1) % elevators.Count;
        return fallback;
    }
}
