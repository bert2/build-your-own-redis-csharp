namespace codecrafters_redis;

public interface ICmd {
    IRespValue Run();
}

public static class ArrayExt {
    public static ICmd ToCmd(this Array cmd) => cmd.Name() switch {
        "PING" => new Ping(),
        "ECHO" => new Echo(cmd.Arg(0)),
        var s => throw new NotImplementedException($"command not implemented: {s}")
    };

    public static string Name(this Array cmd) => cmd
        .Items?
        .FirstOrDefault()
        .As<BulkString>()?
        .Value?
        .ToUpper()
        ?? throw new InvalidOperationException($"invalid command: {cmd.RenderForDisplay()}");

    public static string Arg(this Array cmd, int i) => cmd
        .Items?
        .ElementAtOrDefault(i + 1)
        .As<BulkString>()?
        .Value
        ?? throw new InvalidOperationException($"invalid command: {cmd.RenderForDisplay()}");
}

public record Ping : ICmd {
    public IRespValue Run() => new SimpleString("PONG");
}

public record Echo(string Message) : ICmd {
    public IRespValue Run() => new BulkString(Message);
}
