/*! @file       Runtime/TimeInterval.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-08
 *
 *  @remarks
 *    All the copying of the System.TimeSpan interface is necessary.
 *      (1) a new type was needed for custom tooling.
 *      (2) a new type was needed for direct Unity serializability.
**/

using System;
using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  [Serializable]
  [PublicAPI]
  public struct TimeInterval :
    IComparable<TimeInterval>, IEquatable<TimeInterval>,
    IComparable<TimeSpan>, IEquatable<TimeSpan>
  {
    public static readonly TimeInterval Zero      = new TimeInterval(0L);
    public static readonly TimeInterval MinValue  = new TimeInterval(long.MinValue);
    public static readonly TimeInterval MaxValue  = new TimeInterval(long.MaxValue);

    public static readonly TimeInterval One       = new TimeInterval(1L);
    public static readonly TimeInterval Epsilon   = new TimeInterval(11L);

    public static readonly TimeInterval Frame     = new TimeInterval(TICKS_PER_FRAME);
    public static readonly TimeInterval HalfFrame = new TimeInterval(TICKS_PER_FRAME / 2);


    public const double TICKS2MS  = 1e-4;
    public const double TICKS2SEC = TICKS2MS  / 1000;
    public const double TICKS2MIN = TICKS2SEC / 60;
    public const double TICKS2HR  = TICKS2MIN / 60;
    public const double TICKS2DAY = TICKS2HR  / 24;

    // using constant now since Application.targetFrameRate cannot be called in all contexts...
    private const long TICKS_PER_FRAME = (long)(1.0 / 60 / TICKS2SEC);


    public double Millis
    {
      get => Ticks * TICKS2MS;
      set => Ticks = (long)(value / TICKS2MS + (value >= 0 ? 0.5 : -0.5));
    }

    public float FMillis
    {
      get => (float)(Ticks * TICKS2MS);
      set => Ticks = (long)(value / TICKS2MS + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Seconds
    {
      get => Ticks * TICKS2SEC;
      set => Ticks = (long)(value / TICKS2SEC + (value >= 0 ? 0.5 : -0.5));
    }

    public float FSeconds
    {
      get => (float)(Ticks * TICKS2SEC);
      set => Ticks = (long)(value / TICKS2SEC + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Minutes
    {
      get => Ticks * TICKS2MIN;
      set => Ticks = (long)(value / TICKS2MIN + (value >= 0 ? 0.5 : -0.5));
    }

    public float FMinutes
    {
      get => (float)(Ticks * TICKS2MIN);
      set => Ticks = (long)(value / TICKS2MIN + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Hours
    {
      get => Ticks * TICKS2HR;
      set => Ticks = (long)(value / TICKS2HR + (value >= 0 ? 0.5 : -0.5));
    }

    public float FHours
    {
      get => (float)(Ticks * TICKS2HR);
      set => Ticks = (long)(value / TICKS2HR + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Days
    {
      get => Ticks * TICKS2DAY;
      set => Ticks = (long)(value / TICKS2DAY + (value >= 0 ? 0.5 : -0.5));
    }

    public float FDays
    {
      get => (float)(Ticks * TICKS2DAY);
      set => Ticks = (long)(value / TICKS2DAY + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Frames
    {
      get => (double)Ticks / TICKS_PER_FRAME;
      set => Ticks = (long)(value * TICKS_PER_FRAME + (value > 0 ? 0.5 : -0.5));
    }

    public float FFrames
    {
      get => (float)Ticks / TICKS_PER_FRAME;
      set => Ticks = (long)(value * TICKS_PER_FRAME + (value > 0f ? 0.5f : -0.5f));
    }


    [SerializeField]
    public long Ticks;


    public TimeInterval(long ticks)
    {
      Ticks = ticks;
    }

    public static TimeInterval OfMillis(double ms)
    {
      return new TimeInterval((long)(ms / TICKS2MS + (ms >= 0 ? 0.5 : -0.5)));
    }

    public static TimeInterval OfSeconds(double s)
    {
      return new TimeInterval((long)(s / TICKS2SEC + (s >= 0 ? 0.5 : -0.5)));
    }

    public static TimeInterval OfMinutes(double m)
    {
      return new TimeInterval((long)(m / TICKS2MIN + (m >= 0 ? 0.5 : -0.5)));
    }

    public static TimeInterval OfHours(double h)
    {
      return new TimeInterval((long)(h / TICKS2HR + (h >= 0 ? 0.5 : -0.5)));
    }

    public static TimeInterval OfDays(double d)
    {
      return new TimeInterval((long)(d / TICKS2DAY + (d >= 0 ? 0.5 : -0.5)));
    }

    public static TimeInterval OfFrames(int nFrames)
    {
      return new TimeInterval(TICKS_PER_FRAME * nFrames);
    }

    public static TimeInterval OfFrames(double qFrames)
    {
      return new TimeInterval((long)(TICKS_PER_FRAME * qFrames));
    }


    public void AddSeconds(double s)
    {
      Ticks += (long)(s / TICKS2SEC + 0.5);
    }

    public void AddSeconds(float s)
    {
      Ticks += (long)(s / TICKS2SEC + 0.5f);
    }

    public void SubtractSeconds(double s)
    {
      Ticks -= (long)(s / TICKS2SEC + 0.5);
    }

    public void SubtractSeconds(float s)
    {
      Ticks -= (long)(s / TICKS2SEC + 0.5f);
    }


    public int CompareTo(TimeInterval other)
    {
      return (int)(Ticks - other.Ticks);
    }

    int IComparable<TimeSpan>.CompareTo(TimeSpan other)
    {
      return (int)(Ticks - other.Ticks);
    }

    public bool Equals(TimeInterval other)
    {
      return Ticks == other.Ticks;
    }

    bool IEquatable<TimeSpan>.Equals(TimeSpan other)
    {
      return Ticks == other.Ticks;
    }

    public override bool Equals(object obj)
    {
      return !(obj is null) && GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
      return (int)Ticks ^ (int)(Ticks >> 32);
    }

    public override string ToString()
    {
      return Ticks.ToInvariant();
    }



    public static implicit operator TimeSpan (TimeInterval t)
    {
      return new TimeSpan(t.Ticks);
    }

    public static implicit operator TimeInterval (TimeSpan tspan)
    {
      return new TimeInterval(tspan.Ticks);
    }

    public static implicit operator TimeInterval (long ticks)
    {
      return new TimeInterval(ticks);
    }

    public static implicit operator TimeInterval (double seconds)
    {
      return OfSeconds(seconds);
    }


    public static TimeInterval operator * (TimeInterval lhs, int rhs)
    {
      return new TimeInterval(lhs.Ticks * rhs);
    }

    public static TimeInterval operator / (TimeInterval lhs, int rhs)
    {
      return new TimeInterval(lhs.Ticks / rhs);
    }

    public static TimeInterval operator * (TimeInterval lhs, double rhs)
    {
      return new TimeInterval((long)Math.Round(lhs.Ticks * rhs));
    }

    public static TimeInterval operator / (TimeInterval lhs, double rhs)
    {
      return new TimeInterval((long)Math.Round(lhs.Ticks / rhs));
    }

    public static TimeInterval operator + (TimeInterval lhs, TimeInterval rhs)
    {
      return new TimeInterval(lhs.Ticks + rhs.Ticks);
    }

    public static TimeInterval operator - (TimeInterval lhs, TimeInterval rhs)
    {
      return new TimeInterval(lhs.Ticks - rhs.Ticks);
    }


    public static TimeInterval operator - (TimeInterval self)
    {
      return new TimeInterval(self.Ticks * -1);
    }


    public static bool operator < (TimeInterval lhs, TimeInterval rhs)
    {
      return lhs.Ticks < rhs.Ticks;
    }

    public static bool operator > (TimeInterval lhs, TimeInterval rhs)
    {
      return rhs.Ticks < lhs.Ticks;
    }

    public static bool operator <= (TimeInterval lhs, TimeInterval rhs)
    {
      return lhs.Ticks <= rhs.Ticks;
    }

    public static bool operator >= (TimeInterval lhs, TimeInterval rhs)
    {
      return rhs.Ticks <= lhs.Ticks;
    }

    public static bool operator == (TimeInterval lhs, TimeInterval rhs)
    {
      return lhs.Ticks == rhs.Ticks;
    }

    public static bool operator != (TimeInterval lhs, TimeInterval rhs)
    {
      return lhs.Ticks != rhs.Ticks;
    }

  }
}
