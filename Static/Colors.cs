/*! @file       Static/Colors.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-29
 *
 *  @brief      CHRIST for the love of god, don't let Levi
 *              go crazy in this file. KEEP IT SIMPLE STUPID!
**/

// ReSharper disable HeapView.BoxingAllocation

using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  [PublicAPI]
  public static class Colors
  {
    public static readonly Color32 None       = new Color32(0x00, 0x00, 0x00, 0x00);
    public static readonly Color32 Clear      = new Color32(0xFF, 0xFF, 0xFF, 0x00);
    public static readonly Color32 Black      = new Color32(0x00, 0x00, 0x00, 0xFF);
    public static readonly Color32 White      = new Color32(0xFF, 0xFF, 0xFF, 0xFF);
    public static readonly Color32 Gray       = new Color32(0x88, 0x88, 0x88, 0xFF);
    public static readonly Color32 Red        = new Color32(0xFF, 0x00, 0x00, 0xFF);
    public static readonly Color32 Green      = new Color32(0x00, 0xFF, 0x00, 0xFF);
    public static readonly Color32 Blue       = new Color32(0x00, 0x00, 0xFF, 0xFF);
    public static readonly Color32 Yellow     = new Color32(0xFF, 0xFF, 0x00, 0xFF);
    public static readonly Color32 Magenta    = new Color32(0xFF, 0x00, 0xFF, 0xFF);
    public static readonly Color32 Cyan       = new Color32(0x00, 0xFF, 0xFF, 0xFF);
    public static readonly Color32 Orange     = new Color32(0xFF, 0x88, 0x00, 0xFF);

    public static readonly Color32 Bright     = new Color32(0xFF, 0xF9, 0xF9, 0xFF);
    public static readonly Color32 Medium     = new Color32(0xD3, 0xD8, 0xD8, 0xFF);
    public static readonly Color32 Dim        = new Color32(0xAA, 0x9C, 0x9C, 0xFF);
    public static readonly Color32 Boring     = new Color32(0x5C, 0x5C, 0x5C, 0xFF);
    public static readonly Color32 Dark       = new Color32(0x12, 0x10, 0x10, 0xFF);

    public static readonly Color32 Background = new Color32(0x2C, 0x2A, 0x2A, 0xDB);
    public static readonly Color32 Attention  = new Color32(0xCA, 0x26, 0x22, 0xFF);
    public static readonly Color32 Pending    = new Color32(0x83, 0x42, 0x83, 0x99);
    public static readonly Color32 Success    = new Color32(0x54, 0xAA, 0x54, 0xFF);
    public static readonly Color32 Info       = new Color32(0x2C, 0x8F, 0xAB, 0x99);

    public static readonly Color32 Keyword    = new Color32(0x56, 0x9A, 0xD1, 0xFF);
    public static readonly Color32 Type       = new Color32(0x86, 0xC6, 0x91, 0xFF);
    public static readonly Color32 Reference  = new Color32(0x4E, 0xC9, 0xB1, 0xFF);
    public static readonly Color32 Literal    = new Color32(0xFF, 0x83, 0x4E, 0xFF);
    public static readonly Color32 Value      = new Color32(0xB5, 0xCE, 0xA8, 0xFF);
    public static readonly Color32 Comment    = new Color32(0x57, 0xA6, 0x4A, 0xFF);


    private const byte BUMP_STEP = 0x28;
    private const byte BUMP_CEIL = 0xFF - BUMP_STEP;

    private const float GOOD_MIN_HSV_VALUE  = 0.09f;
    private const float GOOD_MAX_HSV_VALUE  = 0.91f;
    private const float HSV_VALUE_DARK      = 0.52f;
    private const float HSV_VALUE_LIGHT     = 0.55f;


    public static bool IsClear(this Color32 c)
    {
      return c.a == 0x00;
    }

    public static bool IsDefault(this Color32 c)
    {
      // no == operator for Color32, so:
      return c.ToInt32() == 0x00000000;
    }


    public static bool AreEqual(Color32 a, Color32 b)
    {
      return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
    }


    public static Color32 Random(float minHsvValue = GOOD_MIN_HSV_VALUE, float maxHsvValue = GOOD_MAX_HSV_VALUE)
    {
      minHsvValue *= 255f;
      maxHsvValue *= 255f;
      return new Color32(
        r: (byte)(minHsvValue + UnityEngine.Random.value * (maxHsvValue - minHsvValue)).AtMost(255f),
        g: (byte)(minHsvValue + UnityEngine.Random.value * (maxHsvValue - minHsvValue)).AtMost(255f),
        b: (byte)(minHsvValue + UnityEngine.Random.value * (maxHsvValue - minHsvValue)).AtMost(255f),
        a: 0xFF
      );
    }

    public static Color32 RandomDark()
      => Random(maxHsvValue: HSV_VALUE_DARK);

    public static Color32 RandomLight()
      => Random(minHsvValue: HSV_VALUE_LIGHT);

    public static Color32 RandomGray(float minHsvValue = GOOD_MIN_HSV_VALUE, float maxHsvValue = GOOD_MAX_HSV_VALUE)
    {
      minHsvValue *= 255f;
      maxHsvValue *= 255f;
      byte v = (byte)(minHsvValue + UnityEngine.Random.value * (maxHsvValue - minHsvValue)).AtMost(255f);
      return new Color32(v, v, v, 0xFF);
    }


    public static Color32 Quantized(this Color c)
    {
      // silly but handy...
      return c;
    }


    /// <summary>
    /// Inverts the R,G,B channels of a color.
    /// </summary>
    public static Color32 Inverted(this Color32 c)
    {
      return new Color32( 
        r: (byte)(0xFF - c.r),
        g: (byte)(0xFF - c.g),
        b: (byte)(0xFF - c.b),
        a: c.a
      );
    }

    /// <summary>
    /// Inverts only the alpha channel of the given color.
    /// </summary>
    public static Color32 AlphaInverted(this Color32 c)
    {
      c.a = (byte)(0xFF - c.a);
      return c;
    }


    public static Color32 Alpha(this Color32 c, byte a)
    {
      c.a = a;
      return c;
    }

    public static Color32 Alpha(this Color32 c, float percent)
    {
      c.a = (byte)(percent * 0xFF);
      return c;
    }

    public static Color32 AlphaBump(this Color32 c)
    {
      if (c.a >= BUMP_CEIL)
        c.a = 0xFF;
      else
        c.a += BUMP_STEP;
      return c;
    }

    public static Color32 AlphaBump(this Color32 c, int i)
    {
      int alpha = c.a + BUMP_STEP * i;

      if (alpha >= 0xFF)
        c.a = 0xFF;
      else
        c.a = (byte)alpha;
      return c;
    }

    public static Color32 AlphaWash(this Color32 c)
    {
      if (c.a < BUMP_STEP)
        c.a = 0x00;
      else
        c.a -= BUMP_STEP;
      return c;
    }

    public static Color32 AlphaWash(this Color32 c, int i)
    {
      int alpha = c.a - BUMP_STEP * i;

      if (alpha <= 0x00)
        c.a = 0x00;
      else
        c.a = (byte)alpha;
      return c;
    }


    /// <summary>
    /// Calculates the grayscale color of the given color.
    /// </summary>
    public static Color32 Grayscaled(this Color32 c)
    {
      c.r = c.g = c.b = (byte)(((float)c.r + c.g + c.b) / 3f);
      return c;
    }

    /// <summary>
    /// This overload lerps towards the color's grayscale by parameter `t`.
    /// </summary>
    /// <param name="t">
    /// Normalized `t` value [0f,1f]. Not clamped or checked.
    /// </param>
    public static Color32 Grayscaled(this Color32 c, float t)
    {
      float gray = ((float)c.r + c.g + c.b) / 3f;

      c.r = (byte)(c.r + (gray - c.r) * t);
      c.g = (byte)(c.g + (gray - c.g) * t);
      c.b = (byte)(c.b + (gray - c.b) * t);

      return c;
    }


    public static float HSVValue(this Color32 c)
    {
      if (c.r >= c.g && c.r >= c.b)
        return c.r / 255f;
      if (c.g >= c.r && c.g >= c.b)
        return c.g / 255f;
      else
        return c.b / 255f;
    }

    public static Color32 HSVValue(this Color32 c, float newValue)
    {
      newValue /= HSVValue(c);
      return new Color32(
        r: (byte)(c.r * newValue).AtMost(255f),
        g: (byte)(c.g * newValue).AtMost(255f),
        b: (byte)(c.b * newValue).AtMost(255f),
        a: c.a
      );
    }


    /// <summary>
    /// The main benefit of a <see cref="Color32"/> is that it can fit entirely
    /// in 4 bytes (or, the size of an Int32). Unfortunately, Unity did not
    /// think we would want access to this Int32 representation, so we have to
    /// write functions like these.
    /// </summary>
    public static int ToInt32(this Color32 c)
    {
      // I am SO pissed that Unity decided to hide the rgba field...
      // AND access to the InternalEquals(other) method... 
      return c.a << 24 | c.b << 16 | c.g << 8 | c.r;
    }

    /// <param name="i">
    /// A 4 byte integer representing a Color32 (as in the return value of
    /// <see cref="ToInt32"/>).
    /// </param>
    /// <returns>
    /// The Color32 whose internal representation is exactly equivalent to the
    /// bytes passed in as <paramref name="i"/>.
    /// </returns>
    public static Color32 FromInt32(int i)
    {
      return new Color32(r: (byte)(0xFF & i),
                         g: (byte)(0xFF & i >>  8),
                         b: (byte)(0xFF & i >> 16),
                         a: (byte)(0xFF & i >> 24));
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
      return Parsing.TryParseColor32(hex, out Color32 c) ? c : fallback;
    }

    public static bool TryParse(string hex, out Color32 c)
    {
      return Parsing.TryParseColor32(hex, out c);
    }

  } // end static class Colors

}
