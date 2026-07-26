namespace ElevatorSim;

public sealed class SimulationConfig
{
    public int FloorCount { get; init; } = 55;
    public int ElevatorCount { get; init; } = 4;
    public int Capacity { get; init; } = 8;
    public int StartingFloor { get; init; } = 1;
}
