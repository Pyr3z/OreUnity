/*! @file       Objects/ActiveScene.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
 *
 *  A Scene Singleton that sticks to the current "Active Scene", even as this
 *  status moves around among different loaded Scenes.
**/

// ReSharper disable HeapView.DelegateAllocation

using System;
using System.Collections;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using UnityAction = UnityEngine.Events.UnityAction;


namespace Ore
{

  [DefaultExecutionOrder(-1337)] // rationale: Many things might depend on this class early-on.
  [DisallowMultipleComponent]
  [PublicAPI]
  public sealed class ActiveScene : OSingleton<ActiveScene>
  {
    public static Scene Scene => s_ActiveScene;

    // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
    public static ICoroutineRunner Coroutines => s_Coroutiner ?? (s_Coroutiner = new CoroutineRunnerBuffer());

    public static bool IsPlaying => Instance && Application.IsPlaying(Instance);

    public static event UnityAction OnActiveSceneChanged
    {
      add
      {
        if (Instance)
          Instance.m_OnActiveSceneChanged += value;
        else
          s_OnActiveSceneChanged += value;
      }
      remove
      {
        if (Instance)
          Instance.m_OnActiveSceneChanged -= value;
        else
          s_OnActiveSceneChanged -= value;
      }
    }

    public static event UnityAction OnFixedUpdate
    {
      add
      {
        if (Instance)
          Instance.m_OnFixedUpdate += value;
        else
          s_OnFixedUpdate += value;
      }
      remove
      {
        if (Instance)
          Instance.m_OnFixedUpdate -= value;
        else
          s_OnFixedUpdate -= value;
      }
    }

    public static event UnityAction OnUpdate
    {
      add
      {
        if (Instance)
          Instance.m_OnUpdate += value;
        else
          s_OnUpdate += value;
      }
      remove
      {
        if (Instance)
          Instance.m_OnUpdate -= value;
        else
          s_OnUpdate -= value;
      }
    }

    public static event UnityAction OnLateUpdate
    {
      add
      {
        if (Instance)
          Instance.m_OnLateUpdate += value;
        else
          s_OnLateUpdate += value;
      }
      remove
      {
        if (Instance)
          Instance.m_OnLateUpdate -= value;
        else
          s_OnLateUpdate -= value;
      }
    }


  [Header("[ActiveScene]"), Space]
    [SerializeField]
    TimeInterval m_DelayStartCoroutineRunner = TimeInterval.Frame;

    [SerializeField]
    VoidEvent m_OnActiveSceneChanged = new VoidEvent(true);
    [SerializeField]
    VoidEvent m_OnFixedUpdate        = new VoidEvent(true);
    [SerializeField]
    VoidEvent m_OnUpdate             = new VoidEvent(true);
    [SerializeField]
    VoidEvent m_OnLateUpdate         = new VoidEvent(true);


    static Scene s_ActiveScene;

    static UnityAction s_OnActiveSceneChanged;
    static UnityAction s_OnFixedUpdate;
    static UnityAction s_OnUpdate;
    static UnityAction s_OnLateUpdate;

    static readonly HashMap<Scene,float> s_SceneBirthdays = new HashMap<Scene,float>();

    static ICoroutineRunner s_Coroutiner;


    [Pure]
    public static float GetSceneAge()
    {
      if (s_SceneBirthdays.Find(s_ActiveScene, out float birth))
      {
        return Time.realtimeSinceStartup - birth;
      }
      return 0f;
    }

    [Pure]
    public static float GetSceneAge(Scene scene)
    {
      if (s_SceneBirthdays.Find(scene, out float birth))
      {
        return Time.realtimeSinceStartup - birth;
      }
      return 0f;
    }

    [Pure]
    public static float GetSceneAge(string scene_name)
    {
      return GetSceneAge(SceneManager.GetSceneByName(scene_name));
    }


  #region MonoBehaviour API mistake correction

    [System.Obsolete("Do not use the base Unity APIs to start coroutines on the ActiveScene.\nUse the `ActiveScene.Coroutines` API instead.")]
    public /**/ new /**/ Coroutine StartCoroutine(IEnumerator mistake)
    {
      Orator.Error("Do not use the base Unity APIs to start coroutines on the ActiveScene.\nUse the `ActiveScene.Coroutines` API instead.");
      return null;
    }

    [System.Obsolete("Do not use this overload ಠ▃ಠ\nUse the `ActiveScene.Coroutines` API instead.", true)]
    public /**/ new /**/ Coroutine StartCoroutine(string mistake)
    {
      Orator.Error("Do not use this overload ಠ▃ಠ\nUse the `ActiveScene.Coroutines` API instead.");
      return null;
    }

    [System.Obsolete("Do not use this overload ಠ▃ಠ\nUse the `ActiveScene.Coroutines` API instead.", true)]
    public /**/ new /**/ Coroutine StartCoroutine(string mis, object take)
    {
      Orator.Error("Do not use this overload ಠ▃ಠ\nUse the `ActiveScene.Coroutines` API instead.");
      return null;
    }

