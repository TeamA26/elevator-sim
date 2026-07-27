using System.Globalization;

namespace ElevatorSim;

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
