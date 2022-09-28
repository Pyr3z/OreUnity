/*! @file       Static/Primes.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
**/

using JetBrains.Annotations;

using Math = System.Math;


namespace Ore
{
  [PublicAPI]
  public static class Primes
  {
    public static bool IsPrime(int value)
    {
      if (value < 2)
        return false;

      if ((value & 1) == 0) // "is even"
        return value == 2;

      return CONVENIENT_PRIMES.BinarySearch(value) >= 0 ||
             IsPrimeNoLookup(value, (int)Math.Sqrt(value));
    }

    public static bool IsPrimeNoLookup(int value)
    {
      return IsPrimeNoLookup(value, (int)Math.Sqrt(value));
    }

    public static bool IsPrimeNoLookup(int value, int sqrt)
    {
      for (int i = 3; i <= sqrt; i += 2)
      {
        if (value % i == 0)
          return false;
      }

      return true;
    }


    public static int Next(int current)
    {
      if (OAssert.Fails(current >= 0, "integer overflow?"))
      {
        return Integers.MAX_ARRAY_SZ;
      }

      int idx = CONVENIENT_PRIMES.BinarySearch(current);
      if (idx < 0)
      {
        idx = ~idx;
      }

      ++idx;

      if (idx < CONVENIENT_PRIMES.Length)
      {
        // (average case)
        return CONVENIENT_PRIMES[idx];
      }

      // rarer case (will still happen for massive data structures)

      current = current | 1 + 2;

      int sqrt = (int)Math.Sqrt(current);

      while (current < Integers.MAX_ARRAY_SZ)
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

      return Integers.MAX_ARRAY_SZ;
    }


    // data section (pruned)

    private static readonly int[] CONVENIENT_PRIMES =
    {
      // skips over many intermediate primes on a curve
      3, 7, 11, 17, 23, 29, 37, 47, 59, 71,
      89, 107, 131, 163, 197, 239, 293, 353, 431, 521,
      631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371,
      4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023,
      25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363,
      156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403,
      968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559,
      5999471, 7199369
    };

  } // end class Primes
}
