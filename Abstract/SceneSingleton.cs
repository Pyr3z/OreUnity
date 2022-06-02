/*  @file       Abstract/SceneSingleton.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-17
 */

using UnityEngine;


namespace Bore.Abstract
{
  // TODO remove temporary type spoofs
  using SceneRef      = UnityEngine.SceneManagement.Scene;


  /// <summary>
  ///   Base class for a singleton object which must exist in scene space;
  ///   it therefore requires a "parent" GameObject and scene context.
  /// </summary>
  /// <typeparam name="TSelf">
  ///   Successor should pass its own type (CRTP).
  /// </typeparam>
  public abstract class SceneSingleton<TSelf> : SceneComponent
    where TSelf : SceneSingleton<TSelf>
  {
    public static TSelf Instance => s_Current;
    public static TSelf Current  => s_Current;
    private static TSelf s_Current; // private for safety

    public static bool IsActive             => s_Current && s_Current.isActiveAndEnabled;
    public static bool IsReplaceable        => !s_Current || s_Current.m_IsReplaceable;
    public static bool IsDontDestroyOnLoad  => s_Current && s_Current.m_DontDestroyOnLoad;


    public static bool FindInstance(out TSelf instance)
    {
      instance = s_Current;
      return instance;
    }



  [Header("Scene Singleton")]

    [SerializeField] // [RequiredReference(DisableIfPrefab = true)]
    private SceneRef m_OwningScene;

    // TODO consider replacing these bools with the enum flag solution
    [SerializeField]
    protected bool m_IsReplaceable      = false;
    [SerializeField]
    protected bool m_NullOnDisable      = true;
    [SerializeField]
    protected bool m_DontDestroyOnLoad  = false;

    [SerializeField]
    protected DelayedEvent m_OnAfterInitialized = new DelayedEvent();


    protected virtual void OnEnable()
    {
      _ = TryInitialize((TSelf)this);

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
      if (m_NullOnDisable && s_Current == this)
      {
        //if (this is ISceneAware isa)
        //  isa.DeregisterCallbacks();

        s_Current = null;
      }
    }

    protected virtual void OnDestroy()
    {
      if (s_Current == this)
      {
        //if (this is ISceneAware isa)
        //  isa.DeregisterCallbacks();

        s_Current = null;
      }
    }


    protected bool TryInitialize(TSelf self)
    {
      Debug.Assert(this == self, "Proper usage: this.TryInitialize(this)");
      
      if (s_Current)
      {
        if (s_Current == self)
          return true;

        if (!s_Current.m_IsReplaceable)
        {
          Destroy(gameObject);
          return false;
        }

        Destroy(s_Current.gameObject);
      }

      s_Current = self;

      if (m_DontDestroyOnLoad)
      {
        //m_OwningScene = default;
        DontDestroyOnLoad(gameObject);
      }

      m_OwningScene = gameObject.scene;

      System.Func<bool> invoke_condition  = () => s_Current == self && s_Current.isActiveAndEnabled;
      System.Action     else_action       = () => self.enabled = false;
      // TODO on logging reimplemented
      //{
      //  $"Singleton {TSpy<TSelf>.LogName} lived for less than 1 frame, and thus could not post-initialize."
      //    .LogWarning(this);
      //  enabled = false;
      //};

      StartCoroutine(InvokeNextFrameIf(m_OnAfterInitialized.Invoke, invoke_condition, else_action));
      return true;
    }

  } // end class SceneSingleton

}
