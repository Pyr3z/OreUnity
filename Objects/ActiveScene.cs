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
using Object = UnityEngine.Object;
using WeakRef = System.WeakReference;


namespace Ore
{
  [DefaultExecutionOrder(-500)] // rationale: Many things might depend on this class early-on.
  public class ActiveScene : OSingleton<ActiveScene>
  {

  [Header("ActiveScene")]
    [SerializeField, Range(0, 64), Tooltip("Set to 0 to squelch the warning.")]
    private int m_TooManyCoroutinesWarningThreshold = 16;

    [System.NonSerialized]
    private static readonly Dictionary<Scene, float> s_SceneBirthdays = new Dictionary<Scene, float>();

    [System.NonSerialized]
    private static Queue<(IEnumerator,object)> s_CoroutineQueue;

    [System.NonSerialized]
    private readonly List<(Coroutine,WeakRef)> m_ContractedCoroutines = new List<(Coroutine,WeakRef)>();

    [System.NonSerialized]
    private readonly List<(Coroutine,string)> m_KeyedCoroutines = new List<(Coroutine, string)>();

    
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
    
    public static float GetSceneAge()
    {
      if (IsActive && s_SceneBirthdays.TryGetValue(Instance.gameObject.scene, out float birth))
      {
        return Time.realtimeSinceStartup - birth;
      }
      return 0f;
    }
    
    public static float GetSceneAge(Scene scene)
    {
      if (s_SceneBirthdays.TryGetValue(scene, out float birth))
      {
        return Time.realtimeSinceStartup - birth;
      }
      return 0f;
    }
    
    public static float GetSceneAge(string scene_name)
    {
      return GetSceneAge(SceneManager.GetSceneByName(scene_name));
    }
    
    
    // Global static Coroutine impl:

