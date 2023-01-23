/*! @file       Runtime/TimeInterval.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-08
 *
 *  @remarks
 *    All the copying of the System.TimeSpan interface is necessary.
 *      (1) a new type was needed for custom tooling.
 *      (2) a new type was needed for direct Unity serializability.
**/

using JetBrains.Annotations;

using UnityEngine;

using System;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


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
    public static readonly TimeInterval Epsilon   = new TimeInterval(1000L);

    public static readonly TimeInterval Frame     = new TimeInterval(1L, areFrames: true);
    public static readonly TimeInterval Second    = new TimeInterval(10000000L);

    public static readonly TimeInterval Epoch     = new TimeInterval(DateTimes.Epoch);


    public static double SmoothTicksLastFrame
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ActiveScene.IsPlaying ? Time.smoothDeltaTime / TICKS2SEC : TICKS_PER_FRAME_60FPS;
    }

    public static double TicksLastFrame
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => ActiveScene.IsPlaying ? Time.unscaledDeltaTime / TICKS2SEC : TICKS_PER_FRAME_60FPS;
    }

    public static TimeInterval LastFrame
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new TimeInterval(SmoothTicksLastFrame);
    }

    public static TimeInterval ThisSession
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => !ActiveScene.IsPlaying ? Zero : new TimeInterval(Time.realtimeSinceStartupAsDouble);
    }

    public static TimeInterval UtcNow
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => new TimeInterval(DateTime.UtcNow);
    }


    public const double TICKS2MS  = 1e-4;
    public const double TICKS2SEC = TICKS2MS  / 1000;
    public const double TICKS2MIN = TICKS2SEC / 60;
    public const double TICKS2HR  = TICKS2MIN / 60;
    public const double TICKS2DAY = TICKS2HR  / 24;

    // using constant now since Application.targetFrameRate cannot be called in all contexts...
    private const long TICKS_PER_FRAME_60FPS = (long)(1.0 / 60 / TICKS2SEC);


    //
    // TODO none of the following properties (up until `Frames`) work when m_AsFrames=true
    //

    public double Millis
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Ticks * TICKS2MS;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2MS + (value >= 0 ? 0.5 : -0.5));
    }

    public float FMillis
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(Ticks * TICKS2MS);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2MS + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Seconds
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Ticks * TICKS2SEC;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2SEC + (value >= 0 ? 0.5 : -0.5));
    }

    public float FSeconds
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(Ticks * TICKS2SEC);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2SEC + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Minutes
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Ticks * TICKS2MIN;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2MIN + (value >= 0 ? 0.5 : -0.5));
    }

    public float FMinutes
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(Ticks * TICKS2MIN);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2MIN + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Hours
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Ticks * TICKS2HR;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2HR + (value >= 0 ? 0.5 : -0.5));
    }

    public float FHours
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(Ticks * TICKS2HR);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2HR + (value >= 0f ? 0.5f : -0.5f));
    }

    public double Days
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => Ticks * TICKS2DAY;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2DAY + (value >= 0 ? 0.5 : -0.5));
    }

    public float FDays
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(Ticks * TICKS2DAY);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2DAY + (value >= 0f ? 0.5f : -0.5f));
    }


    public int Frames
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)(m_AsFrames ? Ticks : Ticks / SmoothTicksLastFrame);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = m_AsFrames ? value : (int)(value * SmoothTicksLastFrame);
    }


    [SerializeField]
    public long Ticks;

    [SerializeField]
    private bool m_AsFrames;
      // I've matured away from using bitflags for this sort of thing.
      // ... perhaps only for today ...


    public TimeInterval(long ticks, bool areFrames = false)
    {
      Ticks      = ticks;
      m_AsFrames = areFrames;
    }

    public TimeInterval(double seconds)
    {
      Ticks      = (long)(seconds / TICKS2SEC);
      m_AsFrames = false;
    }

    public TimeInterval(TimeSpan timeSpan)
    {
      Ticks      = timeSpan.Ticks;
      m_AsFrames = false;
    }

    public TimeInterval(DateTime dateTime)
    {
      Ticks      = dateTime.Ticks;
      m_AsFrames = false;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfMillis(double ms)
    {
      return new TimeInterval((long)(ms / TICKS2MS + (ms >= 0 ? 0.5 : -0.5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfSeconds(double s)
    {
      return new TimeInterval((long)(s / TICKS2SEC + (s >= 0 ? 0.5 : -0.5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfMinutes(double m)
    {
      return new TimeInterval((long)(m / TICKS2MIN + (m >= 0 ? 0.5 : -0.5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfHours(double h)
    {
      return new TimeInterval((long)(h / TICKS2HR + (h >= 0 ? 0.5 : -0.5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfDays(double d)
    {
      return new TimeInterval((long)(d / TICKS2DAY + (d >= 0 ? 0.5 : -0.5)));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfFrames(int nFrames)
    {
      return new TimeInterval(nFrames, areFrames: true);
    }

    public static TimeInterval OfFrames(float qFrames)
    {
      int rounded = qFrames.Rounded();
      if (qFrames.Approximately(rounded))
      {
        return new TimeInterval(rounded, areFrames: true);
      }
      else
      {
        return new TimeInterval((long)(SmoothTicksLastFrame * qFrames));
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ToUnixTime()
    {
      return this >= Epoch ? (this - Epoch).Millis.Rounded() : Millis.Rounded();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTime ToDateTime(bool utc = true)
    {
      return new DateTime(WithSystemTicks(), utc ? DateTimeKind.Utc : DateTimeKind.Local);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeInterval WithSystemTicks()
    {
      return !m_AsFrames ? this : new TimeInterval((long)(Ticks * SmoothTicksLastFrame), areFrames: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeInterval WithFrameTicks()
    {
      return m_AsFrames ? this : new TimeInterval((long)(Ticks / SmoothTicksLastFrame), areFrames: true);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncrementFrame(int n = 1)
    {
      Ticks += m_AsFrames ? n : (long)(n * (n * n == 1 ? TicksLastFrame : SmoothTicksLastFrame));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DecrementFrame(int n = 1)
    {
      IncrementFrame(-1 * n);
    }


    // implement interfaces

    public int CompareTo(TimeInterval other)
    {
      if (m_AsFrames != other.m_AsFrames)
      {
        return (int)(WithSystemTicks().Ticks - other.WithSystemTicks().Ticks);
      }

      return (int)(Ticks - other.Ticks);
    }

    int IComparable<TimeSpan>.CompareTo(TimeSpan other)
    {
      if (m_AsFrames)
      {
        return (int)(WithSystemTicks().Ticks - other.Ticks);
      }

      return (int)(Ticks - other.Ticks);
    }

    public bool Equals(TimeInterval other)
    {
      return Ticks == other.Ticks && m_AsFrames == other.m_AsFrames;
    }

    bool IEquatable<TimeSpan>.Equals(TimeSpan other)
    {
      if (m_AsFrames)
      {
        return WithSystemTicks().Ticks.IsRelatively(other.Ticks, errorPct: 0.01f);
      }

      return Ticks == other.Ticks;
    }

    public override bool Equals(object obj)
    {
      return !(obj is null) && GetHashCode() == obj.GetHashCode();
    }

    public override int GetHashCode()
    {
      return (int)Hashing.MixHashes(m_AsFrames.ToInt(), (int)Ticks, (int)(Ticks >> 32));
    }

    public override string ToString()
    {
      return Ticks.ToInvariant();
    }


    // operators

    public static implicit operator TimeSpan (TimeInterval t)
    {
      return new TimeSpan(t.WithSystemTicks().Ticks);
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

    public static implicit operator long (TimeInterval t)
    {
      return t.WithSystemTicks().Ticks;
    }

    public static implicit operator double (TimeInterval t)
    {
      return t.WithSystemTicks().Seconds;
    }


    public static TimeInterval operator * (TimeInterval lhs, int rhs)
    {
      lhs.Ticks *= rhs;
      return lhs;
    }

    public static TimeInterval operator / (TimeInterval lhs, int rhs)
    {
      if (rhs == 0)
        return MaxValue;

      lhs.Ticks /= rhs;
      return lhs;
    }

    public static TimeInterval operator * (TimeInterval lhs, double rhs)
    {
      lhs.Ticks = (long)(lhs.Ticks * rhs);
      return lhs;
    }

    public static TimeInterval operator / (TimeInterval lhs, double rhs)
    {
      if (rhs == 0.0)
        return MaxValue;

      lhs.Ticks = (long)(lhs.Ticks / rhs);
      return lhs;
    }

    public static TimeInterval operator + (TimeInterval lhs, TimeInterval rhs)
    {
      if (lhs.m_AsFrames != rhs.m_AsFrames)
      {
        // policy = always promote to system tick quanta
        lhs = lhs.WithSystemTicks();
        rhs = rhs.WithSystemTicks();
      }

      lhs.Ticks += rhs.Ticks;
      return lhs;
    }

    public static TimeInterval operator - (TimeInterval lhs, TimeInterval rhs)
    {
      if (lhs.m_AsFrames != rhs.m_AsFrames)
      {
        // policy = always promote to system tick quanta
        lhs = lhs.WithSystemTicks();
        rhs = rhs.WithSystemTicks();
      }

      lhs.Ticks -= rhs.Ticks;
      return lhs;
    }


    public static TimeInterval operator - (TimeInterval self)
    {
      self.Ticks *= -1;
      return self;
    }


    public static bool operator < (TimeInterval lhs, TimeInterval rhs)
    {
      if (lhs.m_AsFrames != rhs.m_AsFrames)
      {
        return lhs.WithSystemTicks().Ticks < rhs.WithSystemTicks().Ticks;
      }

      return lhs.Ticks < rhs.Ticks;
    }

    public static bool operator > (TimeInterval lhs, TimeInterval rhs)
    {
      if (lhs.m_AsFrames != rhs.m_AsFrames)
      {
        return rhs.WithSystemTicks().Ticks < lhs.WithSystemTicks().Ticks;
      }

      return rhs.Ticks < lhs.Ticks;
    }

    public static bool operator <= (TimeInterval lhs, TimeInterval rhs)
    {
      if (lhs.m_AsFrames != rhs.m_AsFrames)
      {
        return lhs.WithSystemTicks().Ticks <= rhs.WithSystemTicks().Ticks;
      }

      return lhs.Ticks <= rhs.Ticks;
    }

    public static bool operator >= (TimeInterval lhs, TimeInterval rhs)
    {
      if (lhs.m_AsFrames != rhs.m_AsFrames)
      {
        return rhs.WithSystemTicks().Ticks <= lhs.WithSystemTicks().Ticks;
      }

      return rhs.Ticks <= lhs.Ticks;
    }

    public static bool operator == (TimeInterval lhs, TimeInterval rhs)
    {
      return lhs.Ticks == rhs.Ticks && lhs.m_AsFrames == rhs.m_AsFrames;
    }

    public static bool operator != (TimeInterval lhs, TimeInterval rhs)
    {
      return lhs.Ticks != rhs.Ticks || lhs.m_AsFrames != rhs.m_AsFrames;
    }

  }
}
