/*! @file       Abstract/OSingleton.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-17
**/

// ReSharper disable ConvertToAutoPropertyWithPrivateSetter

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

// TODO remove temporary type spoofs
using SceneRef = System.String;


namespace Ore
{


  /// <summary>
  ///   Base class for a singleton object which must exist in scene space;
  ///   it therefore requires a "parent" GameObject and scene context.
  /// </summary>
  /// <typeparam name="TSelf">
  ///   Successor should pass its own type (CRTP), or else be deferred with
  ///   another "TSelf" generic type parameter (i.e. for base classes).
  /// </typeparam>
  [DisallowMultipleComponent]
  [PublicAPI]
  public abstract class OSingleton<TSelf> : OComponent
    where TSelf : OSingleton<TSelf>
  {
    /// <summary>
    ///   Get the current singleton instance.
    /// </summary>
    /// <remarks>
    ///   Some folks prefer <see cref="Instance">"Instance"</see>, some prefer
    ///   <see cref="Current">"Current"</see>, and <i>some</i> even prefer
    ///   <see cref="Agent">"Agent"</see>; they are all exactly the same. <br/><br/>
    ///   This is one of the few times I will attempt to make everyone happy ;)
    /// </remarks>
    public static TSelf Current  => s_Current;

    /// <inheritdoc cref="Current"/>>
    public static TSelf Instance => s_Current;

    /// <inheritdoc cref="Current"/>>
    public static TSelf Agent    => s_Current;


    public static bool IsActive             => s_Current && s_Current.isActiveAndEnabled;

    public static bool IsDontDestroyOnLoad  => s_Current && s_Current.m_DontDestroyOnLoad;

    public static bool IsReplaceable        => !s_Current || s_Current.m_IsReplaceable;

    public static bool IsValidWhileDisabled => s_Current && s_Current.m_IsValidWhileDisabled;


    public static bool TryGuarantee(out TSelf instance)
    {
      return (instance = s_Current) || TryCreate(out instance);
    }

    public static bool TryCreate(out TSelf instance, bool force = false)
    {
      if (s_Current)
      {
        if (force)
        {
          s_Current.DestroyGameObject();
        }
        else
        {
          instance = s_Current;
          return true;
        }
      }

      instance = new GameObject($"[{typeof(TSelf).Name}]").AddComponent<TSelf>();
      return instance && !instance.m_OnFirstInitialized.IsEnabled;
    }

    public static bool TryGetScene(out Scene scene)
    {
      if (IsActive)
      {
        scene = Current.gameObject.scene;
        return true;
      }

      scene = default;
      return false;
    }


    private static TSelf s_Current;


  [Header("Scene Singleton")]
    [SerializeField]
    protected bool m_DontDestroyOnLoad;
    [SerializeField]
    protected bool m_IsReplaceable;
    [SerializeField]
    protected bool m_IsValidWhileDisabled;
    [SerializeField]
    protected DelayedEvent m_OnFirstInitialized = new DelayedEvent();


    [System.Diagnostics.Conditional("DEBUG")]
    public void ValidateInitialization() // good to call as a listener to "On First Initialized"
    {
      // ReSharper disable once HeapView.ObjectAllocation
      OAssert.AllTrue(this, s_Current == this,
                               !m_OnFirstInitialized.IsEnabled,
                               isActiveAndEnabled);
      Orator.Log($"Singleton registration validated.", this);
    }


    protected virtual void OnEnable()
    {
      bool ok = TryInitialize((TSelf)this);
      OAssert.True(ok, this);
    }

    protected virtual void OnDisable()
    {
      if (!m_IsValidWhileDisabled && s_Current == this)
      {
        s_Current = null;
      }
    }

    protected virtual void OnDestroy()
    {
      if (s_Current == this)
      {
        s_Current = null;
        m_OnFirstInitialized.TryCancelInvoke();
      }
    }

    protected virtual void OnValidate()
    {
      // placeholder
    }


    protected bool TryInitialize(TSelf self)
    {
      if (OAssert.Fails(this == self, $"Proper usage: {nameof(TryInitialize)}(this)", ctx: self))
        return false;

      if (s_Current && s_Current != self)
      {
        if (!s_Current.m_IsReplaceable)
        {
          DestroyGameObject();
          return false;
        }

        s_Current.DestroyGameObject();
      }

      if (m_OnFirstInitialized.IsEnabled && !m_OnFirstInitialized.TryInvoke())
        return false;

      m_OnFirstInitialized.IsEnabled = false;

      s_Current = self;

      if (m_DontDestroyOnLoad)
      {
        DontDestroyOnLoad(gameObject);
      }

      return true;
    }


    protected void SetDontDestroyOnLoad(bool set)
    {
      m_DontDestroyOnLoad = set;

      if (!Application.IsPlaying(this))
        return;

      if (set)
        DontDestroyOnLoad(gameObject);
      else
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
    }

  } // end class OSingleton

}
