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

    public const int MaxHashtableSize = 7199369;


    [NotNull]
    public static IReadOnlyList<int> HashtableSizes => s_1p1xHash97Primes;


    private static readonly int[] s_1p2xHash101Primes =
    {
      // chooses primes closest to the curve y=2+1.2^x and where (prime - 1) % 101 != 0
      3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
      89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
      631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
      4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
      25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
      156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
      968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
      5999471, 7199369
    };

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

    private static readonly int[] s_150SmallPrimes =
    {
      31,37,41,43,47,53,59,61,67,71,
      73,79,83,89,97,101,103,107,109,113,
      127,131,137,139,149,151,157,163,167,173,
      179,181,191,193,197,199,211,223,227,229,
      233,239,241,251,257,263,269,271,277,281,
      283,293,307,311,313,317,331,337,347,349,
      353,359,367,373,379,383,389,397,401,409,
      419,421,431,433,439,443,449,457,461,463,
      467,479,487,491,499,503,509,521,523,541,
      547,557,563,569,571,577,587,593,599,601,
      607,613,617,619,631,641,643,647,653,659,
      661,673,677,683,691,701,709,719,727,733,
      739,743,751,757,761,769,773,787,797,809,
      811,821,823,827,829,839,853,857,859,863,
      877,881,883,887,907,911,919,929,937,941,
      947,953,967,971,977,983,991,997,1009,1013,
      1019,1021,1031,1033,1039,1049,1051,1061,1063,1069,
    };

  } // end class Primes
}
