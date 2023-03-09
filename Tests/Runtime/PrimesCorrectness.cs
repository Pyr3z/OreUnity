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

using System.Collections;
using Ore;

using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;

using Math = System.Math;


public static class PrimesCorrectness
{
  [Test]
  public static void BinarySearchCorrectness([Values(500)] int n)
  {
    // TODO move me
    var testvalues = Primes10K.GetTestValues(n, n, true);

    foreach (int value in testvalues)
    {
      int system = System.Array.BinarySearch(Primes10K.InOrder, value);
      int ore   = Primes10K.InOrder.BinarySearch(value);

      Assert.AreEqual(system.Sign(), ore.Sign(), $"(sign of result) forValue={value},system={system},ore={ore}");
      Assert.AreEqual(system, ore, $"forValue={value}");
    }
  }

  [Test]
  public static void IsPrimeNoLookup([Values(200)] int n) // NoLookup should techically be axiomatic
  {
    DoIsPrime<int>(Primes.IsPrimeNoLookup, n);
  }

  [Test]
  public static void IsPrimeLookup([Values(200)] int n)
  {
    DoIsPrime<int>(Primes.IsPrimeLookup, n);
  }

  [Test]
  public static void IsLongPrimeNoLookup([Values(100)] int n)
  {
    Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) -  57), "IsPrime(2^62 -  57)");
    Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) -  87), "IsPrime(2^62 -  87)");
    // leaving a few commented out since this is so slow
 // Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 117), "IsPrime(2^62 - 117)");
 // Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 143), "IsPrime(2^62 - 143)");
 // Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 153), "IsPrime(2^62 - 153)");
 // Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 167), "IsPrime(2^62 - 167)");
 // Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 171), "IsPrime(2^62 - 171)");
 // Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 195), "IsPrime(2^62 - 195)");
    Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 203), "IsPrime(2^62 - 203)");
    Assert.True(Primes.IsLongPrimeNoLookup((1L << 62) - 273), "IsPrime(2^62 - 273)");

    DoIsPrime<long>(Primes.IsLongPrimeNoLookup, n);
  }

  private static void DoIsPrime<T>(System.Func<T,bool> testFunc, int n)
    where T : System.IConvertible
  {
    var knownPrimes = Primes10K.GetTestValues(n, 0);
    var nonPrimes   = Primes10K.GetTestValues(0, n);

    for (int i = 0; i < n; ++i)
    {
      Assert.True(testFunc(knownPrimes[i].Cast<T>()), $"value={knownPrimes[i]}");
      Assert.False(testFunc(nonPrimes[i].Cast<T>()),  $"value={nonPrimes[i]}");
    }
  }

  [Test]
  public static void GetRandom([Values(200)] int n)
  {
    int i = n / 2;

    while (i --> 0)
    {
      int rand = Primes.GetRandom();
      Assert.True(Primes.IsPrime(rand));
    }

    i = n / 2;
    while (i --> 0)
    {
      int rand = Primes.GetRandom(Random.Range(0, Primes.MaxValue), Random.Range(0, Primes.MaxValue));
      Assert.True(Primes.IsPrime(rand));
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

  [Test]
  public static void NextHashableSize()
  {
    DoNext((p,hp) => Primes.NextHashableSize(p, hp));

    int prime = Primes.GetRandom(max: Primes.MaxSizePrime >> 1);
    prime = Primes.NextHashableSize(prime);

    Assert.True(Primes.IsPrime(prime));
    Assert.Contains(prime, Primes.HashableSizes as ICollection);
    Assert.AreEqual(prime, Primes.NextHashableSize(prime, incr: 0));
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

    Assert.AreEqual(2,     Primes.NearestTo(-1));
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
      Assert.True(Primes.IsPrime(prime), $"IsPrimeLookup({prime})");

      int digits    = Integers.CalcDigits(value);
      int dist      = Math.Abs(value - prime);
      int threshold = (int)(digits * MAX_DIST_PER_DIGIT + 0.9999f);

      Assert.LessOrEqual(dist, threshold, $"distance between {value} and {prime}");
    }

    int bigPrime = Primes.NearestTo(Primes.MaxSizePrime + 1);
    Assert.True(Primes.IsPrime(bigPrime), "Primes.IsPrime(bigPrime)");
    Assert.LessOrEqual(bigPrime, Primes.MaxValue, "bigPrime <= Primes.MaxValue");

    bigPrime = Primes.NearestTo(Primes.MaxValue + 1);
    Assert.AreEqual(Primes.MaxValue, bigPrime, "bigPrime == Primes.MaxValue");
  }

  [Test]
  public static void FindGoodHashPrimes()
  {
    var bob = new System.Text.StringBuilder("private static readonly int[] GOODPRIMES = {\n");

    int perline = 10;

    foreach (int hashprime in Hashing.HashPrimes)
    {
      bool good = true;

      foreach (int prime in Primes.HashableSizes)
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


  [Test, Timeout(10000)]
  public static void FindGoodSizePrimes(
    [Values(97, 193, 769)]       int   hashprime,
    [Values(1.1f, 1.15f, 1.19f)] float growFactor)
  {
    var bob = new System.Text.StringBuilder("static readonly int[] PRIMESIZES =\n{\n  ");

    const int perline = 10;
    int count = 0;

    int p = 5;
    while (p <= Primes.MaxSizePrime)
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
      Debug.Log($"Wrote C# to \"./{Filesystem.LastModifiedPath}\".");
    }
    else
    {
      Debug.Log(bob.ToString());
    }
  }

  [Test]
  public static void PrimeDeviationPerDigit()
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

    Assert.Pass();
  }

}
