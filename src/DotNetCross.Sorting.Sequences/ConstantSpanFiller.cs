﻿using System;

namespace DotNetCross.Sorting.Sequences
{
    public struct ConstantSpanFiller : ISpanFiller
    {
        readonly int _fill;

        public ConstantSpanFiller(int fill)
        {
            _fill = fill;
        }

        public void Fill<T>(Span<T> span, Func<int, T> toValue)
        {
            span.Fill(toValue(_fill));
        }
    }
}