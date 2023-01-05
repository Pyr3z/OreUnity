/*! @file       Static/Primes.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
**/

using JetBrains.Annotations;

using System.Collections.Generic;

using Math = System.Math;

using Random = UnityEngine.Random;


namespace Ore
{
  public static class Primes
  {
    // TODO make me an OAssetSingleton? so we can use serialization to choose the best prime pool per project!

    [NotNull]
    public static IReadOnlyList<int> HashableSizes => s_1p15xHash193Sizes;


    public const int MinValue     = 3;          // I know, technically 2 is prime... BUT NOT IN MY HAUS
    public const int MaxValue     = 2146435069; // largest prime that can also be an array size
    public const int MaxSizePrime = 12633961;   // last value of the precomputed HashtableSizes list

    public const long  MaxLongValue         = (1L  << 62) - 57; // https://primes.utm.edu/lists/2small/0bit.html
    public const ulong MaxUnsignedLongValue = (1UL << 63) - 25;


    [PublicAPI]
    public static bool IsPrime(int value)
    {
      return IsPrimeNoLookup(value);
    }

    [PublicAPI]
    public static bool IsPrime(long value)
    {
      return IsLongPrimeNoLookup(value);
    }


    [PublicAPI]
    public static int GetRandom(int min = MinValue, int max = MaxValue)
    {
      return NearestTo(Random.Range(min, max));
    }


    [PublicAPI]
    // ReSharper disable once CognitiveComplexity
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

        if (value >= MaxValue)
          return MaxValue;

        if (d > 0)
        {
          d = -1 * (d + 2);

          while (value < sqrt * sqrt)
          {
            // uncommon case that this scope is entered, or even has to loop
            ++ sqrt;
          }
        }
        else
        {
          d = -1 * (d - 2);
        }

        while (value < sqrt * sqrt)
        {
          -- sqrt;
        }

        // Funky fallthrough scopes, I know, but it's fast and works so bite me
      }

      return value;
    }


    [PublicAPI]
    public static int NextHashableSize(int current, int hashprime = int.MaxValue, int incr = 1)
    {
      if (current < MinValue)
        return MinValue;
      if (current >= MaxValue)
        return MaxValue;

      int idx = s_1p15xHash193Sizes.BinarySearch(current);
      if (idx < 0)
      {
        idx = ~idx;
      }

      idx += incr;

      while (idx < s_1p15xHash193Sizes.Length) // (average case)
      {
        current = s_1p15xHash193Sizes[idx];
        if ((current - 1) % hashprime != 0)
          return current;
        ++idx;
      }

      // rarer case (will happen for massive data structures, or really bad hashprimes)
      return NextNoLookup(current + incr, hashprime);
    }

    [PublicAPI]
    public static int Next(int current, int hashprime = int.MaxValue)
    {
      return NextNoLookup(current, hashprime);
    }

    [PublicAPI]
    public static long Next(long current)
    {
      return NextLongNoLookup(current);
    }

  #region Internal section

    private static readonly int[] s_1p15xHash193Sizes =
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

    internal static bool IsPrimeLookup(int value)
    {
      if (value < 2)
        return false;

      if ((value & 1) == 0)
        return value == 2;

      // Note: in practical contexts, BinarySearch here measures much slower than the NoLookup variant
      return s_1p15xHash193Sizes.BinarySearch(value) >= 0 || IsPrimeNoLookup(value);
    }

    internal static bool IsPrimeNoLookup(int value)
    {
      if (value < 2)
        return false;

      if ((value & 1) == 0)
        return value == 2;

      return IsPrimeNoLookup(value, (int)Math.Sqrt(value));
    }

    internal static bool IsLongPrimeNoLookup(long value)
    {
      if (value < 2L)
        return false;

      if ((value & 1) == 0)
        return value == 2L;

      if (value < MaxValue)
        return IsPrimeNoLookup((int)value, (int)Math.Sqrt(value));

      return value == MaxValue || IsLongPrimeNoLookup(value, (long)Math.Sqrt(value));
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

    private static bool IsLongPrimeNoLookup(long value, long sqrt)
    {
      // minor TODO: this is a pretty long walk... is there nothing better?

      for (long i = 3; i <= sqrt; i += 2L)
      {
        if (value % i == 0)
          return false;
      }

      return true;
    }

    internal static int NextNoLookup(int current, int hashprime = int.MaxValue)
    {
      if (current < MinValue)
        return MinValue;
      if (current >= MaxValue)
        return MaxValue;

      current = (current | 1) + 2;

      int sqrt = (int)Math.Sqrt(current);

      while (current < MaxValue)
      {
        if ((current - 1) % hashprime != 0 && IsPrimeNoLookup(current, sqrt))
          return current;

        current += 2;

        ++ sqrt;

        if (current < sqrt * sqrt)
        {
          -- sqrt;
        }
      }

      return MaxValue;
    }

    internal static long NextLongNoLookup(long current)
    {
      if (current < MinValue)
        return MinValue;
      if (current > MaxLongValue)
        return MaxLongValue;

      current = (current | 1) + 2;

      long sqrt = (long)Math.Sqrt(current);

      while (current < long.MaxValue - 1)
      {
        if (IsLongPrimeNoLookup(current, sqrt))
          return current;

        current += 2;

        ++ sqrt;

        if (current < sqrt * sqrt)
        {
          -- sqrt;
        }
      }

      return MaxLongValue;
    }

  #endregion Internal section

  } // end class Primes
}
