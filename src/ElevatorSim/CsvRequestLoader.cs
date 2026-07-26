using System.Globalization;

namespace ElevatorSim;

public static class CsvRequestLoader
{
    public static IReadOnlyList<PassengerRequest> Load(string path)
    {
        using var reader = new StreamReader(path);
        return Load(reader);
    }

    public static IReadOnlyList<PassengerRequest> Load(TextReader reader)
    {
        var requests = new List<PassengerRequest>();
        string? line;
        var lineNumber = 0;
        var headerSkipped = false;

        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            line = line.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            if (!headerSkipped)
            {
                headerSkipped = true;
                if (line.StartsWith("time", StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 4)
                throw new FormatException($"Line {lineNumber}: expected time,id,source,dest.");

            if (!int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var time))
                throw new FormatException($"Line {lineNumber}: invalid time '{parts[0]}'.");

            var id = parts[1].Trim();
            if (string.IsNullOrEmpty(id))
                throw new FormatException($"Line {lineNumber}: empty passenger id.");

            if (!int.TryParse(parts[2].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var source))
                throw new FormatException($"Line {lineNumber}: invalid source '{parts[2]}'.");

            if (!int.TryParse(parts[3].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var dest))
                throw new FormatException($"Line {lineNumber}: invalid dest '{parts[3]}'.");

            requests.Add(new PassengerRequest(time, id, source, dest));
        }

        return requests;
    }
}
