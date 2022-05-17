namespace codecrafters_redis;

public interface ICmd {
    IRespValue Run(IDictionary<string, string> store);
}

public static class ArrayExt {
    public static ICmd ToCmd(this Array<BulkString> cmd) => cmd.Name() switch {
        "PING" => new Ping(),
        "ECHO" => new Echo(cmd.Arg(0)),
        "SET" => new Set(cmd.Arg(0), cmd.Arg(1)),
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
        ?? throw new InvalidOperationException($"invalid command: {cmd.RenderForDisplay()}");
}

public record Ping : ICmd {
    public IRespValue Run(IDictionary<string, string> store) => new SimpleString("PONG");
}

public record Echo(string Message) : ICmd {
    public IRespValue Run(IDictionary<string, string> store) => new BulkString(Message);
}

public record Set(string Key, string Value) : ICmd {
    public IRespValue Run(IDictionary<string, string> store) {
        store[Key] = Value;
        return new SimpleString("OK");
    }
}

public record Get(string Key) : ICmd {
    public IRespValue Run(IDictionary<string, string> store)
        => store.TryGetValue(Key, out var v) ? new BulkString(v) : BulkString.Nil;
}
