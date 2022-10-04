/*! @file       Runtime/HashMap+Enumerator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-04
**/

using System.Collections.Generic;
using System.Collections;

using JetBrains.Annotations;


namespace Ore
{
  public partial class HashMap<TKey,TValue>
  {
    public struct Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>, IEnumerator<(TKey key, TValue val)>, IDictionaryEnumerator
    {
      public (TKey key, TValue val) Current => (m_Bucket.Key,m_Bucket.Value);

      KeyValuePair<TKey,TValue> IEnumerator<KeyValuePair<TKey,TValue>>.Current => m_Bucket.GetPair();

      object IEnumerator.Current => (m_Bucket.Key, m_Bucket.Value);

      DictionaryEntry IDictionaryEnumerator.Entry => new DictionaryEntry(m_Bucket.Key, m_Bucket.Value);

      object IDictionaryEnumerator.Key => m_Bucket.Key;

      object IDictionaryEnumerator.Value => m_Bucket.Value;


      private HashMap<TKey,TValue> m_Parent;

      private Bucket m_Bucket;

      private int m_Pos, m_Count, m_Version;


      public Enumerator([NotNull] HashMap<TKey,TValue> forMap)
      {
        m_Parent  = forMap;
        m_Pos     = forMap.m_Buckets.Length;
        m_Count   = forMap.m_Count;
        m_Version = forMap.m_Version;
        m_Bucket  = default;
      }


      public bool MoveNext()
      {
        if (--m_Count < 0 || --m_Pos < 0)
        {
          return false;
        }

        if (m_Version != m_Parent.m_Version)
        {
          throw new System.InvalidOperationException("HashMap was modified while iterating through it.");
        }

        do
        {
          m_Bucket = m_Parent.m_Buckets[m_Pos];
        }
        while (m_Bucket.IsFree(m_Parent.m_KeyComparator) && m_Pos --> 0);

        return m_Pos >= 0;
      }

      public void Reset()
      {
        if (m_Parent is null)
        {
          throw new System.InvalidOperationException("HashMap.Enumerator.Reset() cannot be called after disposal.");
        }

        m_Pos     = m_Parent.m_Buckets.Length;
        m_Count   = m_Parent.m_Count;
        m_Version = m_Parent.m_Version;
        m_Bucket  = default;
      }

      public void Dispose()
      {
        m_Parent = null;
      }

    } // end struct Enumerator
  } // end partial class HashMap
}