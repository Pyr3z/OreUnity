/*! @file       Runtime/Promise.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using System.Collections;

using Exception = System.Exception;
using InvalidAsynchronousStateException = System.ComponentModel.InvalidAsynchronousStateException;


namespace Ore
{
  [PublicAPI]
  public class Promise<T> : IEnumerator
  {
    public delegate void SuccessAction(T value);
    public delegate void FailureAction([CanBeNull] T value, [CanBeNull] Exception ex);
    private static void DefaultFailureAction(T value, Exception ex)
    {
      if (ex != null)
        Orator.NFE(ex);
    }


    public T    Value       => m_Value;
    public bool IsCompleted => m_State > State.Pending;
    public bool Succeeded   => m_State == State.Succeeded;
    public bool Failed      => m_State == State.Failed;

    public event SuccessAction OnSucceeded
    {
      add
      {
        switch (m_State)
        {
          case State.Pending:
            m_OnSucceeded += value;
            break;
          case State.Succeeded:
            value?.Invoke(m_Value); // TODO behaves differently than if init'd early?
            break;
        }
      }
      remove => m_OnSucceeded -= value;
    }

    public event FailureAction OnFailed
    {
      add
      {
        switch (m_State)
        {
          case State.Pending:
            m_OnFailed += value;
            break;
          case State.Failed:
            value?.Invoke(m_Value, m_Exception);
            break;
        }
      }
      remove => m_OnFailed -= value;
    }


    public Future<T> GetFuture()
    {
      return new Future<T>(this);
    }

    public void Maybe(T value)
    {
      if (m_State == State.Pending)
      {
        m_Value = value;
      }
    }

    public void Complete()
    {
      if (m_State == State.Pending)
      {
        m_State = State.Succeeded;
      }
    }

    public void CompleteWith(T value)
    {
      if (m_State == State.Pending)
      {
        m_Value = value;
        m_State = State.Succeeded;
      }
    }

    public void Fail()
    {
      if (m_State == State.Succeeded)
      {
        throw new InvalidAsynchronousStateException("Cannot fail a Promise once it's already completed!");
      }

      m_State = State.Failed;
    }

    public void FailWith(Exception ex)
    {
      if (m_State == State.Succeeded)
      {
        throw new InvalidAsynchronousStateException("Cannot fail a Promise once it's already completed!", ex);
      }

      m_State = State.Failed;

      if (ex is null)
        return;

      if (m_Exception is null)
      {
        m_Exception = ex;
      }
      else if (m_Exception != ex)
      {
        m_Exception = MultiException.Create(m_Exception, ex);
      }
    }


    public static implicit operator T (Promise<T> promise)
    {
      if (promise is null || !promise.Succeeded)
        return default;
      return promise.m_Value;
    }


    // private section

    private enum State
    {
      Pending,
      Succeeded,
      Failed
    }

    private T         m_Value;
    private State     m_State;
    private Exception m_Exception;

    private SuccessAction m_OnSucceeded;
    private FailureAction m_OnFailed = DefaultFailureAction;


  #region IEnumerator interface

    object IEnumerator.Current => null;

    bool IEnumerator.MoveNext()
    {
      switch (m_State)
      {
        default:
        case State.Pending:
          return true;

        case State.Succeeded:
          m_OnSucceeded?.Invoke(m_Value);
          break;

        case State.Failed:
          m_OnFailed?.Invoke(m_Value, m_Exception);
          break;
      }

      // release delegate handles:
      m_OnSucceeded = null;
      m_OnFailed    = null;

      return false;
    }

    void IEnumerator.Reset()
      => throw new System.NotSupportedException("Promise<T>.Reset()");

  #endregion IEnumerator interface

  } // end class Promise
}