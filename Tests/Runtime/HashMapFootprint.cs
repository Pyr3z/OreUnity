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
using System.Collections;


namespace Ore.Tests
{
  public static class HashMapFootprint
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
    public static IEnumerator ClearVersusClearNoAlloc()
    {
      yield break;
    }

  }
}