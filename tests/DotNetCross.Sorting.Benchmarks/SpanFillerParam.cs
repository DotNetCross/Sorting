using System;
using BenchmarkDotNet.Code;
using DotNetCross.Sorting.Sequences;

namespace DotNetCross.Sorting.Benchmarks
{
    public class SpanFillerParam //: IParam
    {
        private readonly ISpanFiller _filler;

        public SpanFillerParam(ISpanFiller filler) => _filler = filler;

        public ISpanFiller Value => _filler;

        public string DisplayText => $"{_filler.GetType().Name.Replace("SpanFiller", "")}";

        public override string ToString() => DisplayText;

        public string ToSourceCode() => throw new NotImplementedException();
    }
}
