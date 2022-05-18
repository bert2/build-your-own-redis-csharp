namespace codecrafters_redis;

using System.Text;

public interface IRespValue {
    StringBuilder Render();
}

public static class IRespValueExt {
    public static StringBuilder RenderForDisplay(this IRespValue v) => v
        .Render()
        .Replace("\r\n", "\\r\\n");
}

public record SimpleString(string Value) : IRespValue {
    public StringBuilder Render() => new StringBuilder("+").Append(Value).Append("\r\n");
}

public record BulkString(string? Value) : IRespValue {
    public static readonly BulkString Nil = new((string?)null);
    public StringBuilder Render() => Value is null
        ? new StringBuilder("$-1\r\n")
        : new StringBuilder("$").Append(Value.Length).Append("\r\n").Append(Value).Append("\r\n");
}

public record Array<T>(T[]? Items) : IRespValue
    where T : IRespValue {
    public static readonly Array<T> Nil = new((T[]?)null);
    public StringBuilder Render() => Items is null
        ? new StringBuilder("*-1\r\n")
        : new StringBuilder("*").Append(Items.Length).Append("\r\n").Append(Render(Items));
    private static StringBuilder Render(T[] items) => items
        .Aggregate(new StringBuilder(), (sb, i) => sb.Append(i.Render()).Append("\r\n"));
}
