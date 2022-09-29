/*! @file       Runtime/HashMap.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections.Generic;
using System.Collections;

using JetBrains.Annotations;

using UnityEngine;

using Type = System.Type;


namespace Ore
{

  [System.Serializable] // only *actually* serializable if subclassed!
  public partial class HashMap<TKey,TValue>
  {

    #region Properties

    [PublicAPI]
    public Type KeyType   => typeof(TKey);
    [PublicAPI]
    public Type ValueType => typeof(TValue);

    [PublicAPI]
    public int Count => m_Count;

    [PublicAPI]
    public int Capacity
    {
      get => m_LoadLimit;
      set => _ = EnsureCapacity(value);
    }

    public HashMapParams Parameters => m_Params;
    public int Version => m_Version;

    #endregion Properties


    #region Fields

    [SerializeField] // the only serializable field in this class
    protected HashMapParams m_Params = HashMapParams.Default;

    protected int m_Count, m_Collisions, m_LoadLimit;
    protected int m_Version;

    protected Bucket[] m_Buckets;

    protected IEqualityComparer<TKey>   m_KeyComparator   = HashKeyComparator<TKey>.Default;
    protected IEqualityComparer<TValue> m_ValueComparator = EqualityComparer<TValue>.Default;

    #endregion


    #region Constructors

    public HashMap()
    {
      MakeBuckets();
    }

    public HashMap(HashMapParams parms)
    {
      if (parms.Check())
      {
        m_Params = parms;
      }
      else
      {
        Orator.Warn("Bad HashMapParams passed into ctor.");
      }

      MakeBuckets();
    }

    #endregion Constructors


    #region Public Methods

    public bool EnsureCapacity(int userCapacity)
    {
      OAssert.False(userCapacity < 0, "provided negative userCapacity");

      if (!m_Params.IsFixedSize && userCapacity > m_LoadLimit)
      {
        Rehash(m_Params.SetUserCapacity(userCapacity));
      }

      return m_LoadLimit >= userCapacity;
    }

    #endregion Public Methods


    #region Internal Methods

    private void MakeBuckets()
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(out m_Buckets);
    }
    private void MakeBuckets(int userCapacity)
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(userCapacity, out m_Buckets);
    }

    private int FindBucket(TKey key, out Bucket bucket)
    {
      bucket = default;
      if (key is null || m_Count == 0)
      {
        return -1;
      }

      var (hash31, jump) = CalcHashJump(key);

      int i = hash31 % m_Buckets.Length;
      int jumps = 0;
      int found = 0;

      // TODO

      return -1;
    }

    private (int hash31, int jump) CalcHashJump(TKey key)
    {
      var result = (m_KeyComparator.GetHashCode(key) & int.MaxValue, 0);
      result.Item2 = m_Params.CalcJump(result.Item1, m_Buckets.Length);
      return result;
    }

    private int BucketEquals(int i, int hash31, TKey key)
    {
      // TODO

      return -1;
    }

    private void Rehash(int newSize)
    {
      // TODO
    }

    #endregion Internal Methods

  } // end partial class HashMap

}