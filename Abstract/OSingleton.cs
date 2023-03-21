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
  ///   Successor should pass its own type
  ///   (<a href="https://en.wikipedia.org/wiki/Curiously_recurring_template_pattern">CRTP</a>),
  ///   or else its own "TSelf" generic type parameter if the intent is to
  ///   create a middleman base class.
  /// </typeparam>
  /// <remarks>
  ///   <b>Pro Tip 1:</b> You can implement the built-in <c>void Reset()</c>
  ///   Unity message to define better defaults for this base class's instance
  ///   properties. This way, you can eliminate human configuration error when
  ///   your singleton expects and only works when, for example,
  ///   <see cref="IsValidWhileDisabled"/> is set to <c>true</c>. <br/><br/>
  ///   <b>Pro Tip 2:</b> (extends Pro Tip 1) If you use <c>void OnValidate()</c>
  ///   instead of <c>Reset()</c>, you can <i>force</i> your singleton's
  ///   configuration to be certain way, as opposed to redefining default values.
  /// </remarks>
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

    /// <summary>
    ///   Check if there is an active (and enabled) instance of this singleton,
    ///   in any scene hierarchy.
    /// </summary>
    /// <remarks>
    ///   If the current instance has <see cref="IsValidWhileDisabled"/> set,
    ///   then this property skips any "is active in scene/hierarchy" checks and
    ///   returns true iff there exists an undestroyed current instance.
    /// </remarks>
    public static bool IsActive  => s_Current && ( s_Current.m_IsValidWhileDisabled ||
                                                        s_Current.isActiveAndEnabled );


    /// <summary>
    ///   Attempts to guarantee (with the maximum possible likelihood of success)
    ///   the existence of this singleton's instance.
    /// </summary>
    /// <returns>
    ///   False if no instance could be found or successfully created.
    /// </returns>
    /// <remarks>
    ///   If there is no current instance, this function will attempt to
    ///   instantiate a new GameObject in the active scene containing a single
    ///   component of this type, initialize the singleton instance, and return
    ///   it to you if it all succeeded. <seealso cref="TryCreate"/>
    /// </remarks>
    public static bool TryGuarantee(out TSelf instance)
    {
      // ReSharper disable once AssignmentInConditionalExpression
      if (instance = s_Current)
      {
        return true;
      }

      if (TryCreate(out instance))
      {
        if (instance == s_Current)
          return true;

        var ex = new UnanticipatedException("Runtime singleton did not init after " + nameof(TryCreate));
        Orator.NFE(ex, instance);
        instance.DestroyGameObject();
      }

      return false;
    }

    /// <summary>
    ///   Try to create a new singleton instance. Depending on how your class is
    ///   configured (see: <see cref="IsReplaceable"/>), the new instance may
    ///   replace an existing instance. If it can't, creation will short-circuit.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if a new singleton component was created and initialized
    ///   successfully, and assigned to the "instance" out parameter. <br/><br/>
    ///   If <c>false</c> is returned, the out parameter will be null, or else
    ///   pending destruction.
    /// </returns>
    public static bool TryCreate(out TSelf instance)
    {
      if (s_Current && !s_Current.m_IsReplaceable)
      {
        instance = null;
        return false;
      }

      instance = new GameObject($"[{typeof(TSelf).Name}]").AddComponent<TSelf>();
        // if implicit call to TryInitialize(instance) via AddComponent() failed,
        // the GameObject (and therefore `instance`) could already be marked for destruction.

      return instance;
    }

    /// <summary>
    ///   Get a handle to the <see cref="Scene"/> to which the current singleton
    ///   instance belongs.
    /// </summary>
    /// <param name="scene">
    ///   Whether or not this function returns true, this out parameter is
    ///   guaranteed to reference a valid scene if called during runtime.
    /// </param>
    /// <returns>
    ///   <c>true</c> if there is an active singleton instance, and the out
    ///   parameter points to its owning scene. <br/><br/>
    ///   If <c>false</c>, the out parameter will still point to a valid scene,
    ///   namely the scene (the "active" scene) which <i>would</i> own a new
    ///   instance of this type if it were to be created right away.
    /// </returns>
    public static bool TryGetScene(out Scene scene)
    {
      if (IsActive)
      {
        scene = Current.gameObject.scene;
        return true;
      }

      scene = SceneManager.GetActiveScene();
      return false;
    }


    private static TSelf s_Current;


    // instance API

    /// <summary>
    ///   Check if the current instance is marked to be in the DontDestroyOnLoad
    ///   runtime scene.
    /// </summary>
    public bool IsDontDestroyOnLoad  => m_DontDestroyOnLoad;

    /// <summary>
    ///   If true at runtime: <br/>
    ///   In the event that a new instance of this class gets instantiated, the
    ///   current instance will be destroyed and replaced. Otherwise, new
    ///   instances trying to replace the current instance will be
    ///   auto-destroyed before they finish initializing.
    /// </summary>
    public bool IsReplaceable
    {
      get => m_IsReplaceable;
      set => m_IsReplaceable = value;
    }

    /// <summary>
    ///   If true at runtime: <br/>
    ///   If the singleton instance becomes disabled (and not destroyed), the
    ///   internal <see cref="Current">"Current"</see> reference to the disabled
    ///   instance is maintained. This allows static and non-static callers
    ///   alike to continue using the singleton instance despite its hierarchy
    ///   state. <br/><br/>
    ///   Otherwise, the default behavior is for the "Current" reference to be
    ///   reset to <c>null</c> if/when the instance has <see cref="OnDisable"/> called.
    /// </summary>
    public bool IsValidWhileDisabled
    {
      get => m_IsValidWhileDisabled;
      set => m_IsValidWhileDisabled = value;
    }


  [Header("Scene Singleton")]
    [SerializeField]
    protected bool m_DontDestroyOnLoad;

    [SerializeField]
    [Tooltip("If true at runtime:\nIn the event that a new instance of this "       +
             "class gets instantiated, the current instance will be destroyed and " +
             "replaced. Otherwise, new instances trying to replace the current "    +
             "instance will be auto-destroyed before they finish initializing.")]
    protected bool m_IsReplaceable;

    [SerializeField]
    [Tooltip("If true at runtime:\n" +
             "If the singleton instance becomes disabled (and not destroyed), the " +
             "internal reference to the disabled singleton instance is maintained." +
             "This allows static and non-static callers alike to continue using "   +
             "the singleton instance despite its hierarchy state.\n\n"              +
             "Otherwise, the default behavior is for the global singleton "         +
             "reference to be reset to null if/when the instance has OnDisable() called.")]
    protected bool m_IsValidWhileDisabled;

    [SerializeField]
    protected DelayedEvent m_OnFirstInitialized = new DelayedEvent();



    /// <summary>
    ///   Debug: Validates your thing was set up correctly.
    /// </summary>
    /// <remarks>
    ///   Good to call as a listener to "On First Initialized".
    /// </remarks>
    public void ValidateInitialization(bool silentSuccess = false)
    {
      OAssert.AllTrue(this, s_Current == this,
                               !m_OnFirstInitialized.IsEnabled,
                              ( m_IsValidWhileDisabled || isActiveAndEnabled ));
      if (!silentSuccess)
      {
        Orator.Log("Singleton registration validated.", this);
      }
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
          // if this cancellation disrupts something you're trying to accomplish
          // with the "On First Initialized" event, you should probably consider
          // accomplishing said thing some other, more use-case specific way.
      }
    }

    protected virtual void OnValidate()
    {
      if (s_Current != this || !Application.IsPlaying(this))
        return;

      bool isDDOL = gameObject.scene.path.Equals("DontDestroyOnLoad");

      if (m_DontDestroyOnLoad && !isDDOL)
      {
        DontDestroyOnLoad(gameObject);
      }
      else if (!m_DontDestroyOnLoad && isDDOL)
      {
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
      }
    }


    protected bool TryInitialize(TSelf self)
    {
      if (OAssert.Fails(this == self, "Proper usage: TryInitialize(this)", ctx: self))
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
