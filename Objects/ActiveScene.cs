/*! @file       Objects/ActiveScene.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
 *
 *  A Scene Singleton that sticks to the current "Active Scene", even as this
 *  status moves around among different loaded Scenes.
**/

// ReSharper disable HeapView.DelegateAllocation

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using JetBrains.Annotations;

namespace Ore
{

  using CoroutineList = List<(Coroutine coru, int id)>;


  [DefaultExecutionOrder(-500)] // rationale: Many things might depend on this class early-on.
  [DisallowMultipleComponent]
  public class ActiveScene : OSingleton<ActiveScene>
  {
    [PublicAPI]
    public static Scene Scene => s_ActiveScene;

    [PublicAPI]
    public static ICoroutineRunner Coroutines => s_Coroutiner ??= new CoroutineRunnerBuffer();


    [SerializeField]
    private TimeInterval m_DelayStartCoroutineRunner = TimeInterval.Frame;
    [SerializeField]
    private VoidEvent m_OnActiveSceneChanged = new VoidEvent();


    private static Scene s_ActiveScene; // only the size of an int, so why not?

    private static readonly HashMap<Scene, float> s_SceneBirthdays = new HashMap<Scene, float>();

    private static ICoroutineRunner s_Coroutiner;


    [PublicAPI]
    public static float GetSceneAge()
    {
      if (s_SceneBirthdays.TryGetValue(s_ActiveScene, out float birth))
      {
        return Time.realtimeSinceStartup - birth;
      }
      return 0f;
    }

    [PublicAPI]
    public static float GetSceneAge(Scene scene)
    {
      if (s_SceneBirthdays.TryGetValue(scene, out float birth))
      {
        return Time.realtimeSinceStartup - birth;
      }
      return 0f;
    }

    [PublicAPI]
    public static float GetSceneAge(string scene_name)
    {
      return GetSceneAge(SceneManager.GetSceneByName(scene_name));
    }


  #region MonoBehaviour API mistake correction

    [System.Obsolete("Do not use the base Unity APIs to start coroutines on the ActiveScene.")]
    public /**/ new /**/ Coroutine StartCoroutine(IEnumerator mistake)
    {
      Orator.Error("Don't use the base Unity APIs to start coroutines on the ActiveScene.\nUse the static `EnqueueCoroutine` methods instead.");
      return null;
    }

    [System.Obsolete("SERIOUSLY don't use this overload =^(", true)]
    public /**/ new /**/ Coroutine StartCoroutine(string mistake)
    {
      Orator.Error("SERIOUSLY don't use this overload =^(");
      return null;
    }

    [System.Obsolete("SERIOUSLY don't use this overload =^(", true)]
    public /**/ new /**/ Coroutine StartCoroutine(string mis, object take)
    {
      Orator.Error("SERIOUSLY don't use this overload =^(");
      return null;
    }

  #endregion MonoBehaviour API mistake correction


    IEnumerator Start()
    {
      return new DeferringRoutine(SetupCoroutineRunner, m_DelayStartCoroutineRunner);
    }


    private void SetupCoroutineRunner()
    {
      var coroutiner = GetComponent<CoroutineRunner>();
      if (!coroutiner)
      {
        coroutiner = gameObject.AddComponent<CoroutineRunner>();
      }

      if (s_Coroutiner is CoroutineRunnerBuffer buffer)
      {
        coroutiner.AdoptAndRun(buffer);
      }

      s_Coroutiner = coroutiner;
    }

    // the rest ensures this singleton is ALWAYS on the "active" scene.

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterActiveSceneListener()
    {
      SceneManager.activeSceneChanged += OnActiveSceneChanged;
      SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnActiveSceneChanged(Scene prev, Scene next)
    {
      s_ActiveScene = next;

      var curr = Current;
      if (!curr)
        curr = Instantiate();

      OAssert.Exists(curr, $"{nameof(ActiveScene)}.{nameof(Current)}");

      curr.SetDontDestroyOnLoad(!next.isLoaded);

      curr.m_OnActiveSceneChanged.Invoke();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
      s_SceneBirthdays[scene] = Time.realtimeSinceStartup;
    }

    private static ActiveScene Instantiate()
    {
      OAssert.True(!Current || Current.m_IsReplaceable);

      var obj = new GameObject($"[{nameof(ActiveScene)}]")
      {
        hideFlags = HideFlags.DontSave,
        isStatic  = true
      };

      var bud = obj.AddComponent<ActiveScene>();
      bud.m_IsReplaceable = true;

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
