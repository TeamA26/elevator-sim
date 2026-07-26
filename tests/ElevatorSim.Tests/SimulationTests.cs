using ElevatorSim;

namespace ElevatorSim.Tests;

public class SimulationTests
{
    [Fact]
    public void SinglePassenger_WaitAndTravelMatchDistance()
    {
        var config = new SimulationConfig
        {
            FloorCount = 10,
            ElevatorCount = 1,
            Capacity = 4,
            StartingFloor = 1
        };
        var requests = new[] { new PassengerRequest(0, "p1", 1, 5) };
        var result = new Simulation(config, new CostBasedDispatcher(), requests).Run();

        var p = Assert.Single(result.Passengers);
        Assert.Equal(0, p.WaitTime); // already at floor 1
        Assert.Equal(4, p.TotalTime); // 4 floors of travel
        Assert.Equal(4, p.TotalTime - p.WaitTime);
    }

    [Fact]
    public void SinglePassenger_FromHigherFloor_WaitsForCar()
    {
        var config = new SimulationConfig
        {
            FloorCount = 20,
            ElevatorCount = 1,
            Capacity = 4,
            StartingFloor = 1
        };
        var requests = new[] { new PassengerRequest(0, "p1", 10, 15) };
        var result = new Simulation(config, new CostBasedDispatcher(), requests).Run();

        var p = Assert.Single(result.Passengers);
        Assert.Equal(9, p.WaitTime); // 1 -> 10
        Assert.Equal(14, p.TotalTime); // wait 9 + travel 5
    }

    [Fact]
    public void Capacity_IsRespectedOnBoard()
    {
        var config = new SimulationConfig
        {
            FloorCount = 10,
            ElevatorCount = 1,
            Capacity = 1,
            StartingFloor = 1
        };
        // Two passengers at floor 1 going up; capacity 1 — second waits for return or second trip.
        var requests = new[]
        {
            new PassengerRequest(0, "p1", 1, 5),
            new PassengerRequest(0, "p2", 1, 6)
        };
        var result = new Simulation(config, new CostBasedDispatcher(), requests).Run();

        Assert.Equal(2, result.Passengers.Count);
        Assert.All(result.Passengers, p => Assert.NotNull(p.DropoffTime));

        // Only one can board at t=0; the other must wait longer.
        var waits = result.Passengers.Select(p => p.WaitTime).OrderBy(w => w).ToList();
        Assert.Equal(0, waits[0]);
        Assert.True(waits[1] > 0);
    }

    [Fact]
    public void Capacity_SpillToSecondCar()
    {
        var config = new SimulationConfig
        {
            FloorCount = 10,
            ElevatorCount = 2,
            Capacity = 1,
            StartingFloor = 1
        };
        var requests = new[]
        {
            new PassengerRequest(0, "p1", 1, 5),
            new PassengerRequest(0, "p2", 1, 6)
        };
        var result = new Simulation(config, new CostBasedDispatcher(), requests).Run();

        Assert.All(result.Passengers, p => Assert.Equal(0, p.WaitTime));
    }

    [Fact]
    public void NoPeeking_LateRequestDoesNotChangeEarlyPickup()
    {
        var config = new SimulationConfig
        {
            FloorCount = 30,
            ElevatorCount = 1,
            Capacity = 4,
            StartingFloor = 1
        };

        var earlyOnly = new[] { new PassengerRequest(0, "early", 1, 20) };
        var withLate = new[]
        {
            new PassengerRequest(0, "early", 1, 20),
            new PassengerRequest(50, "late", 1, 2)
        };

        var resultEarly = new Simulation(config, new CostBasedDispatcher(), earlyOnly).Run();
        var resultBoth = new Simulation(config, new CostBasedDispatcher(), withLate).Run();

        var earlyA = resultEarly.Passengers.Single(p => p.Id == "early");
        var earlyB = resultBoth.Passengers.Single(p => p.Id == "early");
        Assert.Equal(earlyA.PickupTime, earlyB.PickupTime);
        Assert.Equal(earlyA.DropoffTime, earlyB.DropoffTime);
    }

