/*! @file       Runtime/CoroutineQueue.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-06
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace Ore
{
  public sealed class CoroutineQueue : ICoroutineRunner, IEnumerable<(IEnumerator routine, object key)>
  {
    private List<(IEnumerator routine, object key)> m_Queue;

    public bool IsEmpty => m_Queue is null || m_Queue.Count == 0;
    public IEnumerator<(IEnumerator routine, object key)> GetEnumerator()
    {
      return m_Queue.GetEnumerator();
    }
    public void Clear()
    {
      m_Queue = null;
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    public void EnqueueCoroutine(IEnumerator routine, Object key)
    {
      m_Queue ??= new List<(IEnumerator, object)>();
      m_Queue.Add((routine,key));
    }

    public void EnqueueCoroutine(IEnumerator routine, string key)
    {
      m_Queue ??= new List<(IEnumerator, object)>();
      m_Queue.Add((routine,key));
    }

    public void EnqueueCoroutine(IEnumerator routine, out string key)
    {
      key = Strings.MakeGUID();
      EnqueueCoroutine(routine, key);
    }

    public void EnqueueCoroutine(IEnumerator routine)
    {
      EnqueueCoroutine(routine, out _ );
    }

    public void CancelCoroutinesFor(object key)
    {
      if (m_Queue is null)
        return;

      int i = m_Queue.Count;
      while (i --> 0)
      {
        if (m_Queue[i].Item2 == key)
        {
          m_Queue.RemoveAt(i);
        }
      }
    }

    public void CancelAllCoroutines()
    {
      m_Queue = null;
    }

  } // end class CoroutineQueue
}