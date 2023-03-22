/*! @file       Tests/Runtime/DelayedRoutineTests.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-15
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections;


// ReSharper disable once CheckNamespace
public static class DelayedRoutineTests
{

  [UnityTest]
  public static IEnumerator DelayByFrame([Values(0, 1, 10, 100, 333, 1234)] int frames)
  {
    int start = Time.frameCount;

    void action()
    {
      Assert.AreEqual(1 + start + frames, Time.frameCount, $"frame # after delay of {frames}");
    }

    var delay = TimeInterval.OfFrames(frames);

    return new DelayedRoutine(action, delay);
  }

  [UnityTest]
  public static IEnumerator DelayBySeconds([Values(0f, 1f, 3.14f, 5f)] float seconds)
  {
    float start = Time.realtimeSinceStartup;

    void action()
    {
      Assert.AreEqual(start + seconds, Time.realtimeSinceStartup, TimeInterval.LastFrame, $"time after delay of {seconds}");
    }

    var delay = TimeInterval.OfSeconds(seconds);

    return new DelayedRoutine(action, delay);
  }

} // end static class DelayedRoutineTests
