/*! @file       Runtime/HashMap.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-09-28
**/

using System.Collections.Generic;
using System.Collections;
using UnityEngine;


namespace Ore
{

  [System.Serializable] // only *actually* serializable if subclassed!
  public partial class HashMap<TKey,TValue>
  {

    #region Fields

    [SerializeField] // the only serializable field in this class
    protected HashMapParams m_Params = HashMapParams.Default;

    private int m_Count, m_Collisions, m_LoadLimit;
    private int m_Version;

    protected Bucket[] m_Buckets;

    protected IEqualityComparer         m_KeyComparator   = HashKeyComparator.Default;
    protected IEqualityComparer<TValue> m_ValueComparator = EqualityComparer<TValue>.Default;

    #endregion


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


    protected void MakeBuckets()
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(out m_Buckets);
    }
    protected void MakeBuckets(int userCapacity)
    {
      m_Count = m_Collisions = 0;
      m_LoadLimit = m_Params.MakeBuckets(userCapacity, out m_Buckets);
    }

  } // end partial class HashMap

}