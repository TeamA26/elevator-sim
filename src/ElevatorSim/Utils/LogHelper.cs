namespace ElevatorSim;

static class LogHelper
{
    public static void PrintStats(SimulationResult result)
    {
        var s = result.Stats;
        Console.WriteLine($"=== Stats ({result.AlgorithmName}) ===");
        Console.WriteLine($"Passengers: {s.Count}");
        Console.WriteLine($"Duration ticks: {result.DurationTicks}");
        Console.WriteLine(
            $"Wait time  min/avg/max: {s.MinWait} / {s.AvgWait:F2} / {s.MaxWait}");
        Console.WriteLine(
            $"Total time min/avg/max: {s.MinTotal} / {s.AvgTotal:F2} / {s.MaxTotal}");
    }

    public static void PrintComparison(SimulationResult cost, SimulationResult rr)
    {
        Console.WriteLine("=== Algorithm comparison ===");
        Console.WriteLine(
            $"{"metric",-18} {"cost",12} {"roundrobin",12}");
        Console.WriteLine(new string('-', 44));
        PrintRow("passengers", cost.Stats.Count, rr.Stats.Count);
        PrintRow("duration", cost.DurationTicks, rr.DurationTicks);
        PrintRow("wait min", cost.Stats.MinWait, rr.Stats.MinWait);
        PrintRow("wait avg", cost.Stats.AvgWait, rr.Stats.AvgWait);
        PrintRow("wait max", cost.Stats.MaxWait, rr.Stats.MaxWait);
        PrintRow("total min", cost.Stats.MinTotal, rr.Stats.MinTotal);
        PrintRow("total avg", cost.Stats.AvgTotal, rr.Stats.AvgTotal);
        PrintRow("total max", cost.Stats.MaxTotal, rr.Stats.MaxTotal);
    }

    static void PrintRow(string name, int a, int b) =>
        Console.WriteLine($"{name,-18} {a,12} {b,12}");

    static void PrintRow(string name, double a, double b) =>
        Console.WriteLine($"{name,-18} {a,12:F2} {b,12:F2}");
}
