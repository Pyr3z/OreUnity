/*! @file       Static/Floats.cs
 *  @author     levianperez\@gmail.com
 *  @author     levi\@leviperez.dev
 *  @date       2020-06-06
 *
 *  @brief
 *    Utilities for native IEEE floating-point primitives.
**/

using JetBrains.Annotations;
using Math = System.Math;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


namespace Ore
{
  /// <summary>
  /// Utilities for IEEE floating-point primitives.
  /// </summary>
  [PublicAPI]
  public static class Floats
  {

  #region Handy constants

    // my chosen epsilon that is sensitive enough for most game applications,
    // with mitigated IEEE aggregations error:
    public const float Epsilon  = 1e-6f; // (Unity tends to use 1e-5f, i.e. Vector3.kEpsilon)
    public const float Epsilon2 = Epsilon * Epsilon + float.Epsilon;

    public const float DefaultPValue = 0.05f; // standard p-value commonly used in statsig analysis

    public const float Sqrt2   = 1.414213562373095f; // precomputed because sqrt(2) is used bloody everywhere.
    public const float MaxSqrt = 1.844674352e+19f;   // precomputed 32-bit IEEE max squarable value

    public const float Pi      = 3.1415926535897931f;
    public const float InvPi   = 1f / Pi;
    public const float PiPi    = 2 * Pi;
    public const float InvPiPi = 1f / PiPi;


    public const string RoundTripFormat       = "G9";
    public const string DoubleRoundTripFormat = "G17";

  #endregion Handy constants

  #region Public methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SinusoidalParameter(float t)
    {
      // "stretches" the normalized value `t` so that 1.0 corresponds to 2 * Pi,
      // then uses cosine to produce an oscillating value between 0.0 and 1.0
      return (((float)Math.Cos(t * PiPi) - 1f) * 0.5f).Clamp01();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InverseSinusoidalParameter(float t)
    {
      return (float)Math.Acos(-2f * t + 1f) * InvPiPi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SmoothStepParameter(float t)
    {
      return t < Epsilon ? 0f : 1f < t + Epsilon ? 1f : t * t * (3f - 2f * t);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SinusoidalLoop(float a, float b, float t)
    {
      return Lerped(a, b, SinusoidalParameter(t));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ErrorPercent(float actual, float expected)
    {
      return expected == 0f ? Abs(actual) : Abs((actual - expected) / expected);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ErrorPercent(double actual, double expected)
    {
      return (float)(expected == 0.0 ? Math.Abs(actual) : Math.Abs((actual - expected) / expected));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float MakeFinite(float val, float finite)
    {
      return float.IsNaN(val) ? finite : float.IsInfinity(val) ? Sign(val) * finite : val;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float ClampSquarable(float val)
    {
      return float.IsNaN(val) || !float.IsPositiveInfinity(val * val) ? val : Sign(val) * MaxSqrt;
    }

  #endregion Public methods

  #region Extension methods

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(this float a, float b)
    {
      // TODO probably can't aggressively inline because parameters are modified?
      return (a -= b) * a < Epsilon2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Approximately(this float a, float b, float epsilon)
    {
      return (a -= b) * a < epsilon * epsilon;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ApproximatelyZero(this float val)
    {
      return val * val < Epsilon2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ApproximatelyZero(this float val, float epsilon)
    {
      return val * val < epsilon * epsilon;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Relatively(this float a, float b, float errorPct = DefaultPValue)
    {
      return ErrorPercent(a, b) <= errorPct;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    #if ENABLE_UNSAFE
    public static unsafe bool IsNegativeZero(this float val)
    {
      return (*(int*)(&val) & 0x80000000) == 0x80000000;
    }
    #else
    public static bool IsNegativeZero(this float val)
    {
      return val == 0f && System.BitConverter.GetBytes(val)[sizeof(float) - 1] == 0x80;
    }
    #endif // ENABLE_UNSAFE


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNaN(this float val)
    {
      return float.IsNaN(val);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsFinite(this float val)
    {
      return !float.IsNaN(val) && !float.IsInfinity(val);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsSquarable(this float val)
    {
      return !float.IsNaN(val) && !float.IsPositiveInfinity(val * val);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsExtreme(this float val)
    {
      // ReSharper disable once CompareOfFloatsByEqualityOperator
      return Abs(val) == float.MaxValue;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NoNaN(this float val)
    {
      return float.IsNaN(val) ? 0f : val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float NoNaN(this float val, float fallback)
    {
      return float.IsNaN(val) ? fallback : val;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Squeezed(this float val)
    {
      return val * val < Epsilon2 ? 0f : val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Squeezed(this float val, float epsilon)
    {
      return val * val < epsilon * epsilon ? 0f : val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SqueezedNoNaN(this float val)
    {
      return float.IsNaN(val) || val * val < Epsilon2 ? 0f : val;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Abs(this float val)
    {
      return Math.Abs(val);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sign(this float val)
    {
      return Epsilon < val ? +1f : val < -Epsilon ? -1f : 0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignOrNaN(this float val)
    {
      return Epsilon < val ? +1f : val < -Epsilon ? -1f : float.IsNaN(val) ? float.NaN : 0f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SignNoZero(this float val)
    {
      return val < 0f ? -1f : 1f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sqrt(this float val)
    {
      return (float)Math.Sqrt(val);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Rounded(this float val)
    {
      return (int)(val + (val < 0 ? -1 : 1) * 0.5f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long Rounded(this double val)
    {
      return (long)(val + (val < 0 ? -1 : 1) * 0.5);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp(this float val, float min, float max)
    {
      if (max < min)
      {
        (min,max) = (max,min); // TODO test if this swap syntax breaks after inlined
      }

      return val < min ? min : max < val ? max : val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Clamp01(this float val)
    {
      return val < Epsilon ? 0f : 1f < val + Epsilon ? 1f : val;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AtMost(this float val, float most)
    {
      return most < val ? most : val;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float AtLeast(this float val, float least)
    {
      return val < least ? least : val;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float SmoothStepped(this float from, float to, float t)
    {
      return from + (to - from) * SmoothStepParameter(t);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Lerped(this float from, float to, float t) // identical to Mathf.LerpUnclamped
    {
      return from + (to - from) * t;
    }

  #endregion Extension methods

  } // end static class Floats

}
