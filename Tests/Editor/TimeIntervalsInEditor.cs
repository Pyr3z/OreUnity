/*! @file       Runtime/TimeIntervalsInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-22
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;


// ReSharper disable once CheckNamespace
internal class TimeIntervalsInEditor
{

  [Test]
  public static void RoundToInterval()
  {
    var ti = TimeInterval.OfSeconds(3.14f);

    Assert.AreEqual(TimeInterval.OfSeconds(3), ti.RoundToInterval(TimeInterval.Second));

    ti = TimeInterval.OfHours(23.6);

    Assert.AreEqual(TimeInterval.OfHours(24), ti.RoundToInterval(TimeInterval.Hour));
  }

} // end class TimeIntervalsInEditor
