namespace codecrafters_redis;

using System.Text;

public interface IRespValue {
    StringBuilder Render();
}

public record SimpleString(string Value) : IRespValue {
    public StringBuilder Render() => new StringBuilder('+').Append(Value).Append("\r\n");
}

public record BulkString(string? Value) : IRespValue {
    public StringBuilder Render() => Value is null
        ? new StringBuilder("$-1\r\n")
        : new StringBuilder('$').Append(Value.Length).Append("\r\n").Append(Value).Append("\r\n");
}

public record Array(IRespValue[]? Items) : IRespValue {
    public StringBuilder Render() => Items is null
        ? new StringBuilder("*-1\r\n")
        : new StringBuilder('*').Append(Items.Length).Append("\r\n").Append(Render(Items));
    private static StringBuilder Render(IRespValue[] items) => items
        .Aggregate(new StringBuilder(), (sb, i) => sb.Append(i.Render()).Append("\r\n"));
}
