/*! @file       Tests/Runtime/PrimesSpeed.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Speed Tests:
 *  [ ] Primes.IsPrimeLookup
 *  [ ] Primes.IsPrimeNoLookup
 *  [ ] Primes.Next
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections;
using System.Collections.Generic;

using Stopwatch = System.Diagnostics.Stopwatch;


// ReSharper disable once CheckNamespace
public static class PrimesSpeed
{
  private const int NFRAMES = 60;
  private const int SCALE   = 1000;


  private static IEnumerator DoSpeedTest(System.Func<int,bool> testFunc, string name, float acceptableMS, List<int> data = null)
  {
    // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
    if (data is null)
    {
      data = Primes10K.GetTestValues(SCALE >> 1, SCALE >> 1);
    }

    var stopwatch = new Stopwatch();

    yield return null; // start all speed tests after 1st loop

    int i = NFRAMES;
    while (i --> 0)
    {
      int j = data.Count;

      stopwatch.Start();

      while (j --> 0)
      {
        if (testFunc(data[j]))
        {
          data[j] &= ~1;
        }
        else
        {
          ++data[j];
        }
      }

      stopwatch.Stop();

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    Debug.Log($"Primes.{name}: Average ms per {data.Count} checks = {ms:N1}ms  ({ms / acceptableMS:P0} of budget)");

    Assert.Less(ms, acceptableMS);
  }

  [UnityTest]
  public static IEnumerator IsPrimeLookup()
  {
    return DoSpeedTest(Primes.IsPrimeLookup, nameof(IsPrimeLookup), 3f);
  }

  [UnityTest]
  public static IEnumerator IsPrimeNoLookup()
  {
    return DoSpeedTest(Primes.IsPrimeNoLookup, nameof(IsPrimeNoLookup), 3f);
  }

  [UnityTest]
  public static IEnumerator NearestTo()
  {
    return DoSpeedTest((p) => Primes.NearestTo(p) > p, nameof(NearestTo), 3f);
  }

  [UnityTest]
  public static IEnumerator NextHashableSize()
  {
    return DoSpeedTest((p) => Primes.NextHashableSize(p) > p + 5, nameof(NextHashableSize), 5f);
  }

  [UnityTest]
  public static IEnumerator NextNoLookup()
  {
    return DoSpeedTest((p) => Primes.NextNoLookup(p) > p + 5, nameof(NextNoLookup), 5f);
  }

  [UnityTest]
  public static IEnumerator IsHashtableSize()
  {
    var primes = Primes.HashableSizes;
    var data = new List<int>(primes);

    while (data.Count < SCALE)
    {
      data.Add(primes[Integers.RandomIndex(primes.Count)]);
    }

    return DoSpeedTest(Primes.IsPrime, "IsPrimeLookup(when convenient)", 2f, data);
  }

}
