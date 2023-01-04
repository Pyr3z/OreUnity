/*! @file   Static/Integers.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2020-06-06
 *
 *  @brief
 *    Provides utilities for native integral primitives and other types that
 *    implement `System.IConvertible`.
**/

using JetBrains.Annotations;
using UnityEngine;
using IConvertible = System.IConvertible;

using CultureInfo = System.Globalization.CultureInfo;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


namespace Ore
{
  /// <summary>
  /// Provides utilities for native integral primitives and other types that
  /// implement `System.IConvertible`.
  /// </summary>
  [PublicAPI]
  public static class Integers
  {
    // Maximum 1D array size, slightly smaller than int.MaxValue (grabbed from decompiled System.Array)
    public const int MaxArraySize     = Primes.MaxValue;

    // logical 2D arrays assume a square grid:
    public const int MaxArray2DSize   = 46329; // floor(sqrt(MaxArraySize))
    public const int MaxArray2DExtent = MaxArraySize / 2;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalcDigits(int self)
    {
      return self < 10 ? 1 : (int)System.Math.Log10(self - 1) + 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string MakeIndexPreformattedString(int size)
    {
      return $"[{{0,{CalcDigits(size)}}}]";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int RandomIndex(int size)
    {
      // order of operations here is intentional.
      return (int)(size * UnityEngine.Random.value - Floats.EPSILON);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalcExtent(int size)
    {
      return size < 0 ? CalcExtent(unchecked((uint)(-1 * size))) : CalcExtent((uint)size);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CalcExtent(uint size)
    {
      return size < 4u ? 1 : ((int)(size / 2)).AtMost(MaxArray2DExtent, warn: true);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Compare(int lhs, int rhs)
    {
      // TODO test if inlining screws up use of this as a function object
      return lhs - rhs;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ReverseCompare(int lhs, int rhs)
    {
      // TODO test if inlining screws up use of this as a function object
      return rhs - lhs;
    }


  #region Extension methods


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Abs(this long self) // branchless!
    {
      long mask = self >> 63;
      return self + mask ^ mask;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Abs(this int self)
    {
      int mask = self >> 31;
      return self + mask ^ mask;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Sign(this int self)
    {
      return self < 0 ? -1 : self > 0 ? +1 : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int SignNoZero(this int self)
    {
      return self < 0 ? -1 : 1;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtLeast(this int self, int min)
    {
      return self < min ? min : self;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int AtMost(this int self, int max)
    {
      return self > max ? max : self;
    }

    public static int AtLeast(this int self, int min, bool warn)
    {
      if (self < min)
      {
        #if UNITY_ASSERTIONS
          OAssert.False(warn, $"{self} < {min}");
        #endif
        return min;
      }

      return self;
    }

    public static int AtMost(this int self, int max, bool warn)
    {
      if (max < self)
      {
        #if UNITY_ASSERTIONS
          OAssert.False(warn, $"{max} < {self}");
        #endif
        return max;
      }

      return self;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(this int self, int min, int max)
    {
      return self < min ? min : self > max ? max : self;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ClampIndex(this int idx, int sz)
    {
      return (sz == 0 || sz <= idx) ? sz - 1 : idx;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int WrapIndex(this int i, int sz)
    {
      // the extra math here supports wrapping negative indices, so i = -1
      // would return sz - 1.
      return (i % sz + sz) % sz;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this long self, long minInclusive, long maxExclusive)
    {
      return self >= minInclusive && self < maxExclusive;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this int self, int minInclusive, int maxExclusive)
    {
      return self >= minInclusive && self < maxExclusive;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsBetween(this short self, short minInclusive, short maxExclusive)
    {
      return self >= minInclusive && self < maxExclusive;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(this long self)
    {
      return (0 < self) == ((self & 1) == 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(this ulong self)
    {
      return (self & 1) == 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(this int self)
    {
      return (0 < self) == ((self & 1) == 0);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEven(this uint self)
    {
      return (self & 1) == 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPrime(this int self)
    {
      return Primes.IsPrime(self);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsPrime(this uint self)
    {
      return Primes.IsPrime(self);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ToBool(this int self)
    {
      return self != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int ToInt(this bool self)
    {
      return self ? 1 : 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Truncate(this ulong self)
    {
      return 0 < self ? 1 : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Truncate(this uint self)
    {
      return 0 < self ? 1 : 0;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Truncate(this long self)
    {
      return self == 0 ? 0 : 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Truncate(this int self)
    {
      return self == 0 ? 0 : 1;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Cast<T>(this IConvertible self)
      where T : IConvertible
    {
      return (T)self.ToType(typeof(T), CultureInfo.InvariantCulture);
    }

  #endregion Extension methods

  } // end static class Integers

}
