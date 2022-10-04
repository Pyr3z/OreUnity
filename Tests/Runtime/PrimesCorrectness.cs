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

using System.Linq;

using NUnit.Framework;

using UnityEngine;


namespace Ore.Tests
{
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
      Assert.AreEqual(7,     Primes.NearestTo(7));
      Assert.AreEqual(25229, Primes.NearestTo(25228));
      Assert.AreEqual(3617,  Primes.NearestTo(3615));
      Assert.AreEqual(3709,  Primes.NearestTo(3711));
      Assert.AreEqual(5059,  Primes.NearestTo(5066));
      Assert.AreEqual(5077,  Primes.NearestTo(5068));
      Assert.AreEqual(5077,  Primes.NearestTo(5070));
    }

    [Test]
    public static void FindGoodHashPrimes()
    {
      var bob = new System.Text.StringBuilder();

      foreach (int prime in Hashing.HashPrimes)
      {
        bool good = true;

        foreach (int value in Primes.ConvenientPrimes)
        {
          if ((value - 1) % prime == 0)
          {
            good = false;
            break;
          }
        }

        if (good)
        {
          bob.Append(prime).AppendLine();
        }
      }

      Assert.Greater(bob.Length, 0);

      bob.Insert(0, "FOUND GOOD PRIMES:\n");
      Debug.Log(bob.ToString());
    }

    [Test]
    public static void HashCollisionRatio()
    {
      Assert.Inconclusive("Test goals TBD");
    }

  }
}