  #endregion MonoBehaviour API mistake correction


    IEnumerator Start()
    {
      if (m_DelayStartCoroutineRunner <= TimeInterval.Frame)
      {
        return new DelayedRoutine(SetupCoroutineRunner, frameDelay: 1);
      }
      else
      {
        return new DelayedRoutine(SetupCoroutineRunner, m_DelayStartCoroutineRunner);
      }
    }

    void SetupCoroutineRunner()
    {
      var coroutiner = GetComponent<CoroutineRunner>();
      if (!coroutiner)
      {
        coroutiner = gameObject.AddComponent<CoroutineRunner>();
      }

      if (s_Coroutiner is CoroutineRunnerBuffer buffer)
      {
        bool sane = coroutiner.AdoptAndRun(buffer);
        OAssert.True(sane, this);
      }

      s_Coroutiner = coroutiner;
    }


    void FixedUpdate()
    {
      m_OnFixedUpdate.Invoke();
    }

    void Update()
    {
      m_OnUpdate.Invoke();
    }

    void LateUpdate()
    {
      m_OnLateUpdate.Invoke();
    }


    // the rest ensures this singleton is ALWAYS on the "active" scene.

    protected override void OnValidate()
    {
      // don't do base logic
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterActiveSceneListener()
    {
      SceneManager.activeSceneChanged += UpdateActiveSceneObject;
      SceneManager.sceneLoaded += UpdateSceneBirthday;
    }

    static void UpdateActiveSceneObject(Scene prev, Scene next)
    {
      s_ActiveScene = next;

      var curr = Current;
      if (!curr)
        curr = Instantiate();

      OAssert.Exists(curr, $"{nameof(ActiveScene)}.{nameof(Current)}");

      curr.SetDontDestroyOnLoad(!next.isLoaded);

      curr.m_OnActiveSceneChanged.Invoke();
    }

    static void UpdateSceneBirthday(Scene scene, LoadSceneMode mode)
    {
      s_SceneBirthdays[scene] = Time.realtimeSinceStartup;
    }

    static ActiveScene Instantiate()
    {
      var obj = new GameObject($"[{nameof(ActiveScene)}]")
      {
        hideFlags = HideFlags.DontSave,
        isStatic  = true
      };

      var bud = obj.AddComponent<ActiveScene>();
      bud.m_IsReplaceable = true;

      if (s_OnActiveSceneChanged != null)
      {
        bud.m_OnActiveSceneChanged = new VoidEvent(s_OnActiveSceneChanged);
        s_OnActiveSceneChanged     = null;
      }

      if (s_OnUpdate != null)
      {
        bud.m_OnUpdate = new VoidEvent(s_OnUpdate);
        s_OnUpdate     = null;
      }

      OAssert.True(Current == bud);

      return bud;
    }


  #region DEPRECATIONS

    const string OBSOLETE_MSG_EnqueueCoroutine =
      "ActiveScene.EnqueueCoroutine is obsolete. Use ActiveScene.Coroutines.Run " +
      "instead (UnityUpgradeable) -> [Ore] Ore.ActiveScene.Coroutines.Run(*)";
    const bool OBSOLETE_ERR_EnqueueCoroutine   = false;

    const string OBSOLETE_MSG_CancelCoroutinesForContract =
      "ActiveScene.CancelCoroutinesForContract is obsolete. Use ActiveScene.Coroutines.Halt " +
      "instead (UnityUpgradeable) -> [Ore] Ore.ActiveScene.Coroutines.Halt(*)";
    const bool OBSOLETE_ERR_CancelCoroutinesForContract   = false;


    [System.Obsolete(OBSOLETE_MSG_EnqueueCoroutine, OBSOLETE_ERR_EnqueueCoroutine)]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine)
    {
      Coroutines.Run(routine);
    }

    [System.Obsolete(OBSOLETE_MSG_EnqueueCoroutine, OBSOLETE_ERR_EnqueueCoroutine)]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] Object contract)
    {
      Coroutines.Run(routine, contract);
    }

    [System.Obsolete(OBSOLETE_MSG_EnqueueCoroutine, OBSOLETE_ERR_EnqueueCoroutine)]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] string key)
    {
      Coroutines.Run(routine, key);
    }

    [System.Obsolete(OBSOLETE_MSG_EnqueueCoroutine, OBSOLETE_ERR_EnqueueCoroutine)]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] out string guidKey)
    {
      Coroutines.Run(routine, out guidKey);
    }

    [System.Obsolete(OBSOLETE_MSG_CancelCoroutinesForContract, OBSOLETE_ERR_CancelCoroutinesForContract)]
    public static void CancelCoroutinesForContract([NotNull] object contract)
    {
      Coroutines.Halt(contract);
    }

  #endregion DEPRECATIONS

  } // end class ActiveScene

}
