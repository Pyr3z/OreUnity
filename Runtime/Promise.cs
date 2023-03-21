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


    public static Promise<T> ForgetOnArrival(bool squelch = false)
    {
      return new Promise<T>(squelch)
      {
        m_State = State.Forgotten
      };
    }

    public static Promise<T> FailOnArrival(T flotsam, bool squelch = false)
    {
      return new Promise<T>(squelch)
      {
        m_State = State.Failed,
        m_Value = flotsam
      };
    }

    public static Promise<T> FailOnArrival(Exception ex, bool squelch = false)
    {
      return new Promise<T>(squelch)
      {
        m_State     = State.Failed,
        m_Exception = ex
      };
    }

    public static Promise<T> SucceedOnArrival(T value, bool squelch = false)
    {
      return new Promise<T>(squelch)
      {
        m_State = State.Succeeded,
        m_Value = value
      };
    }


    public Promise(bool squelchDefaultFailAction = false)
    {
      if (!squelchDefaultFailAction)
      {
        m_OnFailed = DefaultFailureAction;
      }
    }


    public T    Value       => m_Value;

    /// <summary>
    ///   A promise is considered "completed" as long as it's not "pending".
    ///   That is, a completed promise could have succeeded, failed, OR it could
    ///   have been forgotten.
    /// </summary>
    public bool IsCompleted => m_State > State.Pending;

    /// <summary>
    ///   Forgotten promises are considered completed, having neither succeeded
    ///   nor failed.
    /// </summary>
    public bool IsForgotten => m_State == State.Forgotten;

    /// <summary>
    ///   The promise completed successfully.
    /// </summary>
    public bool Succeeded   => m_State == State.Succeeded;

    /// <summary>
    ///   The promise completed, but was marked as a failure.
    /// </summary>
    public bool Failed      => m_State == State.Failed;


    /// <summary>
    ///   This event is invoked in a subsequent call to <see cref="IEnumerator.MoveNext"/>,
    ///   provided that either <see cref="Complete"/> or <see cref="CompleteWith"/>
    ///   were called successfully on this promise.
    /// </summary>
    /// <remarks>
    ///   If the promise already succeeded by the time you add your delegate to
    ///   this event, the delegate will be invoked immediately upon subscription.
    /// </remarks>
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

    /// <summary>
    ///   This event is invoked in a subsequent call to <see cref="IEnumerator.MoveNext"/>
    ///   provided that either <see cref="Fail"/>, <see cref="FailWith(T)"/>,
    ///   or <see cref="FailWith(Exception)"/> were called on this promise.
    /// </summary>
    /// <remarks>
    ///   If the promise already completed with failure by the time you add your
    ///   delegate to this event, the delegate will be invoked immediately upon
    ///   subscription.
    /// </remarks>
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
    ///   Event callback that is invoked regardless of success/fail/forgotten,
    ///   so long as the promise is no longer pending.
    /// </summary>
    /// <remarks>
    ///   If the promise already completed by the time you add your delegate to
    ///   this event, the delegate will be invoked immediately upon subscription.
    /// </remarks>
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


    /// <summary>
    ///   Tentatively sets the promise's value if and only if it is currently in
    ///   a "pending" state. Doing so does not change the promise's state.
    /// </summary>
    public Promise<T> Maybe(T value)
    {
      if (m_State == State.Pending)
      {
        m_Value = value;
      }

      return this;
    }

    /// <summary>
    ///   Sets the promise's state to <see cref="Succeeded"/>, if and only if
    ///   the promise is still pending.
    /// </summary>
    public Promise<T> Complete()
    {
      if (m_State == State.Pending)
      {
        m_State = State.Succeeded;
      }

      return this;
    }

    /// <summary>
    ///   Equivalent to calling <see cref="Maybe"/> and <see cref="Complete"/>
    ///   in sequence, EXCEPT that it can be used to update an already <see cref="Succeeded"/>
    ///   promise with a new value.
    /// </summary>
    public Promise<T> CompleteWith(T value)
    {
      if (m_State == State.Pending || m_State == State.Succeeded)
      {
        m_Value = value;
        m_State = State.Succeeded;
      }

      return this;
    }

    /// <summary>
    ///   No matter what state the promise is currently in, forget about it.
    /// </summary>
    /// <remarks>
    ///   If the "forgotten" state remains unchanged until the next
    ///   <see cref="IEnumerator.MoveNext"/>, then the value held by this
    ///   promise will be overwritten with default(T) when said MoveNext() is
    ///   invoked.
    /// </remarks>
    public Promise<T> Forget()
    {
      // can forget from any state~
      m_State = State.Forgotten;
      return this;
    }

    /// <summary>
    ///   Marks the promise as <see cref="Failed"/>, if and only if it hasn't
    ///   already been marked as <see cref="Succeeded"/>.
    /// </summary>
    /// <remarks>
    ///   You can still mark a "Succeeded" promise as "Failed" if you explicitly
    ///   call <see cref="Forget"/> first.
    /// </remarks>
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

    /// <inheritdoc cref="Fail"/>
    /// <param name="flotsam">
    ///   Following this operation, the "flotsam" value will be passed to
    ///   <see cref="OnFailed"/> and be returned by this.<see cref="Value"/>.
    ///   Thus, the meaning of this parameter is defined by the caller (you),
    ///   and typically varies by use case.
    /// </param>
    public Promise<T> FailWith([CanBeNull] T flotsam)
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

    /// <inheritdoc cref="Fail"/>
    /// <param name="ex">
    ///   Specify an exception to associate this promise's failure with a more
    ///   specific reason <i>why</i>.
    /// </param>
    public Promise<T> FailWith([CanBeNull] Exception ex)
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


    /// <summary>
    ///   Call me if you already created your promise object, and don't want to
    ///   hear from the default <see cref="OnFailed"/> action.
    ///   <seealso cref="Orator.NFE">Orator.NFE(...)</seealso>
    /// </summary>
    public void SquelchDefaultFailureAction()
    {
      m_OnFailed -= DefaultFailureAction;
    }


    /// <summary>
    ///   Danger: Blocks the current thread while this promise is pending.
    ///   You probably don't want to call this from the main thread, but I'm not
    ///   your boss.
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
    ///   <seealso cref="ActiveScene.Coroutines">ActiveScene.Coroutines</seealso>
    ///   <seealso cref="ICoroutineRunner"/>
    /// </param>
    public void AwaitCoroutine(out string key)
    {
      ActiveScene.Coroutines.Run(this, out key);
    }

    /// <inheritdoc cref="AwaitCoroutine()"/>>
    /// <param name="key">
    ///   Re-use an existing string key to associate the coroutine spawned by
    ///   this method with.
    /// </param>
    public void AwaitCoroutine([NotNull] string key)
    {
      ActiveScene.Coroutines.Run(this, key);
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

    /// <summary>
    ///   Resets this promise back to its default, valueless pending state.
    /// </summary>
    /// <remarks>
    ///   If you originally squelched the default failure action, you will need
    ///   to call <see cref="SquelchDefaultFailureAction"/> explicitly after
    ///   calling Reset().
    /// </remarks>
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
      m_OnFailed    = DefaultFailureAction;
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