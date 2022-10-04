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
using IConvertible = System.IConvertible;


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


    public static int CalcDigits(int self)
    {
      return self < 10 ? 1 : (int)System.Math.Log10(self - 1) + 1;
    }

    public static string MakeIndexPreformattedString(int size)
    {
      return $"[{{0,{CalcDigits(size)}}}]";
    }

    public static string ToInvariantString(this IConvertible self)
    {
      return self?.ToString(Strings.InvariantFormatter);
    }


    public static long Abs(this long self) // branchless!
    {
      long mask = self >> 63;
      return self + mask ^ mask;
    }
    public static int Abs(this int self)
    {
      int mask = self >> 31;
      return self + mask ^ mask;
    }


    public static int Sign(this int self)
    {
      if (self < 0)
        return -1;
      if (0 < self)
        return +1;
      return 0;
    }

    public static int SignNoZero(this int self)
    {
      return self < 0 ? -1 : 1;
    }


    public static int AtLeast(this int self, int min)
    {
      return self < min ? min : self;
    }

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


    public static int Clamp(this int self, int min, int max)
    {
      return self < min ? min : self > max ? max : self;
    }


    public static int ClampIndex(this int idx, int size)
    {
      if (size == 0 || size <= idx)
        return size - 1;

      return idx;
    }

    public static int WrapIndex(this int i, int sz)
    {
      // the extra math here supports wrapping negative indices, so i = -1
      // would return sz - 1.
      return (i % sz + sz) % sz;
    }


    public static int RandomIndex(int size)
    {
      // order of operations here is intentional.
      return (int)(size * UnityEngine.Random.value - Floats.EPSILON);
    }


    public static int CalcExtent(int size)
    {
      if (size < 0)
        size *= -1;

      if (size < 4)
        return 1;
      else
        return (size / 2).AtMost(MaxArray2DExtent, warn: true);
    }

    public static int CalcExtent(uint size)
    {
      if (size < 4u)
        return 1;
      else
        return ((int)(size / 2)).AtMost(MaxArray2DExtent, warn: true);
    }


    public static int Compare(int lhs, int rhs)
    {
      return lhs - rhs;
    }

    public static int FlipCompare(int lhs, int rhs)
    {
      return rhs - lhs;
    }


    public static bool HasFlag<TFlag>(this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      // equivalent to Bitwise.HasAllBits()
      int value = flag.ToInt32(null);
      return (self & value) == value;
    }


    public static bool HasFlag<TFlag>(this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      // equivalent to Bitwise.HasAllBits()
      long value = flag.ToInt64(null);
      return (self & value) == value;
    }



    public static long Mask<TFlag>(this long self, TFlag mask)
      where TFlag : unmanaged, IConvertible
    {
      return self & mask.ToInt64(null);
    }

    public static int Mask<TFlag>(this int self, TFlag mask)
      where TFlag : unmanaged, IConvertible
    {
      return self & mask.ToInt32(null);
    }



    public static void SetFlag<TFlag>(ref this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self |= flag.ToInt64(null);
    }

    public static void SetFlag<TFlag>(ref this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self |= flag.ToInt32(null);
    }


    public static void SetFlag<TFlag>(ref this long self, TFlag flag, bool set)
      where TFlag : unmanaged, IConvertible
    {
      if (set)
        self |= flag.ToInt64(null);
      else
        self &= ~flag.ToInt64(null);
    }

    public static void SetFlag<TFlag>(ref this int self, TFlag flag, bool set)
      where TFlag : unmanaged, IConvertible
    {
      if (set)
        self |= flag.ToInt32(null);
      else
        self &= ~flag.ToInt32(null);
    }



    public static void ClearFlag<TFlag>(ref this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self &= ~flag.ToInt64(null);
    }

    public static void ClearFlag<TFlag>(ref this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self &= ~flag.ToInt32(null);
    }



    public static void ToggleFlag<TFlag>(ref this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self ^= flag.ToInt64(null);
    }

    public static void ToggleFlag<TFlag>(ref this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      self ^= flag.ToInt32(null);
    }



    public static bool IsEven(this long self)
    {
      return (0 < self) == ((self & 1) == 0);
    }

    public static bool IsEven(this ulong self)
    {
      return (self & 1) == 0;
    }


    public static bool IsEven(this int self)
    {
      return (0 < self) == ((self & 1) == 0);
    }

    public static bool IsEven(this uint self)
    {
      return (self & 1) == 0;
    }


    public static bool ToBool(this int self)
    {
      return self != 0;
    }
    public static int ToInt(this bool self)
    {
      return self ? 1 : 0;
    }


    public static int ToBinary(this ulong self)
    {
      return 0 < self ? 1 : 0;
    }

    public static int ToBinary(this uint self)
    {
      return 0 < self ? 1 : 0;
    }


    public static int ToBinary(this long self)
    {
      return self == 0 ? 0 : 1;
    }

    public static int ToBinary(this int self)
    {
      return self == 0 ? 0 : 1;
    }



    public static int NOT(this ulong self)
    {
      return 0 < self ? 0 : 1;
    }

    public static int NOT(this uint self)
    {
      return 0 < self ? 0 : 1;
    }


    public static int NOT(this long self)
    {
      return self == 0 ? 1 : 0;
    }

    public static int NOT(this int self)
    {
      return self == 0 ? 1 : 0;
    }

  } // end static class Integers

}
