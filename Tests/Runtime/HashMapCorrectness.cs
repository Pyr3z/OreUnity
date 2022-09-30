/*! @file       Tests/Runtime/HashMapCorrectness.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-29
 *
 *  Correctness Tests:
 *  [ ] HashMapParams.Default
 *  [ ] HashMap.Contains
 *  [ ] HashMap.Map
 *  [ ] HashMap.Unmap
 *  [ ] HashMap.EnsureCapacity (empty)
 *  [ ] HashMap.EnsureCapacity (non-empty)
**/

using NUnit.Framework;


namespace Ore.Tests
{
  public static class HashMapCorrectness
  {

    [Test]
    public static void DefaultParameters()
    {
      int defaultHashPrime = HashMapParams.Default.RehashThreshold + 1;

      foreach (int hashprime in new []{ defaultHashPrime })
      {
        foreach (int value in Primes.ConvenientPrimes)
        {
          Assert.NotZero((value - 1) % hashprime, $"hashprime={hashprime},value={value}");
        }
      }
    }

    [Test]
    public static void Contains()
    {
      // Use the Assert class to test conditions
    }

    [Test]
    public static void Map()
    {

    }

    [Test]
    public static void Unmap()
    {

    }

    [Test]
    public static void EnsureCapacityEmpty()
    {

    }

    [Test]
    public static void EnsureCapacityNonEmpty()
    {

    }

  }
}
