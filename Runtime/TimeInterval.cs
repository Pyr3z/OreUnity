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
using UnityEngine;


namespace Ore
{
  [Serializable]
  public struct TimeInterval :
    IComparable<TimeInterval>, IEquatable<TimeInterval>,
    IComparable<TimeSpan>, IEquatable<TimeSpan>
  {
    public double Seconds
    {
      get => Ticks * 1e-7;
      set => Ticks = (long)(value / 1e-7);
    }


    [SerializeField]
    public long Ticks;


    public TimeInterval(long ticks)
    {
      Ticks = ticks;
    }

    public static TimeInterval OfSeconds(double s)
    {
      return new TimeInterval((long)(s * 1000 + (s >= 0 ? 0.5 : -0.5)) * 10000);
    }

    public static TimeInterval OfMinutes(double m)
    {
      return new TimeInterval((long)(m * 60000 + (m >= 0 ? 0.5 : -0.5)) * 10000);
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
      return obj is {} && GetHashCode() == obj.GetHashCode();
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
