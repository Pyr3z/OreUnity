/*! @file       Runtime/Promise.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using JetBrains.Annotations;

using System.Collections;

using Exception   = System.Exception;
using IDisposable = System.IDisposable;
using Action      = System.Action;


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
    public bool IsForgotten => m_State == State.Forgotten;
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
            value?.Invoke(m_Value);
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

    /// <summary>
    ///   Event callback that is invoked regardless of success/fail, so long as
    ///   the promise is no longer pending.
    /// </summary>
    public event Action OnCompleted
    {
      add
      {
        if (m_State == State.Pending)
          m_OnCompleted += value;
        else
          value?.Invoke();
      }
      remove => m_OnCompleted -= value;
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
      if (m_State == State.Pending || m_State == State.Succeeded)
      {
        m_Value = value;
        m_State = State.Succeeded;
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


    /// <summary>
    ///   Danger: Blocks the current thread while this promise is pending.
    ///   You probably don't want to call this from the main thread.
    /// </summary>
    public void AwaitBlocking()
    {
      while (((IEnumerator)this).MoveNext())
      {
        // await
      }
    }

    /// <summary>
    ///   Like <see cref="AwaitBlocking"/>, except awaits using a coroutine in
    ///   the <see cref="ActiveScene"/>.
    /// </summary>
    public void AwaitCoroutine()
    {
      ActiveScene.Coroutines.Run(this);
    }

    /// <inheritdoc cref="AwaitCoroutine()"/>
    /// <param name="key">
    ///   A string key retval that you can use to refer to the coroutine spawned
    ///   by this method later.
    /// </param>
    public void AwaitCoroutine(out string key)
    {
      ActiveScene.Coroutines.Run(this, out key);
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
    private Action        m_OnCompleted;


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

      if (m_OnCompleted != null)
      {
        m_OnCompleted.Invoke();
        m_OnCompleted = null;
      }

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
      m_OnCompleted = null;
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