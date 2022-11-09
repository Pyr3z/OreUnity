/*! @file   Static/Hashing.cs
 *  @author levi\@leviperez.dev
 *  @date   2020-06-06
**/

using System.Collections.Generic;
using JetBrains.Annotations;


namespace Ore
{
  /// <summary>
  /// Utility functions for making/combining hash codes.
  /// </summary>
  public static class Hashing
  {
    [PublicAPI]
    public static IReadOnlyList<int> HashPrimes => HASHPRIMES;

    public const int DefaultHashPrime = 193; // HASHPRIMES[3]


    public static int SafeGetHashCode(object obj)
    {
      return obj?.GetHashCode() ?? 0;
    }


    public static uint MixHashes(uint h0, uint h1)
    {
      return h0 + (h0 << 5 | h0 >> 27) ^ h1;
    }
    public static uint MixHashes(int h0, int h1)
      => MixHashes((uint)h0, (uint)h1);

    public static uint MixHashes(uint h0, uint h1, uint h2)
    {
        h0 = h0 + (h0 << 5 | h0 >> 27) ^ h1;
      return h0 + (h0 << 5 | h0 >> 27) ^ h2;
    }
    public static uint MixHashes(int h0, int h1, int h2)
      => MixHashes((uint)h0, (uint)h1, (uint)h2);

    public static uint MixHashes(uint h0, uint h1, uint h2, uint h3)
    {
        h0 = h0 + (h0 << 5 | h0 >> 27) ^ h1;
        h0 = h0 + (h0 << 5 | h0 >> 27) ^ h2;
      return h0 + (h0 << 5 | h0 >> 27) ^ h3;
    }
    public static uint MixHashes(int h0, int h1, int h2, int h3)
      => MixHashes((uint)h0, (uint)h1, (uint)h2, (uint)h3);


    public static int MakeHash(long i64)
    {
      return (int)MixHashes((uint)i64, (uint)(i64 >> 32));
    }


    public static int MakeHash(object a, object b)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b));
    }

    public static int MakeHash(object a, object b, object c)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b),
                            (uint)SafeGetHashCode(c));
    }

    public static int MakeHash(object a, object b, object c, object d)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b),
                            (uint)SafeGetHashCode(c),
                            (uint)SafeGetHashCode(d));
    }


    // 1. Each prime is slightly less than twice the size of the previous.
    // 2. By extension, each prime is as far as possible from the nearest two powers of 2 (lbound & rbound).
    private static readonly int[] HASHPRIMES =
    {            // lbound rbound % error  comment?
      53,        // 2^5    2^6    10.4167  I tried it, it's bad
      97,        // 2^6    2^7     1.0417  Allegedly well-suited for dynamic tables expected to grow
      101,       // 2^6    2^7     5.2083  Microsoft's favourite hashprime
      193,       // 2^7    2^8     0.5208  Allegedly very fast in practice
      389,       // 2^8    2^9     1.3021
      769,       // 2^9    2^10    0.1302
      1543,      // 2^10   2^11    0.4557
      3079,      // 2^11   2^12    0.2279
      6151,      // 2^12   2^13    0.1139
      12289,     // 2^13   2^14    0.0081
      24593,     // 2^14   2^15    0.0692
      49157,     // 2^15   2^16    0.0102
      98317,     // 2^16   2^17    0.0132
      196613,    // 2^17   2^18    0.0025
      393241,    // 2^18   2^19    0.0064
      786433,    // 2^19   2^20    0.0001
      1572869,   // 2^20   2^21    0.0003
      3145739,   // 2^21   2^22    0.0003
      6291469,   // 2^22   2^23    0.0002
      12582917,  // 2^23   2^24    0.0000
      25165843,  // 2^24   2^25    0.0001
      50331653,  // 2^25   2^26    0.0000
      100663319, // 2^26   2^27    0.0000
      201326611, // 2^27   2^28    0.0000
      402653189, // 2^28   2^29    0.0000
      805306457, // 2^29   2^30    0.0000
      1610612741 // 2^30   2^31    0.0000
    };

  } // end static class Hashing

}
