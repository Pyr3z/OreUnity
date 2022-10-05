/*! @file       Tests/Runtime/HashMapFootprint.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Footprint Tests:
 *  [ ] HashMap vs Dictionary
 *  [ ] HashMap vs Hashtable
 *  [ ] HashMap.Clear vs HashMap.ClearNoAlloc
**/

using NUnit.Framework;

using UnityEngine.TestTools;
using UnityEngine.Profiling;

using System.Collections;
using UnityEngine;


namespace Ore.Tests
{
  public static class HashMapFootprint
  {

    [UnityTest]
    public static IEnumerator Dictionary()
    {
      Assert.Inconclusive("test not implemented.");
      yield break;
    }

    [UnityTest]
    public static IEnumerator Hashtable()
    {
      Assert.Inconclusive("test not implemented.");
      yield break;
    }

    [UnityTest]
    public static IEnumerator HashMap()
    {
      Assert.Inconclusive("test not implemented.");
      yield break;
    }

    [UnityTest]
    public static IEnumerator HashMapClear()
    {
      return DoHashMapClear(map => map.Clear());
    }

    [UnityTest]
    public static IEnumerator HashMapClearNoAlloc()
    {
      return DoHashMapClear(map => map.ClearNoAlloc());
    }

    private static IEnumerator DoHashMapClear(System.Action<HashMap<string,string>> doClear)
    {
      System.GC.Collect();

      long footprint = Profiler.GetMonoUsedSizeLong();
      var hashmap = HashMapSpeed.GetTestHashMap(1024*1024);

      footprint = Profiler.GetMonoUsedSizeLong() - footprint;

      Debug.Log($"\"HashMap\",\"{hashmap.Count}\",\"{(double)footprint/1000000:N1}\"");

      yield return null;

      System.GC.Collect();

      footprint = Profiler.GetMonoUsedSizeLong();

      doClear(hashmap);

      footprint = Profiler.GetMonoUsedSizeLong() - footprint;

      Debug.Log($"\"HashMap\",\"{hashmap.Count}\",\"{(double)footprint/1000000:N1}\"");
    }

  }
}