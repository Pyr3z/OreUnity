/*! @file       Tests/Runtime/HashMapCorrectness.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Correctness Tests: (x = pass)
 *  [x] HashMapParams.Default
 *  [x] HashMap.HasKey
 *  [x] HashMap.Map
 *  [x] HashMap.Unmap
 *  [x] HashMap.Remap
 *  [x] HashMap.TryMap
 *  [x] HashMap.Clear
 *  [x] HashMap.EnsureCapacity
 *  [x] value = HashMap[key]
 *  [x] HashMap[key] = value
**/

using Ore;

using NUnit.Framework;

using System.Collections.Generic;
using UnityEngine;


public static class HashMapCorrectness
{

  private static readonly string[] CONST_TEST_STRINGS =
  {
    "fef",
    "0123456789",
    "♠ Levi Perez",
    "m_Script",
    System.Guid.Empty.ToString(),
    System.DateTime.UtcNow.ToLongDateString(),
    System.DateTime.UtcNow.ToLongTimeString(),
  };

  private static List<string> GetTestStrings(int nGuids, bool includeConsts = true)
  {
    var list = new List<string>(nGuids + CONST_TEST_STRINGS.Length);

    if (includeConsts)
    {
      list.AddRange(CONST_TEST_STRINGS);
    }

    while (nGuids --> 0)
    {
      string guid = System.Guid.NewGuid().ToString();

      while (list.Contains(guid))
      {
        guid = System.Guid.NewGuid().ToString();
      }

      list.Add(guid);
    }

    return list;
  }


  [Test]
  public static void DefaultParameters()
  {
    int defaultHashPrime = HashMapParams.Default.HashPrime;

    foreach (int hashprime in new []{ defaultHashPrime }) // TODO test other preset hashprimes?
    {
      foreach (int prime in Primes.HashableSizes)
      {
        Assert.NotZero((prime - 1) % hashprime, $"hashprime={hashprime},value={prime}");
      }
    }
  }

  [Test]
  public static void HasKey()
  {
    var map = new HashMap<string,string>();

    Assert.False(map.HasKey("fef"), "map.HasKey('fef')");

    map["fef"] = "bub";

    Assert.True(map.HasKey("fef"), "map.HasKey('fef')");
    Assert.False(map.HasKey("bub"), "map.HasKey('bub')");

    foreach (string key in GetTestStrings(100))
    {
      map[key] = key;

      Assert.True(map.HasKey(key), "map.HasKey(key)");
      Assert.True(map.HasKey("fef"), "map.HasKey('fef') (still?)");
    }

    map.Unmap("fef");

    Assert.False(map.HasKey("fef"), "map.HasKey('fef')");
  }

  [TestCase(1)]
  [TestCase(666)]
  [TestCase(11111)]
  public static void Map(int n)
  {
    var map = new HashMap<string,string>();

    var testvalues = GetTestStrings(n);

    foreach (string value in testvalues)
    {
      Assert.True(map.Map(value, value),  "initial mapping");
      Assert.True(map.ContainsKey(value), $"contains key \"{value}\"");
      Assert.False(map.Map(value, value), "non-overwriting mapping");
    }

    foreach (string value in testvalues)
    {
      // don't assert Find result since this test is for map correctness
      _ = map.Find(value, out string v);
      Assert.AreEqual(value, v);
    }

    Debug.Log($"{nameof(Map)}({n}): Count={map.Count},Capacity={map.Capacity},LifetimeAllocs={map.LifetimeAllocs}");
  }

  [Test]
  public static void Unmap()
  {
    var map = new HashMap<string,string>();

    Assert.False(map.Unmap("fef"), "unmapping fake key 1");

    map.Add("bub", "wub");

    Assert.False(map.Unmap("fef"), "unmapping fake key 2");
    Assert.True(map.Unmap("bub"), "removing key \"bub\"");

    Assert.Zero(map.Count, "mapped item count");

    var testvalues = GetTestStrings(100);

    foreach (string value in testvalues)
    {
      map.Add(value, value);
    }

    Assert.AreEqual(testvalues.Count, map.Count);

    testvalues.Shuffle();

    foreach (string value in testvalues)
    {
      Assert.True(map.Unmap(value));
    }
  }

