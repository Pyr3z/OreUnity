/** @file   Static/Floats.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2020-06-06

    @brief
      Utilities for native IEEE floating-point primitives.
**/


using Math = System.Math;


namespace Bore
{
  public static class Floats
  {
    // my chosen epsilon that is accurate enough for most game applications:
    public const float EPSILON  = 1e-6f; // (Unity tends to use 1e-5f, i.e. Vector3.kEpsilon)
    public const float EPSILON2 = EPSILON * EPSILON;

    public const float SQRT2    = 1.414213562373095f; // precomputed because sqrt(2) is super common.
    public const float SQRT_MAX = 1.844674352e+19f;   // precomputed 32-bit IEEE max squarable value
    
    public const float PI       = 3.1415926535897931f;
    public const float PIPI     = 2 * PI;
    public const float IPIPI    = 1f / PIPI;


    public static float SinusoidalParameter(float t)
    {
      // "stretches" the normalized value `t` so that 1.0 corresponds to 2 * PI,
      // then uses cosine to produce an oscillating value between 0.0 and 1.0
      return Clamp01(((float)Math.Cos(t * PIPI) - 1f) * 0.5f);
    }

    public static float InverseSinusoidalParameter(float t)
    {
      return ((float)Math.Acos(-2f * t + 1f)) * IPIPI;
    }

    public static float SmoothStepParameter(float t)
    {
      if (t < 0f) return 0f;
      if (1f < t) return 1f;
      return (t * t) * (3f - 2f * t);
    }


    public static float SinusoidalLoop(float a, float b, float t)
    {
      return LerpedTo(a, b, SinusoidalParameter(t));
    }


    public static bool Approximately(this float a, float b)
    {
      return (a -= b) * a < EPSILON2;
    }

    public static bool Approximately(this float a, float b, float epsilon)
    {
      return (a -= b) * a < epsilon * epsilon;
    }

    public static bool IsZero(this float val)
    {
      return val * val < EPSILON2;
    }

    public static bool IsZero(this float val, float epsilon)
    {
      return val * val < epsilon * epsilon;
    }

  #if ENABLE_UNSAFE
    public static unsafe bool IsNegativeZero(this float val)
    {
      return (*(int*)(&val) & 0x80000000) == 0x80000000;
    }
  #else
    public static bool IsNegativeZero(this float val)
    {
      return val == 0f && System.BitConverter.GetBytes(val)[sizeof(float)-1] == 0x80;
    }
  #endif // ENABLE_UNSAFE

    public static bool IsNaN(this float val)
    {
      return float.IsNaN(val);
    }

    public static bool IsFinite(this float val)
    {
      return !float.IsNaN(val) && !float.IsInfinity(val);
    }

    public static bool IsSquarable(this float val)
    {
      return !float.IsNaN(val) && !float.IsPositiveInfinity(val * val);
    }

    public static bool IsExtreme(this float val)
    {
      return Math.Abs(val) == float.MaxValue;
    }

    public static float FixNaN(this float val)
    {
      return float.IsNaN(val) ? 0f : val;
    }

    public static float FixNaN(this float val, float fallback)
    {
      return float.IsNaN(val) ? fallback : val;
    }


    public static float MakeFinite(this float val, float finite)
    {
      if (float.IsNaN(val))
        return finite;
      if (float.IsInfinity(val))
        return (0f < val) ? finite : -finite;

      return val;
    }

    public static float ClampSquarable(this float val)
    {
      if (float.IsNaN(val) || !float.IsPositiveInfinity(val * val))
        return val;

      return Sign(val) * SQRT_MAX;
    }


    public static float Abs(this float val)
    {
      return Math.Abs(val);
    }


    public static float Sign(this float val)
    {
      if (EPSILON < val)
        return 1f;
      if (val < -EPSILON)
        return -1f;

      return 0f;
    }

    public static float SignOrNaN(this float val)
    {
      if (EPSILON < val)
        return 1f;
      if (val < -EPSILON)
        return -1f;
      if (float.IsNaN(val))
        return float.NaN;

      return 0f;
    }

    public static float SignNoZero(this float val)
    {
      if (val < 0f)
        return -1f;
      return 1f;
    }


    public static float Sqrt(this float val)
    {
      return (float)System.Math.Sqrt(val);
    }

    public static float Clamp(this float val, float min, float max)
    {
      if (min <= max)
      {
        if (val < min) return min;
        if (max < val) return max;
      }

      return val;
    }

    public static float Clamp01(this float val)
    {
      if (val < 0f) return 0f;
      if (1f < val) return 1f;
      return val;
    }

    public static float AtMost(this float val, float most)
    {
      return (most < val) ? most : val;
    }

    public static float AtLeast(this float val, float least)
    {
      return (val < least) ? least : val;
    }

    public static float SqueezedNaN(this float val)
    {
      if (float.IsNaN(val) || val * val < EPSILON2)
        return 0f;

      return val;
    }


    public static float Squeezed(this float val)
    {
      if (val * val < EPSILON2)
        return 0f;

      return val;
    }

    public static float Squeezed(this float val, float epsilon)
    {
      if (val * val < epsilon * epsilon)
        return 0f;

      return val;
    }


    public static float SmoothSteppedTo(this float from, float to, float t)
    {
      t = SmoothStepParameter(t);
      return from + (to - from) * t;
    }

    public static float LerpedTo(this float from, float to, float t) // identical to Mathf.LerpUnclamped
    {
      return from + (to - from) * t;
    }


    public static T ConvertTo<T>(this float from)
      where T : System.IConvertible
    {
      return (T)((System.IConvertible)from).ToType(typeof(T), System.Globalization.NumberFormatInfo.InvariantInfo);
    }

    public static T ConvertTo<T>(this double from)
      where T : System.IConvertible
    {
      return (T)((System.IConvertible)from).ToType(typeof(T), System.Globalization.NumberFormatInfo.InvariantInfo);
    }


    public static bool TryParse(string str, out float val)
    {
      return float.TryParse(str,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.NumberFormatInfo.CurrentInfo,
                            out val);
    }

    public static bool TryParseInvariant(string str, out float val)
    {
      return float.TryParse(str,
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.NumberFormatInfo.InvariantInfo,
                            out val);
    }

  } // end static class Floats

}
