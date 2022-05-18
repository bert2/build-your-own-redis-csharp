namespace codecrafters_redis;

public interface ICmd {
    IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store);
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
        return new SimpleString("OK");
    }
}

public record Get(string Key) : ICmd {
    public IRespValue Run(IDictionary<string, (string value, DateTime? expiresOn)> store)
        => store.TryGetValue(Key, out var v) && (!v.expiresOn.HasValue || v.expiresOn >= DateTime.Now)
            ? new BulkString(v.value)
            : BulkString.Nil;
}
