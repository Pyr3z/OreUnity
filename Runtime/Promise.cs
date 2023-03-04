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
    public delegate void SuccessAction(T value);
    public delegate void FailureAction([CanBeNull] T flotsam, [CanBeNull] Exception ex);
    private static void DefaultFailureAction(T flotsam, Exception ex)
    {
      Orator.NFE(ex);
    }


    public T    Value       => m_Value;
    public bool IsCompleted => m_State > State.Pending;
    public bool Forgotten   => m_State == State.Forgotten;
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

    public void Forget()
    {
      if (m_State < State.Forgotten)
      {
        m_State = State.Forgotten;
      }
    }

    public void Fail()
    {
      if (m_State > State.Failed)
      {
        Orator.Error<Promise<T>>("Cannot fail a Promise once it's already completed!");
        return;
      }

      m_State = State.Failed;
    }

    public void FailWith(Exception ex)
    {
      if (m_State > State.Failed)
      {
        Orator.Error<Promise<T>>("Cannot fail a Promise once it's already completed! (gonna throw something faux:)");
        Orator.NFE(ex.Silenced());
        return;
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

      if (m_OnFailed is null)
      {
        m_OnFailed = DefaultFailureAction;
      }
    }


    public static implicit operator T (Promise<T> promise)
    {
      if (promise is null)
        return default;
      return promise.m_Value;
    }


    // private section

    private enum State
    {
      Pending,
      Failed,
      Forgotten,
      Succeeded,
    }

    private T         m_Value;
    private State     m_State;
    private Exception m_Exception;

    private SuccessAction m_OnSucceeded;
    private FailureAction m_OnFailed;


  #region IEnumerator interface

    object IEnumerator.Current => null;

    bool IEnumerator.MoveNext()
    {
      switch (m_State)
      {
        default:
        case State.Pending:
          return true;

        case State.Forgotten:
          m_Value = default;
          break;

        case State.Succeeded:
          m_OnSucceeded?.Invoke(m_Value);
          break;

        case State.Failed:
          m_OnFailed?.Invoke(m_Value, m_Exception);
          m_Exception = null;
          break;
      }

      // release delegate handles:
      m_OnSucceeded = null;
      m_OnFailed    = null;

      return false;
    }

    public void Reset()
    {
      m_Value       = default;
      m_State       = State.Pending;
      m_Exception   = null;
      m_OnSucceeded = null;
      m_OnFailed    = null;
    }

  #endregion IEnumerator interface

  } // end class Promise
}