    /// <summary>
    /// Primarily useful for non-Scene-bound code to enqueue a coroutine even if
    /// no scenes are loaded yet.
    /// </summary>
    ///
    /// <param name="routine">
    /// A valid IEnumerator object representing a coroutine function body.
    /// </param>
    ///
    /// <param name="contract">
    /// A WeakRef to this contract object will be stored. When the contract
    /// expires (either due to GC cleanup or Unity Object deletion), the
    /// associated coroutine will stop if it is still running.
    /// </param>
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] Object contract)
    {
      if (OAssert.FailsNullChecks(routine, contract))
        return;

      if (IsActive)
      {
        // late comers = just run it now
        Instance.StartCoroutine(routine, contract);
      }
      else
      {
        s_CoroutineQueue ??= new Queue<(IEnumerator,object)>();
        s_CoroutineQueue.Enqueue((routine,contract));
      }
    }

    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] string key)
    {
      if (OAssert.FailsNullChecks(routine, key))
        return;

      if (IsActive) // late comers
      {
        Instance.StartCoroutine(routine, key);
      }
      else
      {
        s_CoroutineQueue ??= new Queue<(IEnumerator,object)>();
        s_CoroutineQueue.Enqueue((routine,key));
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="contract">
    ///
    /// </param>
    public static void CancelCoroutine([NotNull] Object contract)
    {
      if (IsActive)
      {
        Instance.StopCoroutine(contract);
      }
      else if (s_CoroutineQueue?.Count > 0)
      {
        // queue delete is notoriously SLOW, but hopefully this is uber rare
        var swapq = new Queue<(IEnumerator,object)>(s_CoroutineQueue.Count);

        while (s_CoroutineQueue.Count > 0)
        {
          var blob = s_CoroutineQueue.Dequeue();
          if (!(blob.Item2 is Object obj) || obj != contract)
          {
            swapq.Enqueue(blob);
          }
        }

        s_CoroutineQueue.Clear();
        s_CoroutineQueue = swapq;
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="key">
    ///
    /// </param>
    public static void CancelCoroutine([NotNull] string key)
    {
      if (IsActive)
      {
        Instance.StopCoroutine(key);
      }
      else if (s_CoroutineQueue?.Count > 0)
      {
        // queue delete is notoriously SLOW, but hopefully this is uber rare
        var swapq = new Queue<(IEnumerator,object)>(s_CoroutineQueue.Count);

        while (s_CoroutineQueue.Count > 0)
        {
          var blob = s_CoroutineQueue.Dequeue();
          if (!(blob.Item2 is string kee) || kee != key)
          {
            swapq.Enqueue(blob);
          }
        }

        s_CoroutineQueue.Clear();
        s_CoroutineQueue = swapq;
      }
    }


    public /* static */ void SetCoroutineCountWarningThreshold(int threshold)
    {
      if (m_TooManyCoroutinesWarningThreshold == threshold)
        return;

      m_TooManyCoroutinesWarningThreshold = threshold;

      CheckCoroutineThreshold();
    }

    // BEGIN MISTAKE CORRECTION:

    [System.Obsolete("Do not use the base Unity APIs to start coroutines on the ActiveScene.")]
    public /**/ new /**/ Coroutine StartCoroutine(IEnumerator mistake)
    {
      Orator.Warn("Don't use the base Unity APIs to start coroutines on the ActiveScene.\nUse the static `EnqueueCoroutine` methods instead.");
      return StartCoroutine(mistake, this);
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

    // the good overloads:

    public Coroutine StartCoroutine([NotNull] IEnumerator routine, [NotNull] Object contract)
    {
      var coru = base.StartCoroutine(routine);

      m_ContractedCoroutines.Add((coru, new WeakRef(contract)));

      CheckCoroutineThreshold();

      return coru;
    }

    public Coroutine StartCoroutine([NotNull] IEnumerator routine, [NotNull] string key)
    {
      var coru = base.StartCoroutine(routine);

      m_KeyedCoroutines.Add((coru, key));

      CheckCoroutineThreshold();

      return coru;
    }

    public void StopCoroutine([NotNull] Object contract)
    {
      int i = m_ContractedCoroutines.Count;
      while (i --> 0)
      {
        var (coru,wref) = m_ContractedCoroutines[i];
        if (coru is null)
        {
          m_ContractedCoroutines.RemoveAt(i);
        }
        else if (wref is null || !wref.IsAlive || (Object)wref.Target == contract)
        {
          StopCoroutine(coru);
          m_ContractedCoroutines.RemoveAt(i);
        }
      }
    }

    public /**/ new /**/ void StopCoroutine([NotNull] string key)
    {
      int i = m_KeyedCoroutines.Count;
      while (i --> 0)
      {
        var (coru, kee) = m_KeyedCoroutines[i];
        if (coru is null)
        {
          m_KeyedCoroutines.RemoveAt(i);
        }
        else if (key == kee)
        {
          StopCoroutine(coru);
          m_KeyedCoroutines.RemoveAt(i);
        }
      }
    }


    private void CheckCoroutineThreshold()
    {
      if (m_TooManyCoroutinesWarningThreshold <= 0)
        return;

      int count = m_ContractedCoroutines.Count + m_KeyedCoroutines.Count;
      if (count >= m_TooManyCoroutinesWarningThreshold)
      {
        Orator.Warn($"Too many concurrent coroutines running on ActiveScene! n={count}", this);
      }
    }

    private void CleanContractedCoroutines()
    {
      int i = m_ContractedCoroutines.Count;
      while (i --> 0)
      {
        var (coru, contract) = m_ContractedCoroutines[i];
        if (coru is null)
        {
          m_ContractedCoroutines.RemoveAt(i);
        }
        else if (contract is null || !contract.IsAlive)
        {
          StopCoroutine(coru);
          m_ContractedCoroutines.RemoveAt(i);
        }
      }
    }

    private void CleanKeyedCoroutines()
    {
      int i = m_KeyedCoroutines.Count;
      while (i --> 0)
      {
        if (m_KeyedCoroutines[i].Item1 is null)
        {
          m_KeyedCoroutines.RemoveAt(i);
        }
      }
    }

    // This is loop is a necessary evil, and is why I implore you to PLEASE not
    // abuse this singleton!
    private void LateUpdate()
    {
      // (MUST do every frame)
      CleanContractedCoroutines();

      // clean up keyed coroutines every once in a while:
      if (Time.frameCount % 337 == 0) // 337 = roughly every 5s
      {
        CleanKeyedCoroutines();
      }
    }

    protected override void OnEnable()
    {
      if (!TryInitialize(this) || s_CoroutineQueue == null)
        return;

      while (s_CoroutineQueue.Count > 0)
      {
        var (routine, thing) = s_CoroutineQueue.Dequeue();
        if (routine is {} && thing is {})
        {
          if (thing is string key)
            StartCoroutine(routine, key);
          else if (thing is Object contract)
            StartCoroutine(routine, contract);
          else
            StartCoroutine(routine, thing.ToString());
        }
      }

      s_CoroutineQueue = null;
    }

    protected override void OnDisable()
    {
      base.OnDisable();
      StopAllCoroutines();
      m_ContractedCoroutines.Clear();
      m_KeyedCoroutines.Clear();
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
      var curr = Current;
      if (!curr)
        curr = Instantiate();

      OAssert.Exists(curr, $"{nameof(ActiveScene)}.{nameof(Current)}");

      curr.SetDontDestroyOnLoad(!next.isLoaded);
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

  } // end class ActiveScene

}
