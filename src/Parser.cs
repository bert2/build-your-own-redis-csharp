#pragma warning disable IDE0065 // Misplaced using directive

using FParsec;
using FParsec.CSharp;
using Microsoft.FSharp.Core;

using static FParsec.CSharp.PrimitivesCS;
using static FParsec.CSharp.CharParsersCS;

namespace codecrafters_redis;

public static class Parser {
    public static Array<BulkString> ParseCmd(string input) => cmdArray.And(EOF).Run(input).GetResult();

    private static readonly FSharpFunc<CharStream<Unit>, Reply<BulkString>> bulkString =
        Skip('$')
        .And(Int)
        .And(SkipNewline)
        .And(len => len < 0
            ? Return<string?>(null)
            : AnyString(len).And(SkipNewline).Map(s => (string?)s))
        .Map(s => new BulkString(s));

    private static readonly FSharpFunc<CharStream<Unit>, Reply<Array<BulkString>>> cmdArray =
        Skip('*')
        .And(Int)
        .And(SkipNewline)
        .And(len => len < 1
            ? Fail<BulkString[]>("RESP command arrays must not be empty")
            : Array(len, bulkString))
        .Map(xs => new Array<BulkString>(xs));
}
