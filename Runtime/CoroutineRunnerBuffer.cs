/*! @file       Runtime/CoroutineRunnerBuffer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-06
**/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;


namespace Ore
{
  public sealed class CoroutineRunnerBuffer : ICoroutineRunner, IEnumerable<(IEnumerator routine, object key)>
  {
    public int Count => m_Queue.Count;


    readonly List<(IEnumerator routine, object key)> m_Queue = new List<(IEnumerator routine, object key)>();


    public IEnumerator<(IEnumerator routine, object key)> GetEnumerator()
      => m_Queue.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()
      => m_Queue.GetEnumerator();


    public void Run(IEnumerator routine, Object key)
    {
      m_Queue.Add((routine,key));
    }

    public void Run(IEnumerator routine, string key)
    {
      m_Queue.Add((routine,key));
    }

    public void Run(IEnumerator routine, out string guidKey)
    {
      guidKey = Strings.MakeGUID();
      m_Queue.Add((routine,guidKey));
    }

    public void Run(IEnumerator routine)
    {
      m_Queue.Add((routine,this));
    }

    public void Halt(object key)
    {
      int i = m_Queue.Count;
      while (i --> 0)
      {
        if (Equals(m_Queue[i].Item2, key))
        {
          m_Queue.RemoveAt(i);
        }
      }
    }

    public void HaltAll()
    {
      m_Queue.Clear();
    }


    public void Adopt(CoroutineRunnerBuffer other)
    {
      if (ReferenceEquals(this, other))
        return;

      m_Queue.Capacity = m_Queue.Count + other.m_Queue.Count + 3;

      foreach (var item in other.m_Queue)
      {
        m_Queue.Add(item);
      }

      other.m_Queue.Clear();
    }

  } // end class CoroutineRunnerBuffer
}