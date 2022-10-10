/*! @file       Tests/Runtime/HashMapSpeed.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Speed Tests:
 *  [x] HashMap vs Dictionary
 *  [x] HashMap vs Hashtable
 *  [x] HashMap vs HashSet
 *  [ ] HashMap vs List (binary search)
 *  [ ] HashMap vs List (linear search)
 *  [ ] HashMap vs Array (binary search)
 *  [ ] HashMap vs Array (linear search)
 *  [x] Hashtable vs Dictionary
 *  [ ] HashMap.Clear vs HashMap.ClearNoAlloc
**/

using Ore;

using NUnit.Framework;
using UnityEngine.TestTools;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;
using Stopwatch = System.Diagnostics.Stopwatch;


public static class HashMapSpeed
{

  private const int   NFRAMES = 66;
  private const int   N       = 10000;
  private const float TOOSLOW = 100f;


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
      string guid = Strings.MakeGUID();
      if (lookup.Contains(guid))
      {
        ++nFake;
      }
      else
      {
        keys.Add(guid);
      }
    }

    return keys;
  }


  private static IEnumerator DoLookupTest(IDictionary lookup, string name, float pExist)
  {
    var tests = GetSomeKeysFor(lookup, (int)(pExist * N + 0.5f), (int)((1f-pExist) * N + 0.5f));

    var stopwatch = new Stopwatch();

    yield return null;

    int i = NFRAMES;
    while (i --> 0)
    {
      int j = tests.Count;
      int nExist = 0;

      // tests.Shuffle();

      stopwatch.Start();

      while (j --> 0)
      {
        if (lookup.Contains(tests[j]))
        {
          ++nExist;

          try
          {
            Assert.AreEqual(tests[j], lookup[tests[j]], "");
          }
          catch (AssertionException ae)
          {
            if (lookup is HashMap<string,string> fmap)
            {
              throw new AssertionException($"CachedLookup={fmap.CachedLookup}; {ae.Message}", ae);
            }

            throw;
          }
        }
        else
        {
          // need to do this to make sure tests are balanced
          object optional = null;

          try
          {
            optional = lookup[tests[j]];
          }
          catch {  }

          Assert.Null(optional);
        }
      }

      stopwatch.Stop();

      Assert.AreEqual(0f, (float)nExist/tests.Count - pExist, 0.03f);

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    // RFC 4180 CSV:
    Debug.Log($"\"{name}\",\"{lookup.Count}\",\"{tests.Count}\",\"{pExist:P0}\",\"{ms:N2}\"");

    // Debug.Log($"{name}[{lookup.Count}]: Average ms per {N} lookups = {ms:N1}ms  ({ms / tooslow:P0} of budget)");

    Assert.Less(ms, TOOSLOW);
  }


  private static readonly float[] PERCENTS = { 0f, 0.5f, 1f };
  private static readonly int[]   SIZES    = { 33, 666, 1337, 3141592 };


  [UnityTest]
  public static IEnumerator HashMapLookup(
    [ValueSource(nameof(PERCENTS))] float percentExist,
    [ValueSource(nameof(SIZES))]      int size )
  {
    const string TESTNAME = "HashMap";

    var lookup = GetTestHashMap(size);

    System.GC.Collect();
    return DoLookupTest(lookup, TESTNAME, percentExist);
  }

  [UnityTest]
  public static IEnumerator DictionaryLookup(
    [ValueSource(nameof(PERCENTS))] float percentExist,
    [ValueSource(nameof(SIZES))]      int size )
  {
    const string TESTNAME = "Dictionary";

    var lookup = GetTestDict(size);

    System.GC.Collect();
    return DoLookupTest(lookup, TESTNAME, percentExist);
  }

  [UnityTest]
  public static IEnumerator HashtableLookup(
    [ValueSource(nameof(PERCENTS))] float percentExist,
    [ValueSource(nameof(SIZES))]      int size )
  {
    const string TESTNAME = "Hashtable";

    var lookup = GetTestTable(size);

    System.GC.Collect();
    return DoLookupTest(lookup, TESTNAME, percentExist);
  }

}
