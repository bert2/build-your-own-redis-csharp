﻿#pragma warning disable IDE0065 // Misplaced using directive

using FParsec;
using FParsec.CSharp;
using Microsoft.FSharp.Core;

using static FParsec.CSharp.PrimitivesCS;
using static FParsec.CSharp.CharParsersCS;

namespace codecrafters_redis;

public static class Parser {
    public static Array ParseArray(string input) => array.Run(input).GetResult();

    private static readonly FSharpFunc<CharStream<Unit>, Reply<Unit>> skipCrlf =
        Skip('\r')
        .AndR(Skip('\n'));

    private static readonly FSharpFunc<CharStream<Unit>, Reply<SimpleString>> simpleString =
        Skip('+')
        .And(ManyChars(NoneOf("\r\n")))
        .And(skipCrlf)
        .Map(s => new SimpleString(s));

    private static readonly FSharpFunc<CharStream<Unit>, Reply<BulkString>> bulkString =
        Skip('$')
        .And(Int)
        .And(skipCrlf)
        .And(len => len < 0 ? Return((string?)null) : BulkStringContent(len).Map(s => (string?)s))
        .Map(s => new BulkString(s));

    private static readonly FSharpFunc<CharStream<Unit>, Reply<Array>> array =
        Skip('*')
        .And(Int)
        .And(skipCrlf)
        .And(len => len < 0 ? Return((IRespValue[]?)null) : ArrayContent(len).Map(xs => (IRespValue[]?)xs))
        .Map(xs => new Array(xs));

    private static readonly FSharpFunc<CharStream<Unit>, Reply<IRespValue>> respValue =
        Choice(
            simpleString.Map(x => (IRespValue)x),
            bulkString.Map(x => (IRespValue)x),
            array.Map(x => (IRespValue)x));

    private static FSharpFunc<CharStream<Unit>, Reply<string>> BulkStringContent(int len) =>
        AnyString(len)
        .And(skipCrlf);

    private static FSharpFunc<CharStream<Unit>, Reply<IRespValue[]>> ArrayContent(int len) =>
       Array(len, respValue);
}