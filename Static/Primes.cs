/*! @file       Static/Primes.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
**/

using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;

using Math = System.Math;


namespace Ore
{
  public static class Primes // TODO make me an OAssetSingleton, so we can use serialization to choose our prime pool!
  {

    [PublicAPI]
    public static bool IsPrime(int value)
    {
      if (value < 2)
        return false;

      if ((value & 1) == 0) // "is even"
        return value == 2;

      // return (s_ConvenientPrimes.BinarySearch(value) >= 0) ||
      return IsPrimeNoLookup(value, (int)Math.Sqrt(value));
    }

    internal static bool IsPrimeNoLookup(int value)
    {
      if (value < 2)
        return false;

      if ((value & 1) == 0)
        return value == 2;

      return IsPrimeNoLookup(value, (int)Math.Sqrt(value));
    }

    private static bool IsPrimeNoLookup(int value, int sqrt)
    {
      for (int i = 3; i <= sqrt; i += 2)
      {
        if (value % i == 0)
          return false;
      }

      return true;
    }


    [PublicAPI]
    public static int NearestTo(int value)
    {
      if (value < 3)
        return 2;

      value |= 1;

      int d    = 2;
      int sqrt = (int)Math.Sqrt(value);
      while (!IsPrimeNoLookup(value, sqrt))
      {
        value += d;

        if (d > 0)
        {
          d = -1 * (d + 2);

          while (value < sqrt * sqrt)
            ++sqrt;
        }
        else
        {
          d = -1 * (d - 2);
        }

        while (value < sqrt * sqrt)
          --sqrt;
      }

      return value;
    }


    [PublicAPI]
    public static int Next(int current, int hashprime = int.MaxValue)
    {
      if (current < MinValue)
        return MinValue;
      if (current >= MaxValue)
        return MaxValue;

      int idx = s_1p15xHash193Primes.BinarySearch(current);
      if (idx < 0)
      {
        idx = ~idx;
      }

      ++idx;

      while (idx < s_1p15xHash193Primes.Length) // (average case)
      {
        current = s_1p15xHash193Primes[idx];
        if ((current - 1) % hashprime != 0)
          return current;
        ++idx;
      }

      // rarer case (will happen for massive data structures, or really bad hashprimes)
      return NextNoLookup(current, hashprime);
    }

    [PublicAPI]
    public static int NextNoLookup(int current, int hashprime = int.MaxValue)
    {
      if (current < MinValue)
        return MinValue;
      if (current >= MaxValue)
        return MaxValue;

      current = current | 1 + 2;

      int sqrt = (int)Math.Sqrt(current);

      while (current < MaxValue)
      {
        if ((current - 1) % hashprime != 0 && IsPrimeNoLookup(current, sqrt))
          return current;

        current += 2;

        ++sqrt;

        if (current < sqrt * sqrt)
        {
          --sqrt;
        }
      }

      return MaxValue;
    }


    // data section (pruned)

    public const int MinValue = 3; // I know, technically 2 is prime... BUT NOT IN MY HAUS

    public const int MaxValue = 2146435069; // largest prime that can also be an array size

    public const int MaxConvenientValue = 7199369;

    public const int MaxHashtableSize = 13351537;


    [NotNull]
    public static IReadOnlyList<int> HashtableSizes => s_1p15xHash193Primes;


    private static readonly int[] s_1p15xHash193Primes =
    {
      5,7,11,17,23,29,37,43,53,67,
      79,97,127,149,173,199,233,271,317,367,
      431,499,577,673,787,907,1049,1213,1399,1613,
      1861,2143,2467,2843,3271,3767,4337,4993,5743,6607,
      7603,8747,10061,11579,13327,15329,17657,20323,23371,26879,
      30911,35569,40927,47087,54151,62297,71647,82421,94789,109013,
      125371,144203,165857,190753,219371,252283,290137,333667,383723,441307,
      507503,583631,671189,771877,887659,1020821,1173947,1350047,1552561,1785457,
      2053291,2361323,2715523,3122851,3591281,4129981,4749497,5461931,6281237,7223443,
      8307001,9553057,10986049,12633961,
    };

  } // end class Primes
}
