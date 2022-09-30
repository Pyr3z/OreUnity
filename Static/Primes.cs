/*! @file       Static/Primes.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-18
**/

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

      if (ConvenientPrimes.BinarySearch(value) >= 0)
        return true;

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

    internal static bool IsPrimeNoLookup(int value, int sqrt)
    {
      for (int i = 3; i <= sqrt; i += 2)
      {
        if (value % i == 0)
          return false;
      }

      return true;
    }

    [PublicAPI]
    public static int Next(int current, int hashprime = int.MaxValue)
    {
      if (current < MinValue)
        return MinValue;
      if (current >= MaxValue)
        return MaxValue;

      int idx = ConvenientPrimes.BinarySearch(current);
      if (idx < 0)
      {
        idx = ~idx;
      }

      ++idx;

      while (idx < ConvenientPrimes.Length) // (average case)
      {
        current = ConvenientPrimes[idx];
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


    internal static readonly int[] ConvenientPrimes =
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

  } // end class Primes
}
