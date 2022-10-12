/*! @file       Tests/Runtime/PrimesCorrectness.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Correctness Tests: (x = pass, ~ = skipped for now)
 *  [x] Binary Search
 *  [x] Primes.IsPrime
 *  [x] Primes.IsPrimeNoLookup
 *  [x] Primes.Next
 *  [x] Primes.NextNoLookup
 *  [~] Hash collision ratio
**/

using Ore;

using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

using Math = System.Math;


public static class PrimesCorrectness
{
  [Test]
  public static void BinarySearchCorrectness()
  {
    var testvalues = Primes10K.GetTestValues(500, 500, true);

    foreach (int value in testvalues)
    {
      int system = System.Array.BinarySearch(Primes10K.InOrder, value);
      int ore   = Primes10K.InOrder.BinarySearch(value);

      Assert.AreEqual(system.Sign(), ore.Sign(), $"(sign of result) forValue={value},system={system},ore={ore}");
      Assert.AreEqual(system, ore, $"forValue={value}");
    }
  }

  [Test]
  public static void IsPrimeNoLookup() // NoLookup should techically be axiomatic
  {
    const int N = 200;

    var knownprimes = Primes10K.GetTestValues(N, 0);
    var knownnons   = Primes10K.GetTestValues(0, N);

    for (int i = 0; i < N; ++i)
    {
      Assert.True(Primes.IsPrimeNoLookup(knownprimes[i]), $"value={knownprimes[i]}");
      Assert.False(Primes.IsPrimeNoLookup(knownnons[i]),  $"value={knownnons[i]}");
    }
  }

  [Test]
  public static void IsPrime()
  {
    const int N = 500;

    var knownprimes = Primes10K.GetTestValues(N, 0);
    var knownnons   = Primes10K.GetTestValues(0, N);

    for (int i = 0; i < N; ++i)
    {
      Assert.True(Primes.IsPrime(knownprimes[i]), $"value={knownprimes[i]}");
      Assert.False(Primes.IsPrime(knownnons[i]),  $"value={knownnons[i]}");
    }

    foreach (int value in Primes10K.GetTestValues(N >> 1, N >> 1, true))
    {
      bool lookup = Primes.IsPrime(value);
      bool nolook = Primes.IsPrimeNoLookup(value);

      Assert.AreEqual(lookup, nolook, $"value={value}");
    }
  }

  [Test]
  public static void Next()
  {
    DoNext(Primes.Next);
  }

  [Test]
  public static void NextNoLookup()
  {
    DoNext(Primes.NextNoLookup);
  }

  private static void DoNext(System.Func<int, int, int> nextFunc)
  {
    var testvalues = Primes10K.GetTestValues(100, 100);

    foreach (int hashprime in Hashing.HashPrimes.Append(int.MaxValue).Append(Primes.MaxValue))
    {
      foreach (int value in testvalues)
      {
        int next = nextFunc(value, hashprime);

        string msg = $"hashprime={hashprime},value={value},next={next}";
        Assert.Positive(next, msg);
        Assert.True(Primes.IsPrime(next), msg);

        // MaxValue is a last resort of what's returned; test values shouldn't trigger it
        Assert.Less(next, Primes.MaxValue, msg);
      }
    }
  }

  [Test]
  public static void NearestTo()
  {
    const float MAX_DIST_PER_DIGIT = 2.31f + 6.05f; // experimental avg+stdev calculated from Primes10K

    Assert.AreEqual(7,     Primes.NearestTo(7));
    Assert.AreEqual(25229, Primes.NearestTo(25228));
    Assert.AreEqual(3617,  Primes.NearestTo(3615));
    Assert.AreEqual(3709,  Primes.NearestTo(3711));
    Assert.AreEqual(5059,  Primes.NearestTo(5066));
    Assert.AreEqual(5077,  Primes.NearestTo(5068));
    Assert.AreEqual(5077,  Primes.NearestTo(5070));

    var data = Primes10K.GetTestValues(6, 666);

    foreach (int value in data)
    {
      int prime = Primes.NearestTo(value);
      Assert.True(Primes.IsPrime(prime), $"IsPrime({prime})");

      int digits    = Integers.CalcDigits(value);
      int dist      = Math.Abs(value - prime);
      int threshold = (int)(digits * MAX_DIST_PER_DIGIT + 0.9999f);

      Assert.LessOrEqual(dist, threshold, $"distance between {value} and {prime}");
    }
  }

