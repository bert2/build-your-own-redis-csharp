namespace codecrafters_redis;

using System.Diagnostics.CodeAnalysis;

public static class Parser {
    public static Array<BulkString> ParseCmd(string input) {
        if (input[0] != '*') throw new ParseException(expected: "*", at: 0, input);
        if (input[1] is < '1' or > '9') throw new ParseException(expected: "non-zero digit", at: 1, input);
        if (input[2] != '\r') throw new ParseException(expected: "\\r", at: 2, input);
        if (input[3] != '\n') throw new ParseException(expected: "\\n", at: 3, input);

        var n = input[1] - '0';
        var args = new BulkString[n];

        for (int i = 0, col = 4; i < n; i++) {
            (args[i], var consumed) = ParseBulkString(input[col..]);
            col += consumed;
        }

        return new Array<BulkString>(args);
    }

    private static (BulkString, int) ParseBulkString(ReadOnlySpan<char> input) {
        if (input[0] != '$') throw new ParseException(expected: "$", at: 0, input);
        if (input[1] is < '0' or > '9') throw new ParseException(expected: "digit", at: 1, input);
        if (input[2] != '\r') throw new ParseException(expected: "\\r", at: 2, input);
        if (input[3] != '\n') throw new ParseException(expected: "\\n", at: 3, input);

        var len = input[1] - '0';
        var end = 4 + len;
        var v = input[4..end];

        return (new BulkString(v.ToString()), 4 + len + 2);
    }
}

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors.")]
public class ParseException : Exception {
    public ParseException(string expected, int at, ReadOnlySpan<char> input)
        : base($"Expected {expected} at column {at} of '{EscapeCrlf(input)}'.") { }
    private static string EscapeCrlf(ReadOnlySpan<char> s) => s.ToString().Replace("\r\n", "\\r\\n");
}
