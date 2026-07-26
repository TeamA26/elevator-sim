namespace ElevatorSim;

public sealed class SimulationResult
{
    public required IReadOnlyList<Passenger> Passengers { get; init; }
    public required IReadOnlyList<int[]> PositionLog { get; init; }
    public required PassengerStats Stats { get; init; }
    public int DurationTicks { get; init; }
    public string AlgorithmName { get; init; } = "";
}

public sealed class PassengerStats
{
    public int Count { get; init; }
    public int MinWait { get; init; }
    public int MaxWait { get; init; }
    public double AvgWait { get; init; }
    public int MinTotal { get; init; }
    public int MaxTotal { get; init; }
    public double AvgTotal { get; init; }

    public static PassengerStats From(IReadOnlyList<Passenger> passengers)
    {
        if (passengers.Count == 0)
        {
            return new PassengerStats();
        }

        var waits = passengers.Select(p => p.WaitTime).ToList();
        var totals = passengers.Select(p => p.TotalTime).ToList();
        return new PassengerStats
        {
            Count = passengers.Count,
            MinWait = waits.Min(),
            MaxWait = waits.Max(),
            AvgWait = waits.Average(),
            MinTotal = totals.Min(),
            MaxTotal = totals.Max(),
            AvgTotal = totals.Average()
        };
    }
}
