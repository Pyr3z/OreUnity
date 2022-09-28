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
  public struct HashMapParams
  {

    #region Constants + Defaults

    private const int USERCAPACITY_DEFAULT = 5;

    private const int INTERNALSIZE_DEFAULT = 7;
    private const int INTERNALSIZE_MIN     = Primes.MinValue;
    private const int INTERNALSIZE_MAX     = Primes.MaxConvenientValue;

    private const float LOADFACTOR_DEFAULT = 0.72f;
    private const float LOADFACTOR_MIN     = 0.1f;
    private const float LOADFACTOR_MAX     = 1f;

    private const float GROWFACTOR_DEFAULT = 2f;
    private const float GROWFACTOR_MIN     = 1.1f;
    private const float GROWFACTOR_MAX     = 3f;

    private const int HASHPRIME_DEFAULT    = 53; // Microsoft uses 101

    private const bool ISFIXED_DEFAULT     = false;


    [PublicAPI]
    public static readonly HashMapParams Default = new HashMapParams(USERCAPACITY_DEFAULT);

    #endregion Constants + Defaults


    #region Properties + Fields

    public int UserCapacity => CalcLoadLimit(m_InternalSize);

    public bool IsFixedSize => m_GrowFactor < GROWFACTOR_MIN;


    [SerializeField]
    private int   m_InternalSize;
    [SerializeField]
    private float m_LoadFactor;
    [SerializeField]
    private float m_GrowFactor;
    [SerializeField]
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

      m_InternalSize = Primes.Next((int)(initialCapacity / m_LoadFactor), m_HashPrime);
    }

    #endregion Constructors + Factory Funcs


    #region Methods

    public bool Check()
    {
      return  (INTERNALSIZE_MIN <= m_InternalSize && m_InternalSize <= INTERNALSIZE_MAX) &&
              (LOADFACTOR_MIN <= m_LoadFactor && m_LoadFactor <= LOADFACTOR_MAX)         &&
              (m_GrowFactor <= GROWFACTOR_MAX)                                           &&
              (m_HashPrime == HASHPRIME_DEFAULT || Primes.IsPrime(m_HashPrime));
    }

    public void ResetGrowth()
    {
      m_InternalSize = INTERNALSIZE_DEFAULT;
    }

    public int SetUserCapacity(int userCapacity)
    {
      return m_InternalSize = CalcInternalSize(userCapacity);
    }


    public int MakeBuckets<T>(int userCapacity, out T[] buckets)
    {
      userCapacity = CalcInternalSize(userCapacity);
      buckets = new T[userCapacity];
      return CalcLoadLimit(userCapacity);
    }

    public int CalcInternalSize(int userCapacity)
    {
      return Primes.Next((int)(userCapacity / m_LoadFactor), m_HashPrime);
    }

    public int CalcLoadLimit(int internalSize)
    {
      return (int)(m_LoadFactor * internalSize);
    }

    public int CalcJump(int hash31, int size)
    {
      return 1 + (hash31 * m_HashPrime & int.MaxValue) % (size - 1);
    }

    public int CalcNextSize(int prevSize, int maxSize = Primes.MaxValue)
    {
      if (m_GrowFactor <= 1f || m_HashPrime < Primes.MinValue)
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