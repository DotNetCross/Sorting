using Xunit;

namespace DotNetCross.Sorting.Tests
{
    public class FloorLog2PlusOneTest
    {
        [Fact]
        public void Test()
        {
            for (int i = 2; i < 256 * 1024; i++)
            {
                Assert.Equal(FloorLog2(i), FloorLog2PlusOne(i));
                Assert.Equal(FloorLog2PlusOne(i), Common.FloorLog2PlusOne(i));
                Assert.Equal(FloorLog2(i), Common.FloorLog2PlusOne(i));
            }
        }

        [Fact]
        public void Test_()
        {
            Assert.Equal(6, Common.FloorLog2PlusOne(35));
        }

        // coreclr version
        internal static int FloorLog2(int n)
        {
            int result = 0;
            while (n >= 1)
            {
                result++;
                n = n / 2;
            }
            return result;
        }

        internal static int FloorLog2PlusOne(int n)
        {
            //Debug.Assert(n >= 2);
            int result = 2;
            n >>= 2;
            while (n > 0)
            {
                ++result;
                n >>= 1;
            }
            return result;
        }
    }
}
