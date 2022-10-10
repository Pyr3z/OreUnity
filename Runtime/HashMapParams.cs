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

  #region Constants + Defaults

    private const int USERCAPACITY_DEFAULT = 5;

    private const int INTERNALSIZE_MIN     = Primes.MinValue;
    private const int INTERNALSIZE_MAX     = Primes.MaxConvenientValue;

    private const float LOADFACTOR_DEFAULT = 0.72f;
    private const float LOADFACTOR_MIN     = 0.1f * LOADFACTOR_DEFAULT;
    private const float LOADFACTOR_MAX     = 1f;    // danger

    private const float GROWFACTOR_DEFAULT = 2f;
    private const float GROWFACTOR_MIN     = 1.1f;
    private const float GROWFACTOR_MAX     = 3f;

    private const int HASHPRIME_DEFAULT    = 97; // see Static/Hashing.cs for candidates

    private const bool ISFIXED_DEFAULT     = false;


    [PublicAPI]
    public static readonly HashMapParams Default = new HashMapParams(USERCAPACITY_DEFAULT);

  #endregion Constants + Defaults


  #region Properties + Fields

    public int  RehashThreshold => m_HashPrime - 1;
    public bool IsFixedSize     => m_GrowFactor < GROWFACTOR_MIN;


    [SerializeField, Range(INTERNALSIZE_MIN, INTERNALSIZE_MAX)]
    private int   m_InitialSize;
    [SerializeField, Range(LOADFACTOR_MIN, LOADFACTOR_MAX)]
    private float m_LoadFactor;
    [SerializeField, Range(0f, GROWFACTOR_MAX)]
    private float m_GrowFactor;
    [SerializeField, Min(53)]
    private int   m_HashPrime;

  #endregion Properties + Fields


  #region Constructors + Factory Funcs

    [PublicAPI]
    public static HashMapParams NoAlloc(int userCapacity, float loadFactor = LOADFACTOR_DEFAULT)
    {
      return new HashMapParams(
        initialCapacity: userCapacity,
        isFixed:         true,
        loadFactor:      loadFactor,
        hashPrime:       Primes.Next((int)(userCapacity / loadFactor)));
    }

    [PublicAPI]
    public HashMapParams(
      int   initialCapacity, // "user" capacity, not physical capacity.
      bool  isFixed    = ISFIXED_DEFAULT,
      float loadFactor = LOADFACTOR_DEFAULT,
      float growFactor = GROWFACTOR_DEFAULT,
      int   hashPrime  = HASHPRIME_DEFAULT)
    {
      if (isFixed)
        m_GrowFactor = 1f;
      else if (growFactor < GROWFACTOR_MIN)
        m_GrowFactor = GROWFACTOR_MIN;
      else if (growFactor > GROWFACTOR_MAX)
        m_GrowFactor = GROWFACTOR_MAX;
      else
        m_GrowFactor = growFactor;

      if (hashPrime != HASHPRIME_DEFAULT)
      {
        hashPrime &= int.MaxValue;

        if (!Primes.IsPrime(hashPrime))
        {
          hashPrime = Primes.Next(hashPrime);
        }
      }

      m_HashPrime = hashPrime;

      m_LoadFactor = loadFactor.Clamp(LOADFACTOR_MIN, LOADFACTOR_MAX);

      m_InitialSize = Primes.Next((int)(initialCapacity / m_LoadFactor), m_HashPrime);
    }

  #endregion Constructors + Factory Funcs


  #region Methods

    public bool Check()
    {
      return  (INTERNALSIZE_MIN <= m_InitialSize && m_InitialSize <= INTERNALSIZE_MAX) &&
              (LOADFACTOR_MIN <= m_LoadFactor && m_LoadFactor <= LOADFACTOR_MAX)       &&
              (m_GrowFactor <= GROWFACTOR_MAX)                                         &&
              (m_HashPrime == HASHPRIME_DEFAULT || Primes.IsPrime(m_HashPrime));
    }

    public void ResetGrowth()
    {
      m_InitialSize = CalcInternalSize(USERCAPACITY_DEFAULT);
    }

    public int StoreLoadLimit(int loadLimit)
    {
      return m_InitialSize = CalcInternalSize(loadLimit);
    }


    public int MakeBuckets<T>(out T[] buckets)
    {
      buckets = new T[m_InitialSize];
      return CalcLoadLimit(m_InitialSize);
    }

    public int CalcInternalSize(int loadLimit)
    {
      loadLimit = (int)(loadLimit / m_LoadFactor); // saving on stacksize; this is now internal size

      if ((loadLimit - 1) % m_HashPrime != 0 && Primes.IsPrime(loadLimit))
      {
        return loadLimit;
      }

      return Primes.Next(loadLimit, m_HashPrime);
    }

    public int CalcLoadLimit(int internalSize) // AKA User Capacity
    {
      // without the rounding, we get wonky EnsureCapacity behavior
      return (int)(internalSize * m_LoadFactor + 0.5f);
    }

    public int CalcJump(int hash31, int size)
    {
      return 1 + (hash31 * m_HashPrime & int.MaxValue) % (size - 1);
    }

    public int CalcNextSize(int prevSize, int maxSize = Primes.MaxValue)
    {
      if (m_GrowFactor <= 1f)
        return prevSize;

      if ((int)(maxSize / m_GrowFactor) < prevSize)
        return maxSize;

      return Primes.Next((int)(prevSize * m_GrowFactor), m_HashPrime);
    }

  #endregion Methods


    public static implicit operator HashMapParams (int initialCapacity)
    {
      return new HashMapParams(initialCapacity);
    }

  } // end class HashMapParams
}