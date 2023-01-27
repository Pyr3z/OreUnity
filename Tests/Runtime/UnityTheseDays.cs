/*! @file       Tests/Runtime/UnityTheseDays.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-27
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Profiling;


public static class UnityTheseDays
{

  [Test]
  public static void AllocatesThisMuchForGameObject()
  {
    System.GC.Collect();

    long managed = Profiler.GetMonoUsedSizeLong();

    var go = new GameObject("Empty");

    managed = Profiler.GetMonoUsedSizeLong() - managed;
    Debug.Log($"managed mem: {managed} bytes");

    Assert.Positive(managed, "managed memory usage");

    long native = Profiler.GetRuntimeMemorySizeLong(go);
    Debug.Log($"native mem: {native} bytes");

    Assert.Positive(native, "native memory usage");

    Debug.Log($"total mem: {managed + native} bytes");
  }

}