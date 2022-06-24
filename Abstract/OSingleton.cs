/** @file       Abstract/OSingleton.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-17
**/

using UnityEngine;
using UnityEngine.SceneManagement;

// TODO remove temporary type spoofs
using SceneRef = UnityEngine.SceneManagement.Scene;


namespace Bore
{


  /// <summary>
  ///   Base class for a singleton object which must exist in scene space;
  ///   it therefore requires a "parent" GameObject and scene context.
  /// </summary>
  /// <typeparam name="TSelf">
  ///   Successor should pass its own type (CRTP).
  /// </typeparam>
  public abstract class OSingleton<TSelf> : OComponent, IImmortalSingleton
    where TSelf : OSingleton<TSelf>
  {
    public static TSelf Current  => s_Current;
    private static TSelf s_Current;
    public static TSelf Instance => s_Current; // compatibility API


    public static bool IsActive             => s_Current && s_Current.isActiveAndEnabled;
    public static bool IsReplaceable        => !s_Current || s_Current.m_IsReplaceable;
    public static bool IsDontDestroyOnLoad  => s_Current && s_Current.m_DontDestroyOnLoad;


    public static bool FindInstance(out TSelf instance)
    {
      instance = s_Current;
      return instance;
    }


    public bool IsInitialized => m_IsInitialized;



  [Header("Scene Singleton")]

    [SerializeField] // [RequiredReference(DisableIfPrefab = true)]
    protected SceneRef m_OwningScene;

    [SerializeField]
    protected bool m_IsReplaceable      = false;

    //[SerializeField]
    //protected bool m_NullOnDisable      = true;

    [SerializeField]
    protected bool m_DontDestroyOnLoad  = false;

    [SerializeField]
    protected DelayedEvent m_OnFirstInitialized = new DelayedEvent();


    [System.NonSerialized]
    private bool m_IsInitialized = false;


    public void ValidateInitialization()
    {
      #if DEBUG
      Debug.Assert(s_Current == this,   "Current == this");
      Debug.Assert(m_IsInitialized,     "IsInitialized");
      Debug.Assert(isActiveAndEnabled,  "isActiveAndEnabled");

      Orator.Log($"{nameof(TSelf)} initialized.");
      #endif
    }


    protected virtual void OnEnable()
    {
      bool ok = TryInitialize((TSelf)this) && m_IsInitialized;
      Debug.Assert(ok, "TryInitialize()");

      // TODO re-enable this logic once logging & SceneAware are reimplemented

      //if (TryInitialize((TSelf)this))
      //{
      //  if (this is ISceneAware isa)
      //    isa.RegisterSceneCallbacks();
      //}
      //else
      //{
      //  $"{TSpy<TSelf>.LogName} failed to initialize."
      //    .LogError(this);
      //}
    }

    protected virtual void OnDisable()
    {
      if (s_Current == this)
      {
        s_Current = null;
      }
    }

    protected virtual void OnValidate()
    {
      if (m_DontDestroyOnLoad)
      {
        m_OwningScene = default;
      }
      else
      {
        m_OwningScene = gameObject.scene;
      }
    }


    protected bool TryInitialize(TSelf self)
    {
      Debug.Assert(this == self, "Proper usage: this.TryInitialize(this)");
      
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

      if (m_IsInitialized)
        return true;

      if (m_DontDestroyOnLoad)
      {
        DontDestroyOnLoad(gameObject);
      }

      m_OwningScene = gameObject.scene;

      return ( m_IsInitialized = m_OnFirstInitialized.TryInvokeOn(this) );
    }


    protected void SetDontDestroyOnLoad(bool set)
    {
      m_DontDestroyOnLoad = set;

      if (!Application.IsPlaying(this))
        return;

      if (set)
      {
        DontDestroyOnLoad(gameObject);
      }
      else
      {
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
      }

      m_OwningScene = gameObject.scene;
    }

  } // end class OSingleton

}
