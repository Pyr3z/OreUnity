/*! @file       Tests/Runtime/HashMapSpeed.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Speed Tests:
 *  [ ] HashMap vs Dictionary
 *  [ ] HashMap vs Hashtable
 *  [ ] HashMap vs HashSet
 *  [ ] HashMap vs List (binary search)
 *  [ ] HashMap vs List (linear search)
 *  [ ] HashMap vs Array (binary search)
 *  [ ] HashMap vs Array (linear search)
 *  [ ] Hashtable vs Dictionary
 *  [ ] HashMap.Clear vs HashMap.ClearNoAlloc
**/

using NUnit.Framework;
using UnityEngine.TestTools;
using System.Collections;


namespace Ore.Tests
{
  public static class HashMapSpeed
  {

    [UnityTest]
    public static IEnumerator VersusDictionary()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator VersusHashtable()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator VersusHashSet()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator VersusListBinary()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator VersusListLinear()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator VersusArrayBinary()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator VersusArrayLinear()
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator DictionaryVersusHashtable() // Control
    {
      yield break;
    }

    [UnityTest]
    public static IEnumerator ClearVersusClearNoAlloc()
    {
      yield break;
    }

  }
}