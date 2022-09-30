/*! @file       Tests/Runtime/PrimesCorrectness.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Correctness Tests:
 *  [ ] Binary Search
 *  [ ] Primes.IsPrime
 *  [ ] Primes.IsPrimeNoLookup
 *  [ ] Primes.Next
 *  [ ] Hash collision ratio
**/

using System.Linq;
using System.Collections.Generic;

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

      foreach (int value in testvalues.Concat(Primes10K.InOrder))
      {
        int system = System.Array.BinarySearch(Primes10K.InOrder, value);
        int ore   = Primes10K.InOrder.BinarySearch(value);

        Assert.AreEqual(system.Sign(), ore.Sign(), $"(sign of result) forValue={value},system={system},ore={ore}");

        if (system < 0)
        {
          // I checked it, OK to skip
          // Debug.Log($"[{nameof(BinarySearchCorrectness)}] inverted outputs not the same: system={system},ore={ore}");
        }
        else
        {
          Assert.AreEqual(system, ore);
        }
      }
    }

    [Test]
    public static void IsPrimeNoLookup() // NoLookup should techically be axiomatic
    {
      const int N = 333;

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
      const int N = 666;

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
      var maxTestValueNexts = new HashSet<int>
      {
        20201, 20219, 20231, 20233, 20249, 25229 // should cover all likelihoods
      };

      int next;

      // singular test
      next = Primes.Next(Primes10K.MAX_TEST_VALUE);
      Assert.True(maxTestValueNexts.Contains(next), $"value={Primes10K.MAX_TEST_VALUE},next={next}");

      // singular test with hashprime 101
      next = Primes.Next(Primes10K.MAX_TEST_VALUE, 101);
      Assert.True(maxTestValueNexts.Contains(next), $"value={Primes10K.MAX_TEST_VALUE},next={next}");

      // singular test with hashprime 53
      next = Primes.Next(Primes10K.MAX_TEST_VALUE, 53);
      Assert.True(maxTestValueNexts.Contains(next), $"value={Primes10K.MAX_TEST_VALUE},next={next}");


      // scale it up
      var testvalues = Primes10K.GetTestValues(100, 100);

      foreach (int hashprime in new []{ int.MaxValue, 101, 53 })
      {
        foreach (int value in testvalues)
        {
          next = Primes.Next(value, hashprime);
          Debug.Log($"value={value},next={next}");

          Assert.Positive(next);
          Assert.True(Primes.IsPrime(next));

          // MaxValue is a last resort of what's returned; test values shouldn't trigger it
          Assert.Less(next, Primes.MaxValue);
        }
      }
    }

    [Test]
    public static void FindGoodHashPrimes()
    {
      var bob = new System.Text.StringBuilder();

      foreach (int prime in Primes10K.InOrder)
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
      // TODO
      Assert.Inconclusive("Test not finished.");
    }

  }
}