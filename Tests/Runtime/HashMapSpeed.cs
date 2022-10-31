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

      while (j --> 0)
      {
        stopwatch.Start();
        if (lookup.Contains(tests[j]))
        {
          stopwatch.Stop();
          Assert.AreEqual(tests[j], lookup[tests[j]], "key==value");
          ++nExist;
        }
        else
        {
          stopwatch.Stop();
        }
      }

      Assert.AreEqual(0f, (float)nExist/tests.Count - pExist, 0.03f);

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    // RFC 4180 CSV:
    Debug.Log($"\"{name}\",\"{lookup.Count}\",\"{tests.Count}\",\"{pExist:P0}\",\"{ms:N3}\"");

    // Debug.Log($"{name}[{lookup.Count}]: Average ms per {N} lookups = {ms:N1}ms  ({ms / tooslow:P0} of budget)");

    Assert.Less(ms, TOOSLOW);
  }


  private static readonly float[] PERCENTS = { 0f, 0.5f, 1f };
  private static readonly int[]   SIZES    = { 5, 513, 1337 };


  [UnityTest]
  public static IEnumerator LookupHashMap(
    [ValueSource(nameof(PERCENTS))] float percentExist,
    [ValueSource(nameof(SIZES))]      int size )
  {
    const string TESTNAME = nameof(LookupHashMap);

    var lookup = GetTestHashMap(size);

    System.GC.Collect();
    return DoLookupTest(lookup, TESTNAME, percentExist);
  }

  [UnityTest]
  public static IEnumerator LookupDictionary(
    [ValueSource(nameof(PERCENTS))] float percentExist,
    [ValueSource(nameof(SIZES))]      int size )
  {
    const string TESTNAME = nameof(LookupDictionary);

    var lookup = GetTestDict(size);

    System.GC.Collect();
    return DoLookupTest(lookup, TESTNAME, percentExist);
  }

  [UnityTest]
  public static IEnumerator LookupHashtable(
    [ValueSource(nameof(PERCENTS))] float percentExist,
    [ValueSource(nameof(SIZES))]      int size )
  {
    const string TESTNAME = nameof(LookupHashtable);

    var lookup = GetTestTable(size);

    System.GC.Collect();
    return DoLookupTest(lookup, TESTNAME, percentExist);
  }


  [UnityTest]
  public static IEnumerator LookupBoxlessHashMap(
    [ValueSource(nameof(PERCENTS))] float pExist,
    [ValueSource(nameof(SIZES))]    int   size )
  {
    const string TESTNAME = nameof(LookupBoxlessHashMap);

    System.GC.Collect();

    var lookup = GetTestHashMap(size);

    var tests = GetSomeKeysFor(lookup, (int)(pExist * N + 0.5f), (int)((1f -pExist) * N + 0.5f));

    var stopwatch = new Stopwatch();

    yield return null;

    int i = NFRAMES;
    while (i --> 0)
    {
      int j      = tests.Count;
      int nExist = 0;

      while (j --> 0)
      {
        stopwatch.Start();
        if (lookup.Find(tests[j], out string result))
        {
          stopwatch.Stop();
          Assert.AreEqual(tests[j], result);
          ++nExist;
        }
        else
        {
          stopwatch.Stop();
        }
      }

      Assert.AreEqual(0f, (float)nExist /tests.Count - pExist, 0.03f);

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    // RFC 4180 CSV:
    Debug.Log($"\"{TESTNAME}\",\"{lookup.Count}\",\"{tests.Count}\",\"{pExist:P0}\",\"{ms:N3}\"");

    Assert.Less(ms, TOOSLOW);
  }

  [UnityTest]
  public static IEnumerator LookupBoxlessDictionary(
    [ValueSource(nameof(PERCENTS))] float pExist,
    [ValueSource(nameof(SIZES))]    int   size )
  {
    const string TESTNAME = nameof(LookupBoxlessDictionary);

    System.GC.Collect();

    var lookup = GetTestDict(size);

    var tests = GetSomeKeysFor(lookup, (int)(pExist * N + 0.5f), (int)((1f -pExist) * N + 0.5f));

    var stopwatch = new Stopwatch();

    yield return null;

    int i = NFRAMES;
    while (i --> 0)
    {
      int j      = tests.Count;
      int nExist = 0;


      while (j --> 0)
      {
        stopwatch.Start();
        if (lookup.TryGetValue(tests[j], out string result))
        {
          stopwatch.Stop();
          Assert.AreEqual(tests[j], result);
          ++nExist;
        }
        else
        {
          stopwatch.Stop();
        }
      }

      Assert.AreEqual(0f, (float)nExist /tests.Count - pExist, 0.03f);

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    // RFC 4180 CSV:
    Debug.Log($"\"{TESTNAME}\",\"{lookup.Count}\",\"{tests.Count}\",\"{pExist:P0}\",\"{ms:N3}\"");

    Assert.Less(ms, TOOSLOW);
  }


  [UnityTest]
  public static IEnumerator InsertBoxlessHashMap([ValueSource(nameof(SIZES))] int size)
  {
    const string TESTNAME = nameof(InsertBoxlessHashMap);

    System.GC.Collect();

    var stopwatch = new Stopwatch();

    yield return null;

    int i = NFRAMES;
    while (i --> 0)
    {
      var lookup = new HashMap<string,string>();
      var tests = GetTestList(size);

      int j = size;

      while (j --> 0)
      {
        string test = tests[j];
        int precount = lookup.Count;

        stopwatch.Start();
        lookup[test] = test;
        lookup[test] = test;
        stopwatch.Stop();

        Assert.AreEqual(precount + 1, lookup.Count);
        Assert.AreEqual(test,         lookup[test]);
      }

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    // RFC 4180 CSV:
    Debug.Log($"\"{TESTNAME}\",\"{size}\",\"{ms:N3}\"");

    Assert.Less(ms, TOOSLOW);
  }

  [UnityTest]
  public static IEnumerator InsertBoxlessDictionary([ValueSource(nameof(SIZES))] int size)
  {
    const string TESTNAME = nameof(InsertBoxlessDictionary);

    System.GC.Collect();

    var stopwatch = new Stopwatch();

    yield return null;

    int i = NFRAMES;
    while (i --> 0)
    {
      var lookup = new Dictionary<string,string>();
      var tests = GetTestList(size);

      int j = size;

      while (j --> 0)
      {
        string test     = tests[j];
        int    precount = lookup.Count;

        stopwatch.Start();
        lookup[test] = test;
        lookup[test] = test;
        stopwatch.Stop();

        Assert.AreEqual(precount + 1, lookup.Count);
        Assert.AreEqual(test,         lookup[test]);
      }

      yield return null;
    }

    float ms = stopwatch.ElapsedMilliseconds / (float)NFRAMES;

    // RFC 4180 CSV:
    Debug.Log($"\"{TESTNAME}\",\"{size}\",\"{ms:N3}\"");

    Assert.Less(ms, TOOSLOW);
  }

}
