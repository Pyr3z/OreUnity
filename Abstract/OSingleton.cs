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
  ///   Successor should pass its own type (CRTP).
  /// </typeparam>
  [DisallowMultipleComponent]
  public abstract class OSingleton<TSelf> : OComponent
    where TSelf : OSingleton<TSelf>
  {
    [PublicAPI]
    public static TSelf Current => s_Current;
    [PublicAPI]
    public static TSelf Instance => s_Current; // compatibility API
    [PublicAPI]
    public static bool IsActive => s_Current && s_Current.isActiveAndEnabled;
    [PublicAPI]
    public static bool IsReplaceable => !s_Current || s_Current.m_IsReplaceable;
    [PublicAPI]
    public static bool IsDontDestroyOnLoad => s_Current && s_Current.m_DontDestroyOnLoad;


    [PublicAPI]
    public static bool TryGuarantee(out TSelf instance)
    {
      return (instance = s_Current) || TryCreate(out instance);
    }

    [PublicAPI]
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
      return instance && instance.m_IsSingletonInitialized;
    }


    private static TSelf s_Current;


  [Header("Scene Singleton")]
    [SerializeField]
    protected bool m_IsReplaceable;
    [SerializeField]
    protected bool m_DontDestroyOnLoad;
    [SerializeField]
    protected DelayedEvent m_OnFirstInitialized = DelayedEvent.WithApproximateFrameDelay(1);


    [System.NonSerialized]
    private bool m_IsSingletonInitialized;


    [System.Diagnostics.Conditional("DEBUG")]
    public void ValidateInitialization() // good to call as a listener to "On First Initialized"
    {
      // ReSharper disable once HeapView.ObjectAllocation
      OAssert.AllTrue(this, s_Current == this, m_IsSingletonInitialized, isActiveAndEnabled);
      Orator.Log($"Singleton registration validated.", this);
    }


    protected virtual void OnEnable()
    {
      bool ok = TryInitialize((TSelf)this);
      OAssert.True(ok, this);
    }

    protected virtual void OnDisable()
    {
      if (s_Current == this)
        s_Current = null;
    }

    protected virtual void OnValidate()
    {
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

      s_Current = self;

      if (m_DontDestroyOnLoad)
        DontDestroyOnLoad(gameObject);

      return m_IsSingletonInitialized || (m_IsSingletonInitialized = m_OnFirstInitialized.TryInvoke());
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
