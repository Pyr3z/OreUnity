/*! @file       Runtime/HashMapParams.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
 *
 *  A parameter POD storing and modifying performance-critical HashMap
 *  configurations.
**/

using JetBrains.Annotations;

using UnityEngine;


namespace Ore
{
  [System.Serializable]
  public struct HashMapParams
  {

    public enum CollisionPolicy
    {
      Default,

      HashProbing = Default,
      LinearProbing,
      QuadraticProbing,

      RobinHoodHashing,
      HopscotchHashing,
      CuckooHashing,

      SeparateChaining,
      ParallelChaining,
    }


  #region Constants + Defaults

    [PublicAPI]
    public static readonly HashMapParams Default = new HashMapParams(USERCAPACITY_DEFAULT);


    private const int USERCAPACITY_DEFAULT = 5;

    private const int HASHPRIME_DEFAULT    = Hashing.DefaultHashPrime;

    private const int INTERNALSIZE_MIN     = Primes.MinValue;
    private const int INTERNALSIZE_MAX     = Primes.MaxSizePrime;

    private const float LOADFACTOR_DEFAULT = 0.72f;
    private const float LOADFACTOR_MIN     = 0.1f * LOADFACTOR_DEFAULT;
    private const float LOADFACTOR_MAX     = 1f;    // danger

    private const float GROWFACTOR_DEFAULT = 2f;
    private const float GROWFACTOR_MIN     = 1.1f;
    private const float GROWFACTOR_MAX     = 4f;

  #endregion Constants + Defaults


  #region Properties + Fields

    public bool IsFixedSize => GrowFactor < GROWFACTOR_MIN;


    [SerializeField, Range(INTERNALSIZE_MIN, INTERNALSIZE_MAX)]
    public int InitialSize;

    [SerializeField, Range(LOADFACTOR_MIN, LOADFACTOR_MAX)]
    public float LoadFactor;

    [SerializeField, Range(0f, GROWFACTOR_MAX)]
    public float GrowFactor;

    [SerializeField, Min(53)]
    public int HashPrime;

    [SerializeField]
    public CollisionPolicy Policy;

  #endregion Properties + Fields


  #region Constructors + Factory Funcs

    [PublicAPI]
    public static HashMapParams NoAlloc(int userCapacity, float loadFactor = LOADFACTOR_DEFAULT)
    {
      return new HashMapParams(
        initialCapacity: userCapacity,
        growFactor:      0f,
        loadFactor:      loadFactor,
        hashPrime:       Primes.Next((int)(userCapacity / loadFactor)));
    }

    /// <summary>
    /// You should only need to instantiate this struct if you're trying to play
    /// around with or optimize a particular HashMap instance.
    /// </summary>
    /// <param name="initialCapacity">
    /// Interpreted as initial "user" capacity, not physical capacity.
    /// </param>
    /// <param name="loadFactor"></param>
    /// <param name="growFactor"></param>
    /// <param name="hashPrime"></param>
    /// <param name="collisionPolicy"></param>
    [PublicAPI]
    public HashMapParams(
      int             initialCapacity,
      float           loadFactor      = LOADFACTOR_DEFAULT,
      float           growFactor      = GROWFACTOR_DEFAULT,
      int             hashPrime       = HASHPRIME_DEFAULT,
      CollisionPolicy collisionPolicy = CollisionPolicy.Default)
    {
      if (growFactor < Floats.EPSILON)
        GrowFactor = 1f;
      else if (growFactor < GROWFACTOR_MIN)
        GrowFactor = GROWFACTOR_MIN;
      else if (growFactor > GROWFACTOR_MAX)
        GrowFactor = GROWFACTOR_MAX;
      else
        GrowFactor = growFactor;

      HashPrime = Primes.NearestTo(hashPrime & int.MaxValue);

      LoadFactor = loadFactor.Clamp(LOADFACTOR_MIN, LOADFACTOR_MAX);

      InitialSize = Primes.Next((int)(initialCapacity / LoadFactor), HashPrime);

      Policy = CollisionPolicy.Default;
    }

  #endregion Constructors + Factory Funcs


  #region Methods

    public bool Check()
    {
      // ReSharper is saying the next line of code is always true, but I disagree.
      // Just consider if this struct was default constructed, and InitialSize = 0...
      return  (INTERNALSIZE_MIN <= InitialSize && InitialSize <= INTERNALSIZE_MAX) &&
              (LOADFACTOR_MIN <= LoadFactor && LoadFactor <= LOADFACTOR_MAX)       &&
              (GrowFactor <= GROWFACTOR_MAX)                                       &&
              (HashPrime == HASHPRIME_DEFAULT || Primes.IsPrime(HashPrime));
    }

    public void ResetGrowth()
    {
      InitialSize = CalcInternalSize(USERCAPACITY_DEFAULT);
    }

    public int StoreLoadLimit(int loadLimit)
    {
      return InitialSize = CalcInternalSize(loadLimit);
    }


    public int MakeBuckets<T>(out T[] buckets)
    {
      buckets = new T[InitialSize];
      return CalcLoadLimit(InitialSize);
    }

    public int CalcInternalSize(int loadLimit)
    {
      return Primes.NextHashableSize((int)(loadLimit / LoadFactor), HashPrime, 0);
    }

    public int CalcLoadLimit(int internalSize) // AKA User Capacity
    {
      // without the rounding, we get wonky EnsureCapacity behavior
      return (int)(internalSize * LoadFactor + 0.5f);
    }

    public int CalcJump(int hash31, int size)
    {
      return 1 + (hash31 * HashPrime & int.MaxValue) % (size - 1);
    }

    public int CalcNextSize(int prevSize, int maxSize = Primes.MaxValue)
    {
      if (GrowFactor <= 1f)
        return prevSize;

      if ((int)(maxSize / GrowFactor) < prevSize)
        return maxSize;

      return Primes.Next((int)(prevSize * GrowFactor), HashPrime);
    }

  #endregion Methods


    public static implicit operator HashMapParams (int initialCapacity)
    {
      return new HashMapParams(initialCapacity);
    }

  } // end class HashMapParams
}