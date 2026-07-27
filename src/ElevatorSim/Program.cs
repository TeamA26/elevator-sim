using ElevatorSim;

static class Program
{
    static int Main(string[] args)
    {
        try
        {
            var options = CliOptions.Parse(args);
            if (options.ShowHelp)
            {
                CliOptions.PrintHelp();
                return 0;
            }

            var requests = CsvRequestLoader.Load(options.InputPath);
            var config = new SimulationConfig
            {
                FloorCount = options.Floors,
                ElevatorCount = options.Elevators,
                Capacity = options.Capacity
            };

            if (options.Algorithm.Equals("compare", StringComparison.OrdinalIgnoreCase))
            {
                var cost = Simulation.RunSimulation(config, new CostBasedDispatcher(), requests, logPath: null, writeLog: false);
                var rr = Simulation.RunSimulation(config, new RoundRobinDispatcher(), requests, logPath: null, writeLog: false);
                LogHelper.PrintComparison(cost, rr);
                return 0;
            }

            var dispatcher = CreateDispatcher(options.Algorithm);
            var result = Simulation.RunSimulation(config, dispatcher, requests, options.LogPath, writeLog: true);
            LogHelper.PrintStats(result);
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static IDispatcher CreateDispatcher(string name) =>
        name.ToLowerInvariant() switch
        {
            "cost" => new CostBasedDispatcher(),
            "roundrobin" or "rr" => new RoundRobinDispatcher(),
            _ => throw new ArgumentException(
                $"Unknown algorithm '{name}'. Use cost, roundrobin, or compare.")
        };
}
