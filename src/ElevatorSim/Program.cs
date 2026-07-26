using System.Globalization;
using System.Text;
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
                var cost = RunOne(config, new CostBasedDispatcher(), requests, logPath: null, writeLog: false);
                var rr = RunOne(config, new RoundRobinDispatcher(), requests, logPath: null, writeLog: false);
                PrintComparison(cost, rr);
                return 0;
            }

            var dispatcher = CreateDispatcher(options.Algorithm);
            var result = RunOne(config, dispatcher, requests, options.LogPath, writeLog: true);
            PrintStats(result);
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

    static SimulationResult RunOne(
        SimulationConfig config,
        IDispatcher dispatcher,
        IReadOnlyList<PassengerRequest> requests,
        string? logPath,
        bool writeLog)
    {
        var simulation = new Simulation(config, dispatcher, requests);
        var result = simulation.Run();

        if (writeLog)
        {
            var text = FormatPositionLog(result);
            if (string.IsNullOrEmpty(logPath))
            {
                Console.WriteLine("=== Elevator positions (time, elev1, elev2, ...) ===");
                Console.Write(text);
                Console.WriteLine();
            }
            else
            {
                File.WriteAllText(logPath, text);
                Console.WriteLine($"Position log written to {logPath}");
            }
        }

        return result;
    }

    static string FormatPositionLog(SimulationResult result)
    {
        var sb = new StringBuilder();
        var elevators = result.PositionLog.Count > 0 ? result.PositionLog[0].Length : 0;
        sb.Append("time");
        for (var i = 1; i <= elevators; i++)
            sb.Append(CultureInfo.InvariantCulture, $",elev{i}");
        sb.AppendLine();

        for (var t = 0; t < result.PositionLog.Count; t++)
        {
            sb.Append(t);
            foreach (var floor in result.PositionLog[t])
            {
                sb.Append(',');
                sb.Append(floor);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    static void PrintStats(SimulationResult result)
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

    static void PrintComparison(SimulationResult cost, SimulationResult rr)
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

sealed class CliOptions
{
    public string InputPath { get; init; } = "samples/requests.csv";
    public int Floors { get; init; } = 55;
    public int Elevators { get; init; } = 4;
    public int Capacity { get; init; } = 8;
    public string Algorithm { get; init; } = "cost";
    public string? LogPath { get; init; }
    public bool ShowHelp { get; init; }

    public static CliOptions Parse(string[] args)
    {
        var input = "samples/requests.csv";
        var floors = 55;
        var elevators = 4;
        var capacity = 8;
        var algorithm = "cost";
        string? logPath = null;
        var help = false;

        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            switch (arg)
            {
                case "-h" or "--help":
                    help = true;
                    break;
                case "--input":
                    input = RequireValue(args, ref i, arg);
                    break;
                case "--floors":
                    floors = ParseInt(RequireValue(args, ref i, arg), arg);
                    break;
                case "--elevators":
                    elevators = ParseInt(RequireValue(args, ref i, arg), arg);
                    break;
                case "--capacity":
                    capacity = ParseInt(RequireValue(args, ref i, arg), arg);
                    break;
                case "--algorithm":
                    algorithm = RequireValue(args, ref i, arg);
                    break;
                case "--log":
                    logPath = RequireValue(args, ref i, arg);
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{arg}'. Use --help.");
            }
        }

        if (floors < 2)
            throw new ArgumentException("--floors must be >= 2.");
        if (elevators < 1)
            throw new ArgumentException("--elevators must be >= 1.");
        if (capacity < 1)
            throw new ArgumentException("--capacity must be >= 1.");

        return new CliOptions
        {
            InputPath = input,
            Floors = floors,
            Elevators = elevators,
            Capacity = capacity,
            Algorithm = algorithm,
            LogPath = logPath,
            ShowHelp = help
        };
    }

    public static void PrintHelp()
    {
        Console.WriteLine("""
            Elevator Destination Dispatch Simulator

            Usage:
              dotnet run --project src/ElevatorSim -- [options]

            Options:
              --input <path>         Request CSV (default: samples/requests.csv)
              --floors <n>           Number of floors (default: 55)
              --elevators <n>        Number of elevators (default: 4)
              --capacity <n>         Max passengers per car (default: 8)
              --algorithm <name>     cost | roundrobin | compare (default: cost)
              --log <path>           Write position log to file instead of stdout
              -h, --help             Show help

            CSV columns: time,id,source,dest
            """);
    }

    static string RequireValue(string[] args, ref int i, string flag)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Missing value for {flag}.");
        return args[++i];
    }

    static int ParseInt(string value, string flag)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n))
            throw new ArgumentException($"Invalid integer for {flag}: '{value}'.");
        return n;
    }
}