    [Fact]
    public void Simulation_TerminatesWithAllDroppedOff()
    {
        var config = new SimulationConfig
        {
            FloorCount = 55,
            ElevatorCount = 3,
            Capacity = 4
        };
        var requests = new[]
        {
            new PassengerRequest(0, "passenger1", 1, 51),
            new PassengerRequest(0, "passenger2", 1, 37),
            new PassengerRequest(10, "passenger3", 20, 1)
        };

        var result = new Simulation(config, new CostBasedDispatcher(), requests).Run();
        Assert.Equal(3, result.Stats.Count);
        Assert.All(result.Passengers, p =>
        {
            Assert.NotNull(p.PickupTime);
            Assert.NotNull(p.DropoffTime);
            Assert.True(p.DropoffTime >= p.PickupTime);
        });
        Assert.True(result.DurationTicks > 0);
    }

    [Fact]
    public void BothAlgorithms_CompleteSameFixture()
    {
        var config = new SimulationConfig
        {
            FloorCount = 55,
            ElevatorCount = 4,
            Capacity = 8
        };
        var requests = new[]
        {
            new PassengerRequest(0, "passenger1", 1, 51),
            new PassengerRequest(0, "passenger2", 1, 37),
            new PassengerRequest(10, "passenger3", 20, 1),
            new PassengerRequest(15, "passenger4", 5, 30),
            new PassengerRequest(20, "passenger5", 40, 10)
        };

        var cost = new Simulation(config, new CostBasedDispatcher(), requests).Run();
        var rr = new Simulation(config, new RoundRobinDispatcher(), requests).Run();

        Assert.Equal(5, cost.Stats.Count);
        Assert.Equal(5, rr.Stats.Count);
        Assert.All(cost.Passengers, p => Assert.NotNull(p.DropoffTime));
        Assert.All(rr.Passengers, p => Assert.NotNull(p.DropoffTime));
    }

    [Fact]
    public void PositionLog_HasOneRowPerTick()
    {
        var config = new SimulationConfig
        {
            FloorCount = 10,
            ElevatorCount = 2,
            Capacity = 4
        };
        var requests = new[] { new PassengerRequest(0, "p1", 1, 3) };
        var result = new Simulation(config, new RoundRobinDispatcher(), requests).Run();

        Assert.Equal(result.DurationTicks, result.PositionLog.Count);
        Assert.All(result.PositionLog, row => Assert.Equal(2, row.Length));
    }
}

public class CsvRequestLoaderTests
{
    [Fact]
    public void LoadsSampleFormat()
    {
        var csv = """
            time,id,source,dest
            0,passenger1,1,51
            10,passenger3,20,1
            """;
        using var reader = new StringReader(csv);
        var requests = CsvRequestLoader.Load(reader);
        Assert.Equal(2, requests.Count);
        Assert.Equal("passenger1", requests[0].Id);
        Assert.Equal(20, requests[1].Source);
    }
}

public class DispatcherTests
{
    [Fact]
    public void CostBased_PrefersNearerCar()
    {
        var near = new Elevator(1, capacity: 4, startingFloor: 5);
        var far = new Elevator(2, capacity: 4, startingFloor: 50);
        var passenger = new Passenger("p", source: 6, dest: 10, requestTime: 0);

        var chosen = new CostBasedDispatcher().Assign(passenger, new[] { far, near }, now: 0);
        Assert.Equal(1, chosen.Id);
    }

    [Fact]
    public void RoundRobin_CyclesCars()
    {
        var cars = new[]
        {
            new Elevator(1, 4, 1),
            new Elevator(2, 4, 1),
            new Elevator(3, 4, 1)
        };
        var dispatcher = new RoundRobinDispatcher();
        var a = dispatcher.Assign(new Passenger("a", 1, 2, 0), cars, 0);
        var b = dispatcher.Assign(new Passenger("b", 1, 2, 0), cars, 0);
        var c = dispatcher.Assign(new Passenger("c", 1, 2, 0), cars, 0);
        Assert.Equal(new[] { 1, 2, 3 }, new[] { a.Id, b.Id, c.Id });
    }
}
