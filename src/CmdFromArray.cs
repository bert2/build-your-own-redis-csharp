namespace codecrafters_redis;

using System;
using System.Linq;

public static class CmdFromArray {
    public static ICmd ToCmd(this Array<BulkString> cmd) => cmd.Name() switch {
        "PING" => new Ping(),
        "ECHO" => new Echo(cmd.Arg(0)),
        "SET" => new Set(cmd.Arg(0), cmd.Arg(1), cmd.OptLong("px")),
        "GET" => new Get(cmd.Arg(0)),
        var s => throw new NotImplementedException($"command not implemented: {s}")
    };

    private static string Name(this Array<BulkString> cmd) => cmd
        .Items?
        .FirstOrDefault()?
        .Value?
        .ToUpper()
        ?? throw new InvalidOperationException($"invalid command: {cmd.RenderForDisplay()}");

    private static string Arg(this Array<BulkString> cmd, int i) => cmd
        .Items?
        .ElementAtOrDefault(i + 1)?
        .Value
        ?? throw new InvalidOperationException($"expected at least {i + 1} arguments: {cmd.RenderForDisplay()}");

    private static string? Opt(this Array<BulkString> cmd, string opt) {
        if (cmd.Items is null) return null;

        var optIdx = Array.FindIndex(cmd.Items, x => string.Equals(x.Value, opt, StringComparison.OrdinalIgnoreCase));
        if (optIdx == -1) return null;

        return cmd
            .Items?
            .ElementAtOrDefault(optIdx + 1)?
            .Value
            ?? throw new InvalidOperationException($"expected value for option '{opt}': {cmd.RenderForDisplay()}");
    }

    public static long? OptLong(this Array<BulkString> cmd, string opt)
        => long.TryParse(cmd.Opt(opt), out var v) ? v : null;
}
