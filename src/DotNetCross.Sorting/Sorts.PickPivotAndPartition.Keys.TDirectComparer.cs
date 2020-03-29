using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static DotNetCross.Sorting.Swapper;

namespace DotNetCross.Sorting
{
    public static partial class Sorts
    {
        internal static partial class TDirectComparer
        {
            // https://sharplab.io/#v2:EYLgxg9gTgpgtADwGwBYA0AXEBDAzgWwB8ABAJgEYBYAKGIAYACY8gOgBEBLbAcwDsJcGDmFwBuGvSasASgFdeQ/DBYBhCPgAOHADYwoAZT0A3YTDETG+gBbYoGgDLZgLOQo5Lx1CQGYmpBioMAN40DGFMvsQoDACyABQAlMGh4QC+KeGZ4RwKerzY2gw5GHoAZthgMAwAkpywYBhqmrZ6ADw5DAAqAHwZWSHUWUMMwBAQhQDisNglUJ02vHGdDAhoXQwAngmew1mj4wz2Zrjz2IvLq+tbO7vh+4VHuCcLAKIAjrIFSytry9cMAHoAV0APJsEEgBhsGC6EpFUoMADuVRaDH4GAYABMIDluAxcBoYGAuNoOAAvKr6aAYbx9TLpLyDIYAbRiMAwVggmOqmm0cTZHK5PI02hBGiEEF4uBYAEFuNxYE8OEYYNVeKTeLiEgBdOnZXJQfKFZhIJjRfSI7AaVo9OKwBHLbBre3rYAJPVhAa3cLLEqaBgAXgY2Bu3uwgZGoduwAjfo0UfpNA9RQNRvxGCgsgaNQU3lIdSJjXUGhaUAYkNqHHqReasCg7QUvSZQy93oYrPZnO5vP5naFvLFEqlsvlitwytV6pyWt1zduxF89wYUxgMz0p0WxR+KYxW0D3RWDAPGwTww7gu7It7F+FovFHEl0rlCuOE7VGpnycyC5GYwexw3OIt0uLc9wDA8EAYVpNlPFkBS7W9rwQgd70fEcXyVFV32nXhuB1L9wh/JdHmeM53k+PlgLWUCknAw9WiDE9kwZTICLCZkACkOAwCYYF4PRhDiDANkJCBSiAhQEjWYTRPEtUaXzKtCyaEs6wSHUGDYnc8gKKRTS3AAFYQAGsjKMCAMBlXhMQM2whCHG0AGkYA2X4VNLbo4i0zIXU6ZyNgYYyXNwaiFAYbQIFCjErA4NBvJ9dy6wYSBaz0d05zbJErD0KpOkSvRyxqAsGny+s/JcptMtbTKwmhYBZG4WUnj0DA4hS1SCoAQiDXhZG0bRtni2qYHqxqZWaqBWoio8gzoQaMrbOqGqa3AWriGKj3CiB5qGwFgRU2Q4SUTEuF4OAxLgDlYGUBgACFDuDbRcAgBhVPsh9eAYDkYHwNZx14SokRgAByFUsUlKpvuS4tbA4Z7eBYJMFu9IEGAMl5pAAMUhAADCKcYYaAGBxmKCcRHRCn4lUy2AKp+O4GYJxTcdMUh7KtogDQ4uR25UaGZ7oc0Q7cQYY7TqYRhZGKXAihl3BsFKKp/sB5FAv4RFNJ53Y+ayPG+O4DkoKDYoWBibAEAANQKWQYBxtYzkxFMTsqGXUW6Wbdp1zIHa+qxZBlsAznRQnqdKCKNbOBgpYUFhNZq8IvfCfRsBVGWIfxWRgAzCoh1egqIs5wWOsdjAXs94Ehhxrd8A4TFMV0CNpoAagYOJ1o4Bg4C2pJugPchthx3bq9r+uqiDCSMASNu4mjyeNpbmfigSCKe77nateGVGqUmra1hrx2fY2q0NCgTmoC4EptFc32+NekzRdr4MZahrRzIwRGN6GXz/MC4KjlKDEQYXQAFUpQK2UDKOudoYAIiChsEK3dYK3G/i5X+8CYgjwbkAmBDBQHy0VrKKBLo4EIP3qPea8cwgoICiQ6QHBuBWEAQwEBYCCGQMxNA2BwU1gxQoZQ7eNJOFoNwP/DAzocEkIwXXXQ4iuHwLoQwsRRdSzr0oV0H+r8LIRkkZgmAnhy4MAAOoom0NMTEAU3rcQ+jAR201D4dx9hoB6L9lRaI6BtLurAwgGKso7VWJ94A5DALAJQYUABkWIiQhL4hiWm4cP5qNofQxhEYWH4IgUQiRwUFGMLWHAfuSDdgWitEInR0iYCyOETkye+jP5ZC3LoABjcICFOGIiGKDc4h4PASwaouB2FjhIoBYhf8YFKJGfI5Jk90pqOqmovaaMMbYwYJjImxQdKFBOtWZRdZn7sygGcbgVQwDZTAMZV2sA0RaP4jYmxu1MiJzCMrKoqtjLqxvgFQOvBg601/Nwf2RQWA3RLE8G+d835Im4pyB6tMRblCesoe54RsTBAYE3JujSMCiGEaI1JODulsMyXIkRYy1gFIYKkOO8ywjtJ0FUNqMM6wsCGQsOIJDRFrE0dM2p1KwgoqCFUqZeKEQEoyRwiZuBql5PJSxXltLOntVLMygCrKuVrCSYo9SPLeUcARF01hEDYDJyUKU0ZADKkasYUkQghAkVDFFb0/pdcxwrjXHMVlEqOXMKyZMzVCRUZxExTNZhUyZm8syMAaYxltXUuKRoU18CvUSuqXw+Osr46owMg9LlKYwXn0UVtQOQ4EmUMtUwtJPT2EJoQR4hgBSh56s6g6mURrsAms9aS71xKU1hvjnMtRcbq1Jp9ZK0NrShgMl2sQAA7OFMZ46KVaXPMhK88F+wikHB9J8o5XxYSnJqXC+E6nrMNLpE0ZoGACO8E5FyblGV6E8rtahzC6CVPKgFKA5A30/ygKQbmai8r3rLIqtSu12k5S6KVQqlZqylRvRsSqbZ+1tiBAI0gQioCvq7cwr9Oy0oLoeQCNDGGsMul/WsED+Hy7EbI7hsjf68NQFUZQ3VrdKNQGVU8Dc5EviYedP3XtNVkMZuBJho2OG7XwjY0BzjpFeA8b5J+50pB1KSeE/M1GYmGI4fE7+yT35p3YtRn032cMmCzu4GMR2RNgDYEdqUImhIoClAAPySfTRp0TjBtOfsk5p/w0FfN1N2DCVaUmGWpQ4yysiHxeOkZU4J2Z+mE5ed0wFiTwW2yDto5U39qb5kebUZp7zQCqCZaGP5qCGXw2VcC3QSToWYBqeS2EWrL7dNlfDZkd9X1/RALoARtsYmgGkEG96X9qTyBjeQeQWMmhpuJnK4upbjXdrqcypp2bdXJOsYix1KLKqzh2gY5+1TS3MjrcoW1z9VXMMtcvZaeNZHSM4LywttIfnRNbZfZ9nTPnSANYReF9jsnhknbmolyhl2RN/ZG7d+r53hg9YwH1l972v6MFK+jrIN2RvY58v4IMKP4zud+zd7biPWtfbS2TkrzCAeU4YKtxn0OaqbfhzTxnmRsuvZewiU72PCttiFxOmgqQgA
            internal static int PickPivotAndPartition<TKey, TComparer>(
                ref TKey keys, int lo, int hi,
                TComparer comparer)
                where TComparer : IDirectComparer<TKey>
            {
                Debug.Assert(comparer != null);
                Debug.Assert(lo >= 0);
                Debug.Assert(hi > lo);

                // Compute median-of-three. But also partition them, since we've done the comparison.

                // PERF: `lo` or `hi` will never be negative inside the loop,
                //       so computing median using uints is safe since we know 
                //       `length <= int.MaxValue`, and indices are >= 0
                //       and thus cannot overflow an uint. 
                //       Saves one subtraction per loop compared to 
                //       `int middle = lo + ((hi - lo) >> 1);`
                int middle = (int)(((uint)hi + (uint)lo) >> 1);

                // Sort lo, mid and hi appropriately, then pick mid as the pivot.
                ref TKey keysLeft = ref Unsafe.Add(ref keys, lo);
                ref TKey keysMiddle = ref Unsafe.Add(ref keys, middle);
                ref TKey keysRight = ref Unsafe.Add(ref keys, hi);
                Sort3(ref keysLeft, ref keysMiddle, ref keysRight, comparer);

                TKey pivot = keysMiddle;

                // We already partitioned lo and hi and put the pivot in hi - 1.  
                // And we pre-increment & decrement below.
                keysRight = ref Unsafe.Add(ref keysRight, -1);
                Swap(ref keysMiddle, ref keysRight);

                int left = lo;
                while (Unsafe.IsAddressLessThan(ref keysLeft, ref keysRight))
                {
                    // PERF: For internal direct comparers the range checks are not needed
                    //       since we know they cannot be bogus i.e. pass the pivot without being false.
                    do { ++left; keysLeft = ref Unsafe.Add(ref keysLeft, 1); } 
                    while (comparer.LessThan(keysLeft, pivot));

                    do { keysRight = ref Unsafe.Add(ref keysRight, -1); }
                    while (comparer.LessThan(pivot, keysRight));

                    if (Unsafe.AreSame(ref keysLeft, ref keysRight) ||
                        Unsafe.IsAddressGreaterThan(ref keysLeft, ref keysRight))// (left >= right)
                        break;

                    Swap(ref keysLeft, ref keysRight);
                }
                // Put pivot in the right location.
                keysRight = ref Unsafe.Add(ref keys, hi - 1);
                if (!Unsafe.AreSame(ref keysLeft, ref keysRight))
                {
                    Swap(ref keysLeft, ref keysRight);
                }

                return left;
            }
        }
    }
}
