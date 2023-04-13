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

using System; // rare allowance

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
    public enum Units
      // TODO now that we have this enum, one might be tempted to radically restructure this POD...
    {
      Frames,
      Ticks,
      Milliseconds,
      Seconds,
      Minutes,
      Hours,
      Days,
      Weeks
    }


    public static readonly TimeInterval Zero      = new TimeInterval(0L);
    public static readonly TimeInterval MinValue  = new TimeInterval(long.MinValue);
    public static readonly TimeInterval MaxValue  = new TimeInterval(long.MaxValue);

    public static readonly TimeInterval One       = new TimeInterval(1L);
    public static readonly TimeInterval Epsilon   = new TimeInterval(1000L);

    public static readonly TimeInterval Frame     = new TimeInterval(1L, areFrames: true);
    public static readonly TimeInterval Milli     = new TimeInterval(10000L);
    public static readonly TimeInterval Second    = new TimeInterval(10000000L);
    public static readonly TimeInterval Minute    = new TimeInterval(600000000L);
    public static readonly TimeInterval Hour      = new TimeInterval(36000000000L);
    public static readonly TimeInterval Day       = new TimeInterval(864000000000L);
    public static readonly TimeInterval Week      = new TimeInterval(6048000000000L);

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
      #if UNITY_2020_1_OR_NEWER
      get => !ActiveScene.IsPlaying ? Zero : new TimeInterval(Time.realtimeSinceStartupAsDouble);
      #else
      get => !ActiveScene.IsPlaying ? Zero : new TimeInterval(Time.realtimeSinceStartup);
      #endif
    }


    public const double TICKS2MS  = 1e-4;
    public const double TICKS2SEC = TICKS2MS  / 1000;
    public const double TICKS2MIN = TICKS2SEC / 60;
    public const double TICKS2HR  = TICKS2MIN / 60;
    public const double TICKS2DAY = TICKS2HR  / 24;
    public const double TICKS2WK  = TICKS2DAY / 7;

    // using constant now since Application.targetFrameRate cannot be called in all contexts...
    const double TICKS_PER_FRAME_60FPS = 1.0 / 60 / TICKS2SEC;

    const DateTimeKind ASSUME_DATETIME_KIND = DateTimeKind.Utc;


    public static Units DetectUnits(long ticks)
    {
      if (ticks < 0)
        ticks *= -1;

      // the following thresholds were chosen only *somewhat* arbitrarily

      if (ticks < Milli.Ticks)
        return Units.Ticks;

      if (ticks < OfSeconds(0.5).Ticks)
        return Units.Milliseconds;

      if (ticks < OfMinutes(5).Ticks)
        return Units.Seconds;

      if (ticks < OfHours(4).Ticks)
        return Units.Minutes;

      if (ticks < OfDays(3).Ticks)
        return Units.Hours;

      return Units.Days;
    }

    public static TimeInterval SmolParse([CanBeNull] string str)
    {
      if (str.IsEmpty())
        return default;

      // ReSharper disable once PossibleNullReferenceException
      int i = str.Length;
      bool hasUnit = false;

      while (i -- > 0 && ( str[i] > '9' || str[i] < '.' ))
      {
        hasUnit = true;
      }

      string unitPart = "f";
      if (hasUnit)
      {
        ++ i;

        unitPart = str.Substring(i).Trim();
        str = str.Remove(i);

        if (unitPart.Length == 0)
          unitPart = "f";
      }

      _ = double.TryParse(str, out double d);

      switch (unitPart[0])
      {
     // case 'f':
        default:
          return OfFrames((float)d);
        case 't':
        case 'L':
          return OfTicks((long)d);
        case 'm':
          if (unitPart.Length > 1 && unitPart[1] == 's')
            return OfMillis(d);
          return OfMinutes(d);
        case 's':
          return OfSeconds(d);
        case 'h':
          return OfHours(d);
        case 'd':
          return OfDays(d);
        case 'w':
          return OfDays(d) / 7;
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeInterval OfTicks(long ticks)
    {
      // "middleman" function provided simply for API uniformity.
      // good thing it's aggressively compiled out~
      return new TimeInterval(ticks);
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


    // instance shminstance

    public double Millis
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => WithSystemTicks().Ticks * TICKS2MS;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2MS + (value >= 0 ? 0.5 : -0.5));
        m_AsFrames = false;
      }
    }

    public float FMillis
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(WithSystemTicks().Ticks * TICKS2MS);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2MS + (value >= 0f ? 0.5f : -0.5f));
        m_AsFrames = false;
      }
    }

    public double Seconds
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => WithSystemTicks().Ticks * TICKS2SEC;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2SEC + (value >= 0 ? 0.5 : -0.5));
        m_AsFrames = false;
      }
    }

    public float FSeconds
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(WithSystemTicks().Ticks * TICKS2SEC);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2SEC + (value >= 0f ? 0.5f : -0.5f));
        m_AsFrames = false;
      }
    }

    public double Minutes
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => WithSystemTicks().Ticks * TICKS2MIN;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = (long)(value / TICKS2MIN + (value >= 0 ? 0.5 : -0.5));
    }

    public float FMinutes
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(WithSystemTicks().Ticks * TICKS2MIN);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2MIN + (value >= 0f ? 0.5f : -0.5f));
        m_AsFrames = false;
      }
    }

    public double Hours
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => WithSystemTicks().Ticks * TICKS2HR;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2HR + (value >= 0 ? 0.5 : -0.5));
        m_AsFrames = false;
      }
    }

    public float FHours
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(WithSystemTicks().Ticks * TICKS2HR);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2HR + (value >= 0f ? 0.5f : -0.5f));
        m_AsFrames = false;
      }
    }

    public double Days
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => WithSystemTicks().Ticks * TICKS2DAY;
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2DAY + (value >= 0 ? 0.5 : -0.5));
        m_AsFrames = false;
      }
    }

    public float FDays
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (float)(WithSystemTicks().Ticks * TICKS2DAY);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set
      {
        Ticks = (long)(value / TICKS2DAY + (value >= 0f ? 0.5f : -0.5f));
        m_AsFrames = false;
      }
    }


    public int Frames
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => (int)(m_AsFrames ? Ticks : Ticks / SmoothTicksLastFrame);
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      set => Ticks = m_AsFrames ? value : (int)(value * SmoothTicksLastFrame);
    }

    public bool TicksAreFrames
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => m_AsFrames;
    }


    [SerializeField]
    public long Ticks;

    [SerializeField]
    private /* readonly */ bool m_AsFrames;
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
      if (dateTime.Kind != ASSUME_DATETIME_KIND)
      {
        #pragma warning disable CS0162
        // ReSharper disable HeuristicUnreachableCode
        switch (ASSUME_DATETIME_KIND)
        {
          case DateTimeKind.Utc:
            dateTime = dateTime.ToUniversalTime();
            break;
          case DateTimeKind.Local:
            dateTime = dateTime.ToLocalTime();
            break;
        }
        // ReSharper restore HeuristicUnreachableCode
        #pragma warning restore CS0162
      }

      Ticks      = dateTime.Ticks;
      m_AsFrames = false;
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ToUnixTime()
    {
      return this >= Epoch ? (this - Epoch).Millis.Rounded() : Millis.Rounded();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public DateTime ToDateTime(DateTimeKind kind = ASSUME_DATETIME_KIND)
    {
      return new DateTime(WithSystemTicks(), kind);
    }


    public double ToUnits(Units units)
    {
      switch (units)
      {
        default:
        case Units.Ticks:
          return Ticks;
        case Units.Frames:
          return Frames;
        case Units.Milliseconds:
          return Millis;
        case Units.Seconds:
          return Seconds;
        case Units.Minutes:
          return Minutes;
        case Units.Hours:
          return Hours;
        case Units.Days:
          return Days;
        case Units.Weeks:
          return Days / 7;
      }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeInterval WithSystemTicks()
    {
      return !m_AsFrames ? this : new TimeInterval((Ticks * SmoothTicksLastFrame).Rounded(), areFrames: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TimeInterval WithFrameTicks()
    {
      return m_AsFrames ? this : new TimeInterval((Ticks / SmoothTicksLastFrame).Rounded(), areFrames: true);
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


    /// <summary>
    ///   Modifies this TimeInterval to represent the nearest multiple of the
    ///   given step.
    /// </summary>
    /// <param name="step">
    ///   If the step is <see cref="Zero"/>, the interval will be zeroed out. <br/>
    ///   If the step is negative, its absolute value will be used.
    /// </param>
    /// <returns>
    ///   <c>this</c>.
    /// </returns>
    public TimeInterval RoundToInterval(TimeInterval step)
    {
      long i;
      if (m_AsFrames)
      {
        i = step.WithFrameTicks().Ticks;
      }
      else if (step.m_AsFrames)
      {
        i = step.WithSystemTicks().Ticks;
      }
      else
      {
        i = step.Ticks;
      }

      if (i == 0L)
      {
        Ticks = 0L;
        return this;
      }

      if (i < 0L)
      {
        i *= -1L;
      }

      long r = Ticks % i;
      if (r < i >> 1)
        Ticks -= r;       // round down
      else
        Ticks += (i - r); // round up

      return this;
    }


    /// <summary>
    ///   Get a "wait" object (null => a single frame or less) that can be
    ///   <c>yield return</c>ed by a Unity coroutine, causing the coroutine to
    ///   wait for the time interval represented by this struct.
    /// </summary>
    /// <param name="scaledTime">
    ///   If specified and true, <see cref="Time.timeScale"/> will be ignored in
    ///   the resulting wait object.
    /// </param>
    [CanBeNull]
    public object Yield(bool scaledTime = false)
    {
      if (Ticks <= 1)
      {
        return null;
      }

      if (m_AsFrames)
      {
        return new WaitForFrames((int)Ticks);
      }

      if (scaledTime)
      {
        return new WaitForSeconds(FSeconds);
      }

      return new WaitForSecondsRealtime(FSeconds);
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
      var units = m_AsFrames ? Units.Frames : DetectUnits(Ticks);
      return ToString(units);
    }

    public string ToString(Units units, string decimalFmt = "F1", IFormatProvider provider = null)
    {
      if (provider is null)
        provider = Strings.InvariantFormatter;

      using (new RecycledStringBuilder(out var bob))
      {
        switch (units)
        {
          default:
          case Units.Ticks:
            bob.Append(WithSystemTicks().Ticks.ToString(provider));
            bob.Append('t');
            break;
          case Units.Frames:
            bob.Append(Frames.ToString(provider));
            bob.Append('f');
            break;
          case Units.Milliseconds:
            bob.Append(Millis.ToString(decimalFmt, provider));
            bob.Append("ms");
            break;
          case Units.Seconds:
            bob.Append(Seconds.ToString(decimalFmt, provider));
            bob.Append('s');
            break;
          case Units.Minutes:
            bob.Append(Minutes.ToString(decimalFmt, provider));
            bob.Append('m');
            break;
          case Units.Hours:
            bob.Append(Hours.ToString(decimalFmt, provider));
            bob.Append('h');
            break;
          case Units.Days:
            bob.Append(Days.ToString(decimalFmt, provider));
            bob.Append('d');
            break;
          case Units.Weeks:
            bob.Append((Days / 7).ToString(decimalFmt, provider));
            bob.Append('w');
            break;
        }

        return bob.ToString();
      }
    }


    // operators

    // explicit casts = assume the caller knows what t represents

    public static explicit operator TimeSpan (TimeInterval t)
    {
      return new TimeSpan(t.Ticks);
    }

    public static explicit operator DateTime (TimeInterval t)
    {
      return new DateTime(t.Ticks, ASSUME_DATETIME_KIND);
    }

    public static explicit operator TimeInterval (DateTime timepoint)
    {
      return new TimeInterval(timepoint);
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

    public static TimeInterval operator * (int lhs, TimeInterval rhs)
    {
      rhs.Ticks *= lhs;
      return rhs;
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

    public static TimeInterval operator * (double lhs, TimeInterval rhs)
    {
      rhs.Ticks = (long)(rhs.Ticks * lhs);
      return rhs;
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

    public static DateTime operator + (DateTime lhs, TimeInterval rhs)
    {
      return lhs.AddTicks(rhs.WithSystemTicks().Ticks);
    }

    public static DateTime operator - (DateTime lhs, TimeInterval rhs)
    {
      return lhs.AddTicks(rhs.WithSystemTicks().Ticks * -1);
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
