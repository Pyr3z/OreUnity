/*! @file   Static/Bitwise.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2020-06-06
 *
 *  @brief
 *    Fast bitwise utilities and syntax sugars for it.
**/

using JetBrains.Annotations;

using IConvertible = System.IConvertible;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


namespace Ore
{
  [PublicAPI]
  public static partial class Bitwise
  {

  #region LSB  (Least Significant Bit)

    public static ulong LSB(ulong bits)
    {
      return bits & (~bits + 1);
    }

    public static uint LSB(uint bits)
    {
      return bits & (~bits + 1);
    }

    public static ushort LSB(ushort bits)
    {
      return (ushort)(bits & (~bits + 1));
    }

    public static byte LSB(byte bits)
    {
      return (byte)(bits & (~bits + 1));
    }


    public static long LSB(long bits)
    {
      return bits & -bits;
    }

    public static int LSB(int bits)
    {
      return bits & -bits;
    }

    public static short LSB(short bits)
    {
      return (short)(bits & -bits);
    }

    public static sbyte LSB(sbyte bits)
    {
      return (sbyte)(bits & -bits);
    }


    // LSBye = removes the Least Significant Bit ;)

    public static ulong LSBye(ulong bits)
    {
      return bits & ~(bits & (~bits + 1));
    }

    public static uint LSBye(uint bits)
    {
      return bits & ~(bits & (~bits + 1));
    }

    public static ushort LSBye(ushort bits)
    {
      return (ushort)(bits & ~(bits & -bits));
    }

    public static byte LSBye(byte bits)
    {
      return (byte)(bits & ~(bits & -bits));
    }


    public static long LSBye(long bits)
    {
      return bits & ~(bits & -bits);
    }

    public static int LSBye(int bits)
    {
      return bits & ~(bits & -bits);
    }

    public static short LSBye(short bits)
    {
      return (short)(bits & ~(bits & -bits));
    }

    public static sbyte LSBye(sbyte bits)
    {
      return (sbyte)(bits & ~(bits & -bits));
    }

  #endregion LSB  (Least Significant Bit)

  #region Bitwise operators missing from C#

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NOT(this ulong self)
    {
      return 0 < self ? 0 : 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NOT(this uint self)
    {
      return 0 < self ? 0 : 1;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NOT(this long self)
    {
      return self == 0 ? 1 : 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int NOT(this int self)
    {
      return self == 0 ? 1 : 0;
    }

  #endregion Bitwise operators missing from C#

  #region Bitflag operations

    public static bool HasFlag<TFlag>(this int self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
      int value = flag.ToInt32(null);
      return (self & value) == value;
    }


    public static bool HasFlag<TFlag>(this long self, TFlag flag)
      where TFlag : unmanaged, IConvertible
    {
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

  #endregion Bitflag operations

  } // end static partial class Bitwise

}
