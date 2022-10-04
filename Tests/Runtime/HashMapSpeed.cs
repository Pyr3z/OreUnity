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

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;
using Stopwatch = System.Diagnostics.Stopwatch;


namespace Ore.Tests
{
  public static class HashMapSpeed
  {

    private const int NFRAMES = 300;
    private const int N       = 1000;


    internal static HashSet<string> GetTestSet(int n)
    {
      var set = new HashSet<string>();

      while (n --> 0)
      {
        if (!set.Add(Strings.MakeGUID()))
        {
          ++n;
        }
      }

      return set;
    }

    internal static List<string> GetTestList(int n)
    {
      return GetTestSet(n).ToList();
    }

    internal static string[] GetTestArray(int n)
    {
      return GetTestSet(n).ToArray();
    }

    internal static Dictionary<string,string> GetTestDict(int n)
    {
      var keys = GetTestSet(n);
      var dict = new Dictionary<string,string>(n);

      foreach (string key in keys)
      {
        dict[key] = key;
      }

      return dict;
    }

    internal static Hashtable GetTestTable(int n)
    {
      return new Hashtable(GetTestDict(n));
    }

    internal static HashMap<string,string> GetTestHashMap(int n)
    {
      var map = new HashMap<string,string>(n);

      while (n --> 0)
      {
        string guid = Strings.MakeGUID();
        if (!map.Map(guid, guid))
        {
          ++n;
        }
      }

      return map;
    }


    private static IEnumerator DoLookupTest(IDictionary data, string name, float tooslow)
    {
      var stopwatch = new Stopwatch();

      yield return null;

      int i = NFRAMES;
      while (i --> 0)
      {
        int j = data.Count;

        stopwatch.Start();

        while (j --> 0)
        {

        }

        stopwatch.Stop();

        yield return null;
      }

      float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

      Debug.Log($"{name}[{data.Count}]: Average ms per {N} lookups = {ms:N1}ms  ({ms / tooslow:P0} of budget)");

      Assert.Less(ms, tooslow);
    }


    [UnityTest]
    public static IEnumerator HashMapLookup()
    {
      string name = "HashMap<string,string>";

      var data = GetTestHashMap(1024);

      yield return DoLookupTest(data, name, 16f);

      while (data.Count > 8)
      {

      }
    }


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