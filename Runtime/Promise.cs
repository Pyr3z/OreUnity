/*! @file       Runtime/Promise.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using System.Collections;

using Exception = System.Exception;


namespace Ore
{
  [PublicAPI]
  public class Promise<T> : IEnumerator
  {
    public T Value => m_Value;


    public Future<T> GetFuture()
    {
      return new Future<T>(this);
    }

    public void CompleteWith(T value)
    {
      if (m_State < State.Completed)
      {
        m_Value = value;
        m_State = State.Completed;
      }
    }

    public void Complete()
    {
      if (m_State < State.Completed)
      {
        m_State = State.Completed;
      }
    }

    public void Throw(Exception ex = null)
    {
      m_State = State.Errored;

      if (ex is null)
      {
        ex = new UnanticipatedException($"Promise<{typeof(T).Name}>");
      }

      if (m_Exception is null)
      {
        m_Exception = ex;
      }
      else
      {
        m_Exception = MultiException.Create(ex, m_Exception);
      }
    }


    // private section

    private enum State
    {
      Pending,
      Completed,
      Errored
    }

    private T         m_Value;
    private State     m_State;
    private Exception m_Exception;


    #region IEnumerator interface

    object IEnumerator.Current => null;

    bool IEnumerator.MoveNext()
    {
      if (m_Exception != null)
      {
        Orator.NFE(m_Exception);
        m_Exception = null;
      }

      return m_State == State.Pending;
    }

    void IEnumerator.Reset()
      => throw new System.NotSupportedException(nameof(IEnumerator.Reset));

    #endregion IEnumerator interface

  } // end class Promise
}