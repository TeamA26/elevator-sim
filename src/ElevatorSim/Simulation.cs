namespace ElevatorSim;

public sealed class Simulation
{
    private readonly SimulationConfig _config;
    private readonly IDispatcher _dispatcher;
    private readonly List<PassengerRequest> _requests;

    public Simulation(
        SimulationConfig config,
        IDispatcher dispatcher,
        IEnumerable<PassengerRequest> requests)
    {
        _config = config;
        _dispatcher = dispatcher;
        // Sorted copy — only dequeue when time == now (no peeking for decisions).
        _requests = requests.OrderBy(r => r.Time).ThenBy(r => r.Id, StringComparer.Ordinal).ToList();
        ValidateRequests(_requests, config);
    }

    public SimulationResult Run()
    {
        var elevators = Enumerable.Range(1, _config.ElevatorCount)
            .Select(id => new Elevator(id, _config.Capacity, _config.StartingFloor))
            .ToList();

        var allPassengers = new List<Passenger>();
        var positionLog = new List<int[]>();
        var requestIndex = 0;
        var time = 0;
        const int maxTicks = 1_000_000;

        while (time <= maxTicks)
        {
            // 1–2. Admit and immediately assign requests at this time only.
            while (requestIndex < _requests.Count && _requests[requestIndex].Time == time)
            {
                var req = _requests[requestIndex++];
                var passenger = new Passenger(req.Id, req.Source, req.Dest, req.Time);
                allPassengers.Add(passenger);
                var car = _dispatcher.Assign(passenger, elevators, time);
                car.Assign(passenger);
            }

            // 3. Serve current floors (alight then board).
            foreach (var elevator in elevators)
                elevator.ServeFloor(time);

            // 4. Log positions at this time (after serving, before moving).
            positionLog.Add(elevators.Select(e => e.Floor).ToArray());

            var hasFutureRequests = requestIndex < _requests.Count;
            var hasWork = elevators.Any(e => e.HasWork);
            if (!hasFutureRequests && !hasWork)
                break;

            // 5. Plan direction (SCAN) and move one floor.
            foreach (var elevator in elevators)
            {
                elevator.UpdateDirection();
                elevator.Move();
            }

            time++;
        }

        if (time > maxTicks)
            throw new InvalidOperationException("Simulation exceeded maximum tick limit.");

        if (allPassengers.Any(p => p.DropoffTime is null))
            throw new InvalidOperationException("Simulation ended with undelivered passengers.");

        return new SimulationResult
        {
            Passengers = allPassengers,
            PositionLog = positionLog,
            Stats = PassengerStats.From(allPassengers),
            DurationTicks = positionLog.Count,
            AlgorithmName = _dispatcher.Name
        };
    }

    private static void ValidateRequests(List<PassengerRequest> requests, SimulationConfig config)
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var r in requests)
        {
            if (r.Time < 0)
                throw new ArgumentException($"Request {r.Id} has negative time.");
            if (r.Source < 1 || r.Source > config.FloorCount)
                throw new ArgumentException($"Request {r.Id} source floor {r.Source} out of range 1..{config.FloorCount}.");
            if (r.Dest < 1 || r.Dest > config.FloorCount)
                throw new ArgumentException($"Request {r.Id} dest floor {r.Dest} out of range 1..{config.FloorCount}.");
            if (r.Source == r.Dest)
                throw new ArgumentException($"Request {r.Id} source and dest are the same floor.");
            if (!ids.Add(r.Id))
                throw new ArgumentException($"Duplicate passenger id '{r.Id}'.");
        }
    }
}
