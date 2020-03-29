using System.Runtime.CompilerServices;

namespace DotNetCross.Sorting
{
    internal interface IIsNaN<T>
    {
        bool IsNaN(T value);
    }

    internal struct SingleIsNaN : IIsNaN<float>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNaN(float value) => float.IsNaN(value);
    }
    internal struct DoubleIsNaN : IIsNaN<double>
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNaN(double value) => double.IsNaN(value);
    }
}
