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

    private const int NFRAMES = 144;
    private const int N       = 10000;


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

    internal static List<string> GetSomeKeysFor(IDictionary lookup, int nExist, int nFake)
    {
      var keys = new List<string>(lookup.Count);

      if (lookup.Count > 0)
      {
        var iter = lookup.GetEnumerator();

        while (nExist > 0)
        {
          while (iter.MoveNext())
          {
            keys.Add(iter.Key?.ToString() ?? "");
            if (--nExist == 0)
              break;
          }

          iter.Reset();
        }
      }

      while (nFake --> 0)
      {
        keys.Add(Strings.MakeGUID());
      }

      return keys;
    }


    private static IEnumerator DoLookupTest(IDictionary lookup, string name, float tooslow)
    {
      var tests = GetSomeKeysFor(lookup, N / 2, N / 2);

      var stopwatch = new Stopwatch();

      yield return null;

      int i = NFRAMES;
      while (i --> 0)
      {
        int j = tests.Count;

        tests.Shuffle();

        stopwatch.Start();

        while (j --> 0)
        {
          if (lookup.Contains(tests[j]))
          {
            lookup.Add("fef", "sheff");
            lookup.Remove("fef");
          }
          else
          {
            lookup.Add(tests[j], "wef");
            lookup.Remove(tests[j]);
          }
        }

        stopwatch.Stop();

        yield return null;
      }

      float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

      Debug.Log($"{name}[{lookup.Count}]: Average ms per {N} lookups = {ms:N1}ms  ({ms / tooslow:P0} of budget)");

      Assert.Less(ms, tooslow);
    }


    [UnityTest]
    public static IEnumerator HashMapLookup()
    {
      const string name = "HashMap";
      const float tooslow = 8f;

      var lookup = GetTestHashMap(2048);

      yield return DoLookupTest(lookup, name, tooslow);

      var swap = new HashMap<string,string>();
      while (lookup.Count > 8)
      {
        int i = lookup.Count >> 1;
        foreach (var (key, value) in lookup)
        {
          if (i --> 0)
            swap[key] = value;
          else
            break;
        }

        (lookup,swap) = (swap,lookup);
        swap.Clear();

        yield return DoLookupTest(lookup, name, tooslow);
      }
    }


    [UnityTest]
    public static IEnumerator DictionaryLookup()
    {
      const string name = "Dictionary";
      const float tooslow = 8f;

      var lookup = GetTestDict(2048);

      yield return DoLookupTest(lookup, name, tooslow);

      var swap = new Dictionary<string, string>();
      while (lookup.Count > 8)
      {
        int i = lookup.Count >> 1;
        foreach (var kvp in lookup)
        {
          if (i --> 0)
            swap[kvp.Key] = kvp.Value;
          else
            break;
        }

        (lookup,swap) = (swap,lookup);
        swap.Clear();

        yield return DoLookupTest(lookup, name, tooslow);
      }
    }

    [UnityTest]
    public static IEnumerator HashtableLookup()
    {
      const string name = "Hashtable";
      const float tooslow = 8f;

      var lookup = GetTestTable(2048);

      yield return DoLookupTest(lookup, name, tooslow);

      var swap = new Hashtable();
      while (lookup.Count > 8)
      {
        int i = lookup.Count >> 1;

        var iter = lookup.GetEnumerator();
        while (iter.MoveNext())
        {
          if (i --> 0)
            swap[iter.Key] = iter.Value;
          else
            break;
        }

        (lookup,swap) = (swap,lookup);
        swap.Clear();

        yield return DoLookupTest(lookup, name, tooslow);
      }
    }

  }
}