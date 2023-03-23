/*! @file       Runtime/TimeIntervalsInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-22
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;


// ReSharper disable once CheckNamespace
internal static class TimeIntervalsInEditor
{

  [Test]
  public static void LazySmolParse()
  {
    (string test, TimeInterval expected)[] tests =
    {
      ("1337 ticks", TimeInterval.OfTicks(1337)),
      ("22ms",       TimeInterval.OfMillis(22)),
      ("3.14s",      TimeInterval.OfSeconds(3.14)),
      ("5.0m",       TimeInterval.OfMinutes(5)),
      ("0.50 hr ",   TimeInterval.OfHours(0.5)),
      ("1 day",      TimeInterval.OfDays(1)),
      ("1.5days",    TimeInterval.OfDays(1.5)),
    };

    foreach (var (test, expected) in tests)
    {
      var actual = TimeInterval.SmolParse(test);
      AssertAreEqual(expected, actual, $"SmolParse(\"{test}\")");
    }
  }


  [Test]
  public static void RoundToInterval()
  {
    var ti = TimeInterval.OfSeconds(3.14);
    Assert.AreEqual(3.14, ti.Seconds, Floats.Epsilon);

    AssertAreEqual(TimeInterval.OfSeconds(3), 
                   ti.RoundToInterval(TimeInterval.Second),
                    "rounding 3.14s");

    ti = TimeInterval.OfHours(23.6);
    Assert.AreEqual(23.6, ti.Hours, Floats.Epsilon);

    AssertAreEqual(TimeInterval.OfHours(24),
                   ti.RoundToInterval(TimeInterval.Hour),
                    "rounding 23.6h");
  }


  static void AssertAreEqual(TimeInterval expected, TimeInterval actual, string msg = null)
  {
    var units = TimeInterval.DetectUnits(expected.Ticks.AtMost(actual.Ticks));

    // do this so that assertion messages are more readable
    Assert.AreEqual(expected.ToUnits(units), actual.ToUnits(units), Floats.Epsilon, msg);
  }

} // end class TimeIntervalsInEditor
