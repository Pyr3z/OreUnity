/** @file   Runtime/StaticTypes/Bitwise.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2020-06-06

    @brief
      Fast bitwise utilities and syntax sugars.
**/

namespace Bore
{
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

    // LSBye = removes the Least Significant Bit

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

  } // end static partial class Bitwise

}