  [Test]
  public static void FindGoodHashPrimes()
  {
    var bob = new System.Text.StringBuilder("private static readonly int[] GOODPRIMES = {\n");

    int perline = 10;

    foreach (int hashprime in Hashing.HashPrimes)
    {
      bool good = true;

      foreach (int prime in Primes.HashtableSizes)
      {
        if ((prime - 1) % hashprime == 0)
        {
          good = false;
          break;
        }
      }

      if (good)
      {
        bob.Append(hashprime).Append(',');
        if (--perline == 0)
        {
          bob.AppendLine();
          perline = 10;
        }
      }
    }

    bob.Append("\n};");

    Debug.Log(bob.ToString());
  }

  private static readonly int[] HASHPRIMES =
  {
    97, 193, 769
  };

  [Test, Timeout(10000)]
  public static void FindGoodSizePrimes(
    [ValueSource(nameof(HASHPRIMES))] int hashprime,
    [Values(1.1f, 1.15f, 1.19f)] float growFactor)
  {
    var bob = new System.Text.StringBuilder("static readonly int[] PRIMESIZES =\n{\n  ");

    const int perline = 10;
    int count = 0;

    int p = 5;
    while (p <= Primes.MaxHashtableSize)
    {
      if ((p - 1) % hashprime != 0)
      {
        bob.Append(p).Append(',');

        if (++count % perline == 0)
        {
          bob.AppendLine().Append("  ");
        }
      }

      p = (int)(p * growFactor + 0.5f);
      p = Primes.NextNoLookup(p);
    }

    bob.Append("\n};\n");

    bob.Insert(0, $"const int N_PRIMESIZES = {count};\n\n");

    bob.Insert(0, $"const int HASHPRIME = {hashprime};\n\n");

    bob.Insert(0, $"const float GROWFACTOR = {growFactor:F2}f;\n\n");

    if (Filesystem.TryWriteText($"Temp/PrimeSizes_{growFactor:F2}f_{hashprime}.cs", bob.ToString()))
    {
      Debug.Log($"Wrote C# to \"./{Filesystem.LastWrittenPath}\".");
    }
    else
    {
      Debug.Log(bob.ToString());
    }
  }

  [Test]
  public static void HashCollisionRatio()
  {
    var primedeltas = new List<int>(10000);
    
    double sum = 0, sumperdig = 0;

    for (int i = 1, ilen = Primes10K.InOrder.Length; i < ilen; ++i)
    {
      int d = Primes10K.InOrder[i] - Primes10K.InOrder[i-1];
      sum += d;
      primedeltas.Add(d);

      int digits = Integers.CalcDigits(Primes10K.InOrder[i]);
      sumperdig += (double)d / digits;
    }

    sum /= primedeltas.Count;
    sumperdig /= primedeltas.Count;

    double stdev = primedeltas.Average(v => Math.Pow(v - sum, 2));
    stdev = Math.Sqrt(stdev);

    double stdevperdig = primedeltas.Average(v => Math.Pow((double)v / Integers.CalcDigits(v) - sumperdig, 2));
    stdevperdig = Math.Sqrt(stdevperdig);

    Debug.Log($"Avg + Stdev of first 10k primes: avg={sum:F1},stdev={stdev:F1}");
    Debug.Log($"Avg + Stdev PER DIGIT of primes: avg={sumperdig:F1},stdev={stdevperdig:F1}");

    Assert.Inconclusive("Test goals TBD");
  }

}
