/*! @file       Runtime/VoidEvent.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08
**/

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.Events;

using System.Reflection;


namespace Ore
{
  [System.Serializable]
  public class VoidEvent : UnityEvent, IEvent
  {
    public bool IsEnabled
    {
      get => m_IsEnabled;
      set => m_IsEnabled = value;
    }

    [SerializeField, HideInInspector] // handled by custom drawer
    protected bool m_IsEnabled;


    /// <param name="isEnabled">
    ///   Whether or not the event should start enabled (making it invokable). <br/>
    ///   False by default, for base use case optimization.
    /// </param>
    public VoidEvent(bool isEnabled = false)
    {
      m_IsEnabled = isEnabled;
    }

    /// <param name="listener">
    ///   A runtime delegate to register with the event.
    /// </param>
    /// <param name="isEnabled">
    ///   Whether or not the event should start enabled (making it invokable). <br/>
    ///   This overload has it <c>true</c> by default, since we can assume our
    ///   listener is probably ready to roll at construction time.
    /// </param>
    ///
    /// <inheritdoc cref="VoidEvent(bool)"/>
    public VoidEvent([CanBeNull] UnityAction listener, bool isEnabled = true)
    {
      m_IsEnabled = isEnabled;

      if (listener != null)
      {
        AddListener(listener);
      }
    }


    public new void Invoke()
    {
      _ = TryInvoke();
    }

    public virtual bool TryInvoke()
    {
      if (!m_IsEnabled)
        return false;

      try
      {
        base.Invoke();
      }
      catch (System.Exception ex)
      {
        Orator.NFE(ex);
        return false;
      }

      return true;
    }


    public VoidEvent AddPersistent([NotNull] UnityAction action)
    {
      if (action.Target is Object uObj)
      {
        AddPersistent(uObj, action.Method);
      }
      else if (action.Method.IsStatic)
      {
        AddPersistent(action.Method);
      }
      else
      {
        Orator.Warn<VoidEvent>("Not sure what to do here... adding action as a runtime listener.");
        AddListener(action);
      }

      return this;
    }

    public VoidEvent AddPersistent([NotNull] Object instance, [NotNull] MethodInfo method)
    {
      int i = GetPersistentEventCount();
      RegisterPersistentListener(i, instance, method);
      return this;
    }

    public VoidEvent AddPersistent([NotNull] MethodInfo staticMethod)
    {
      OAssert.True(staticMethod.IsStatic, "staticMethod.IsStatic");

      int i = GetPersistentEventCount();
      RegisterPersistentListener(i, null, staticMethod.DeclaringType, staticMethod);
      return this;
    }


    [NotNull]
    public static VoidEvent operator + ([CanBeNull] VoidEvent   lhs,
                                        [CanBeNull] UnityAction rhs)
    {
      if (lhs is null)
        return new VoidEvent(rhs);

      lhs.AddListener(rhs);

      return lhs;
    }

    [CanBeNull]
    public static VoidEvent operator - ([CanBeNull] VoidEvent   lhs,
                                        [CanBeNull] UnityAction rhs)
    {
      if (lhs != null)
      {
        lhs.RemoveListener(rhs);
      }

      return lhs;
    }

  } // end class VoidEvent
}