namespace ElevatorSim;

public sealed record PassengerRequest(int Time, string Id, int Source, int Dest);
