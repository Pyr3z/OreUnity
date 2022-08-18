/*! @file       Static/Colors.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-29
 *
 *  @brief      CHRIST for the love of god, don't let Levi
 *              go crazy in this file. KEEP IT SIMPLE STUPID!
**/

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable HeapView.BoxingAllocation

using UnityEngine;


namespace Ore
{

  public static class Colors
  {

    public static int ToInt32(this Color32 c)
    {
      return c.a << 24 | c.b << 16 | c.g << 8 | c.r;
    }

    public static Color32 FromInt32(int i)
    {
      return new Color32(r: (byte)(i & 0xFF),
                         g: (byte)(i >> 8 & 0xFF),
                         b: (byte)(i >> 16 & 0xFF),
                         a: (byte)(i >> 24 & 0xFF));
    }


    public static string ToHex(this Color32 c)
    {
      return $"{c.r:X2}{c.g:X2}{c.b:X2}{c.a:X2}";
    }

    public static Color32 FromHex(string hex)
    {
      _ = Parsing.TryParseColor32(hex, out var c); // Do or do not;
      return c;                                           // there is no "Try".
    }

    public static Color32 FromHex(string hex, Color32 fallback)
    {
      return Parsing.TryParseColor32(hex, out var c) ? c : fallback;
    }


    public static bool IsClear(this Color32 c)
    {
      return c.a == 0x00;
    }

    public static bool IsDefault(this Color32 c)
    {
      // no == operator for Color32, so:
      return c.ToInt32() == 0x00000000;
    }

  } // end static class Colors

}
