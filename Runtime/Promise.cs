/*! @file       Runtime/Promise.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using System.Collections;

using Exception   = System.Exception;
using IDisposable = System.IDisposable;


namespace Ore
{
  [PublicAPI]
  public class Promise<T> : IEnumerator, IDisposable
  {
    public delegate void SuccessAction(T value);
    public delegate void FailureAction([CanBeNull] T flotsam, [CanBeNull] Exception ex);

    private static void DefaultFailureAction(T flotsam, Exception ex)
    {
      Orator.NFE(ex);
    }


    public Promise(bool squelchDefaultFailAction = false)
    {
      if (!squelchDefaultFailAction)
      {
        m_OnFailed = DefaultFailureAction;
      }
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


    public Promise<T> Maybe(T value)
    {
      if (m_State == State.Pending)
      {
        m_Value = value;
      }

      return this;
    }

    public Promise<T> Complete()
    {
      if (m_State == State.Pending)
      {
        m_State = State.Succeeded;
      }

      return this;
    }

    public Promise<T> CompleteWith(T value)
    {
      if (m_State == State.Pending)
      {
        m_Value = value;
        m_State = State.Succeeded;
      }

      return this;
    }

    public Promise<T> UpdateValue(T value)
    {
      if (m_State == State.Succeeded)
      {
        m_Value = value;
      }

      return this;
    }

    public Promise<T> Forget()
    {
      // can forget from any state~
      m_State = State.Forgotten;
      return this;
    }

    public Promise<T> Fail()
    {
      if (m_State == State.Succeeded)
      {
        Orator.Error<Promise<T>>("Cannot fail a Promise once it's already completed!");
      }
      else
      {
        m_State = State.Failed;
      }

      return this;
    }

    public Promise<T> FailWith(T flotsam)
    {
      if (m_State == State.Succeeded)
      {
        Orator.Error<Promise<T>>("Cannot fail a Promise once it's already completed!");
      }
      else
      {
        m_Value = flotsam;
        m_State = State.Failed;
      }

      return this;
    }

    public Promise<T> FailWith(Exception ex)
    {
      if (m_State == State.Succeeded)
      {
        Orator.Error<Promise<T>>("Cannot fail a Promise once it's already completed! (gonna throw something faux:)");
        Orator.NFE(ex.Silenced());
      }
      else
      {
        m_State = State.Failed;

        if (ex != null)
        {
          if (m_Exception is null)
          {
            m_Exception = ex;
          }
          else if (m_Exception != ex)
          {
            m_Exception = MultiException.Create(m_Exception, ex);
          }
        }
      }

      return this;
    }

    public void SquelchDefaultFailureAction()
    {
      if (m_OnFailed != null)
      {
        m_OnFailed -= DefaultFailureAction;
      }
    }


    public override string ToString()
    {
      return $"{m_Value}";
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


  #region interfaces

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
          break;
      }

      m_Exception   = null;
      m_OnSucceeded = null;
      m_OnFailed    = null;

      return false;
    }

    public void Reset()
    {
      if (m_Value is IDisposable disposable)
      {
        disposable.Dispose();
      }

      m_Value       = default;
      m_State       = State.Pending;
      m_Exception   = null;
      m_OnSucceeded = null;
      m_OnFailed    = null;
    }

    void IDisposable.Dispose()
    {
      // invoke MoveNext just in case we haven't invoked our callbacks yet:
      _ = ((IEnumerator)this).MoveNext();
      Reset();
    }

  #endregion interfaces

  } // end class Promise
}