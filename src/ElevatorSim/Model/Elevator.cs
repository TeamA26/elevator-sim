namespace ElevatorSim;

public sealed class Elevator
{
    public Elevator(int id, int capacity, int startingFloor = 1)
    {
        Id = id;
        Capacity = capacity;
        Floor = startingFloor;
        Direction = Direction.Idle;
    }

    public int Id { get; }
    public int Capacity { get; }
    public int Floor { get; private set; }
    public Direction Direction { get; private set; }

    public List<Passenger> Onboard { get; } = new();
    public List<Passenger> AssignedWaiting { get; } = new();

    public int ReservedCount => Onboard.Count + AssignedWaiting.Count;
    public bool HasSpareCapacity => ReservedCount < Capacity;
    public bool HasWork => Onboard.Count > 0 || AssignedWaiting.Count > 0;

    public IEnumerable<int> StopFloors()
    {
        foreach (var p in Onboard)
            yield return p.Dest;
        foreach (var p in AssignedWaiting)
            yield return p.Source;
    }

    public void Assign(Passenger passenger)
    {
        AssignedWaiting.Add(passenger);
    }

    public void ServeFloor(int time)
    {
        // Alight first to free capacity, then board.
        for (var i = Onboard.Count - 1; i >= 0; i--)
        {
            var passenger = Onboard[i];
            if (passenger.Dest != Floor)
                continue;

            passenger.DropoffTime = time;
            Onboard.RemoveAt(i);
        }

        for (var i = AssignedWaiting.Count - 1; i >= 0; i--)
        {
            if (Onboard.Count >= Capacity)
                break;

            var passenger = AssignedWaiting[i];
            if (passenger.Source != Floor)
                continue;

            passenger.PickupTime = time;
            AssignedWaiting.RemoveAt(i);
            Onboard.Add(passenger);
        }
    }

    public void UpdateDirection()
    {
        var stops = StopFloors().ToHashSet();
        if (stops.Count == 0)
        {
            Direction = Direction.Idle;
            return;
        }

        var hasAbove = stops.Any(s => s > Floor);
        var hasBelow = stops.Any(s => s < Floor);
        var atStop = stops.Contains(Floor);

        switch (Direction)
        {
            case Direction.Up:
                if (hasAbove)
                    return;
                if (hasBelow)
                    Direction = Direction.Down;
                else if (!atStop)
                    Direction = Direction.Idle;
                break;

            case Direction.Down:
                if (hasBelow)
                    return;
                if (hasAbove)
                    Direction = Direction.Up;
                else if (!atStop)
                    Direction = Direction.Idle;
                break;

            case Direction.Idle:
                if (hasAbove && !hasBelow)
                    Direction = Direction.Up;
                else if (hasBelow && !hasAbove)
                    Direction = Direction.Down;
                else if (hasAbove || hasBelow)
                {
                    // Choose nearest pending stop; tie -> up.
                    var nearest = stops
                        .Where(s => s != Floor)
                        .OrderBy(s => Math.Abs(s - Floor))
                        .ThenBy(s => s)
                        .First();
                    Direction = nearest > Floor ? Direction.Up : Direction.Down;
                }
                break;
        }
    }

    public void Move()
    {
        if (Direction == Direction.Idle)
            return;

        Floor += (int)Direction;
    }

    /// <summary>
    /// Estimated ticks until this car can arrive at <paramref name="targetFloor"/>
    /// given current commitments (SCAN path), without mutating state.
    /// </summary>
    public int EstimateTicksToFloor(int targetFloor)
    {
        if (Floor == targetFloor)
            return 0;

        var floor = Floor;
        var direction = Direction;
        var stops = StopFloors().ToHashSet();
        stops.Add(targetFloor);

        const int safety = 100_000;
        for (var ticks = 1; ticks <= safety; ticks++)
        {
            direction = NextDirection(floor, direction, stops);
            if (direction == Direction.Idle)
                return ticks; // should not happen while target remains

            floor += (int)direction;
            if (floor == targetFloor)
                return ticks;
        }

        return safety;
    }

    private static Direction NextDirection(int floor, Direction direction, HashSet<int> stops)
    {
        if (stops.Count == 0)
            return Direction.Idle;

        var hasAbove = stops.Any(s => s > floor);
        var hasBelow = stops.Any(s => s < floor);

        return direction switch
        {
            Direction.Up when hasAbove => Direction.Up,
            Direction.Up when hasBelow => Direction.Down,
            Direction.Down when hasBelow => Direction.Down,
            Direction.Down when hasAbove => Direction.Up,
            Direction.Idle when hasAbove && !hasBelow => Direction.Up,
            Direction.Idle when hasBelow && !hasAbove => Direction.Down,
            Direction.Idle when hasAbove || hasBelow =>
                stops.Where(s => s != floor)
                    .OrderBy(s => Math.Abs(s - floor))
                    .ThenBy(s => s)
                    .Select(s => s > floor ? Direction.Up : Direction.Down)
                    .FirstOrDefault(),
            _ => Direction.Idle
        };
    }
}