  [Test]
  public static void Remap()
  {
    var map = new HashMap<string,string>();

    Assert.True(map.Remap("fef", "fef"));

    Assert.True(map.Remap("fef", "bub"));

    Assert.AreEqual(1, map.Count, "map count after 1st remap");

    var testvalues = GetTestStrings(100);

    foreach (string value in testvalues)
    {
      map.Add(value, value);
    }

    for (int i = 0, ilen = testvalues.Count; i < ilen; ++i)
    {
      int j = (i + 1) % ilen;

      Assert.True(map.Remap(testvalues[i], testvalues[j]));
      Assert.True(map.Find(testvalues[i], out string v));
      Assert.True(v == testvalues[j]);
    }
  }

  [Test]
  public static void TryMap()
  {
    var map = new HashMap<string,string>();

    Assert.Null(map.TryMap(null, null, out string prev));

    var testvalues = GetTestStrings(1);

    for (int i = 0, ilen = testvalues.Count / 2; i < ilen; ++i)
    {
      map.Add(testvalues[i], "bub");
    }

    testvalues.Shuffle();

    foreach (string key in testvalues)
    {
      bool? result = map.TryMap(key, "flee", out prev);

      Assert.NotNull(result);

      if (result == true)
      {
        Assert.Null(prev);
        Assert.AreEqual("flee", map[key]);
      }
      else
      {
        Assert.AreEqual("bub", prev);
      }
    }
  }

  [Test]
  public static void IndexOperator()
  {
    var map = new HashMap<string,string>()
    {
      ["fef"] = "cyka"
    };

    Assert.True(map.Find("fef", out string val) && val == "cyka");

    map["fef"]  = "bub";
    map["toot"] = "blee";
    map["snee"] = "woon";

    Assert.AreEqual(3, map.Count);

    Assert.AreEqual("bub", map["fef"]);
    Assert.AreEqual("blee", map["toot"]);
    Assert.AreEqual("woon", map["snee"]);
  }

  [Test]
  public static void Clear()
  {
    DoClear(map => map.ClearAlloc(), 1);
  }

  [Test]
  public static void ClearNoAlloc()
  {
    DoClear(map => map.ClearNoAlloc(), 0);
  }

  private static void DoClear(System.Func<HashMap<string,string>, bool> clearFunc, int expectedAlloc)
  {
    var map = new HashMap<string,string>()
    {
      ["trash"] = "iOS"
    };

    int allocs = map.LifetimeAllocs;

    Assert.True(clearFunc(map));
    Assert.Zero(map.Count);
    Assert.AreEqual(allocs + 1 * expectedAlloc, map.LifetimeAllocs);

    Assert.False(clearFunc(map));
    Assert.AreEqual(allocs + 2 * expectedAlloc, map.LifetimeAllocs);

    foreach (string value in GetTestStrings(100))
    {
      map.Add(value, value);
    }

    Assert.Greater(map.Count, 0);

    allocs = map.LifetimeAllocs;
    Assert.True(clearFunc(map));
    Assert.Zero(map.Count);
    Assert.AreEqual(allocs + 1 * expectedAlloc, map.LifetimeAllocs);
  }

  [Test]
  public static void EnsureCapacity()
  {
    var map = new HashMap<string,string>();

    int cap = map.Capacity;

    Assert.True(map.EnsureCapacity(map.Capacity), $"cap={cap}");
    Assert.AreEqual(cap, map.Capacity, "capacity");
    Assert.AreEqual(1, map.LifetimeAllocs);

    Assert.True(map.EnsureCapacity(cap*2), $"cap={cap*2}");
    Assert.AreEqual(2, map.LifetimeAllocs);

    var guids = GetTestStrings(map.Capacity, false);

    foreach (string guid in guids)
    {
      map[guid] = guid;
    }

    Assert.AreEqual(map.Capacity, map.Count, "capacity <=> count");
    Assert.AreEqual(2, map.LifetimeAllocs);

    cap = map.Capacity + 100;

    Assert.True(map.EnsureCapacity(cap));
    Assert.AreEqual(3, map.LifetimeAllocs);

    foreach (string guid in guids)
    {
      Assert.True(map[guid] == guid);
    }
  }

  [Test]
  public static void Enumerator()
  {
    var data = HashMapSpeed.GetTestHashMap(100);
    var dict = new Dictionary<string,string>(data);

    Assert.AreEqual(dict.Count, data.Count);

    int c = 0;
    foreach (var (key, value) in data)
    {
      Assert.True(dict[key] == value);
      ++c;
    }

    Assert.AreEqual(dict.Count, c);

    foreach (var kvp in dict)
    {
      Assert.True(data[kvp.Key] == kvp.Value);
    }
  }

}
