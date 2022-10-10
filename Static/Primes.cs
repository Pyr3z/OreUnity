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

      int idx = s_1p1xHash97Primes.BinarySearch(current);
      if (idx < 0)
      {
        idx = ~idx;
      }

      ++idx;

      while (idx < s_1p1xHash97Primes.Length) // (average case)
      {
        current = s_1p1xHash97Primes[idx];
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
        if (IsPrimeNoLookup(current, sqrt))
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
    public static IReadOnlyList<int> HashtableSizes => s_1p1xHash97Primes;


    private static readonly int[] s_1p1xHash97Primes =
    {
      5,7,11,17,19,23,29,37,43,47,
      59,67,79,89,101,113,127,149,167,191,
      211,239,263,293,331,367,409,457,503,557,
      617,683,751,827,911,1009,1117,1231,1361,1499,
      1657,1823,2011,2221,2447,2699,2971,3271,3607,3989,
      4391,4831,5323,5857,6449,7103,7817,8599,9461,10427,
      11471,12619,13883,15271,16811,18503,20357,22397,24659,27127,
      29851,32839,36131,39749,43753,48131,52951,58271,64109,70529,
      77587,93901,103291,113623,124987,137491,151243,166393,183037,201359,
      221497,243647,268043,294859,324361,356803,392489,431759,474937,522439,
      574687,632189,695411,764969,841541,925697,1018271,1120109,1232171,1355399,
      1490941,1640053,1804063,1984471,2182931,2401237,2641363,2905549,3196111,3515731,
      3867323,4254083,4679533,5147497,5662249,6228493,6851347,7536499,8290151,9119167,
      10031113,11034271,12137701,13351537
    };

  } // end class Primes
}
