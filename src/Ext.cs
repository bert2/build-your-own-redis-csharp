namespace codecrafters_redis;

public static class Ext {
    public static T? As<T>(this object? o) where T : class => o as T;
}
