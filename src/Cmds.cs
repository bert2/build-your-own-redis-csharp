namespace codecrafters_redis;

public interface ICmd {
    IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store);
}

public static class ArrayExt {
    public static ICmd ToCmd(this Array<BulkString> cmd) => cmd.Name() switch {
        "PING" => new Ping(),
        "ECHO" => new Echo(cmd.Arg(0)),
        "SET" => new Set(cmd.Arg(0), cmd.Arg(1), cmd.OptLong("px")),
        "GET" => new Get(cmd.Arg(0)),
        var s => throw new NotImplementedException($"command not implemented: {s}")
    };

    public static string Name(this Array<BulkString> cmd) => cmd
        .Items?
        .FirstOrDefault()?
        .Value?
        .ToUpper()
        ?? throw new InvalidOperationException($"invalid command: {cmd.RenderForDisplay()}");

    public static string Arg(this Array<BulkString> cmd, int i) => cmd
        .Items?
        .ElementAtOrDefault(i + 1)?
        .Value
        ?? throw new InvalidOperationException($"expected at least {i + 1} arguments: {cmd.RenderForDisplay()}");

    public static string? Opt(this Array<BulkString> cmd, string opt) {
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

public record Ping : ICmd {
    public IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store)
        => new SimpleString("PONG");
}

public record Echo(string Message) : ICmd {
    public IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store)
        => new BulkString(Message);
}

public record Set(string Key, string Value, long? Px) : ICmd {
    public IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store) {
        DateTime? expiresOn = Px.HasValue ? DateTime.Now.AddMilliseconds(Px.Value) : null;
        store[Key] = (Value, expiresOn);
        if (expiresOn.HasValue) Console.WriteLine($"******** expires {expiresOn.Value.Ticks} {expiresOn.Value:O}");
        return new SimpleString("OK");
    }
}

public record Get(string Key) : ICmd {
    public IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store) {
        if (store.TryGetValue(Key, out var v) && (!v.expiresOn.HasValue || v.expiresOn >= DateTime.Now)) {
            if (v.expiresOn.HasValue) Console.WriteLine($"******** now {DateTime.Now.Ticks} {DateTime.Now:O}");
            return new BulkString(v.value);
        } else {
            return BulkString.Nil;
        }
    }
}
