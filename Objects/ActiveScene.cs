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
  public class ActiveScene : OSingleton<ActiveScene>
  {

  [Header("ActiveScene")]
    [SerializeField, Range(0, 64), Tooltip("Set to 0 to squelch the warning.")]
    private int m_CoroutineWarnThreshold = 16;

    [SerializeField]
    private VoidEvent m_OnActiveSceneChanged = new VoidEvent();


    // TODO refactor coroutine runner to separate class

    [System.NonSerialized]
    private int m_NextCoroutineID, m_ActiveCoroutineCount;

    [System.NonSerialized]
    private readonly HashMap<object, CoroutineList> m_CoroutineMap = new HashMap<object, CoroutineList>()
    {
      KeyComparator = new ContractComparator()
    };

    [System.NonSerialized]
    private static Queue<(IEnumerator,object)> s_CoroutineQueue;

    [System.NonSerialized]
    private static readonly HashMap<Scene, float> s_SceneBirthdays = new HashMap<Scene, float>();


    [PublicAPI]
    public static float GetSceneAge()
    {
      if (IsActive && s_SceneBirthdays.TryGetValue(Instance.gameObject.scene, out float birth))
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
    [PublicAPI]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] Object contract)
    {
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

    [PublicAPI]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] string key)
    {
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

    [PublicAPI]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine, [NotNull] out string guidKey)
    {
      guidKey = Strings.MakeGUID();
      EnqueueCoroutine(routine, guidKey);
    }

    [PublicAPI]
    public static void EnqueueCoroutine([NotNull] IEnumerator routine)
    {
      if (IsActive)
      {
        Instance.StartCoroutine(routine, null);
      }
      else
      {
        s_CoroutineQueue ??= new Queue<(IEnumerator, object)>();
        s_CoroutineQueue.Enqueue((routine,null));
      }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="contract">
    /// The Object instance originally given to EnqueueCoroutine or
    /// StartCoroutine as a lifetime contract.
    /// </param>
    [PublicAPI]
    public static void CancelCoroutinesForContract([NotNull] object contract)
    {
      if (IsActive)
      {
        Instance.StopAllCoroutinesWith(contract);
      }
      else if (s_CoroutineQueue?.Count > 0)
      {
        // queue delete is notoriously SLOW, but hopefully this is uber rare
        var swapq = new Queue<(IEnumerator,object)>(s_CoroutineQueue.Count);

        while (s_CoroutineQueue.Count > 0)
        {
          var blob = s_CoroutineQueue.Dequeue();
          if (blob.Item2 != contract)
          {
            swapq.Enqueue(blob);
          }
        }

        s_CoroutineQueue = swapq;
      }
    }

    [PublicAPI]
    public /* static */ void SetCoroutineWarningThreshold(int threshold)
    {
      if (m_CoroutineWarnThreshold == threshold)
        return;

      m_CoroutineWarnThreshold = threshold;

      CheckCoroutineThreshold();
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

    // the good overloads:

    public Coroutine StartCoroutine([NotNull] IEnumerator routine, [CanBeNull] object contract)
    {
      contract ??= this;

      if (m_CoroutineMap.TryMap(contract, new CoroutineList(), out CoroutineList list) == null)
      {
        Orator.Error($"Failed to start coroutine for {contract}; HashMap state error.");
        return null;
      }

      var coru = (base.StartCoroutine(DoRoutinePlusCleanup(routine, contract, m_NextCoroutineID)), m_NextCoroutineID);

      list.Add(coru);

      ++ m_NextCoroutineID;
      ++ m_ActiveCoroutineCount;

      CheckCoroutineThreshold();

      return coru.Item1;
    }

    public void StopAllCoroutinesWith([NotNull] object contract)
    {
      if (!m_CoroutineMap.Pop(contract, out CoroutineList list))
        return;

      foreach (var (coru, id) in list)
      {
        if (coru is {})
        {
          StopCoroutine(coru);
          -- m_ActiveCoroutineCount;
        }
      }

      // break the garbage collector's back
    }


    private IEnumerator DoRoutinePlusCleanup(IEnumerator routine, object contract, int id)
    {
      if (!(contract is Object wref))
      {
        wref = this;
      }

      while (routine.MoveNext())
      {
        yield return routine.Current;

        if (!wref)
        {
          m_CoroutineMap.Unmap(contract);
          -- m_ActiveCoroutineCount;
          yield break;
        }
      }

      -- m_ActiveCoroutineCount;

      if (OAssert.Fails(m_CoroutineMap.Pop(contract, out CoroutineList list), this))
      {
        yield break;
      }

      int i = list.Count;
      while (i --> 0)
      {
        if (list[i].id == id)
        {
          list.RemoveAt(i);
          break;
        }
      }

      if (list.Count > 0)
      {
        m_CoroutineMap.Map(contract, list);
      }
    }


    private void CheckCoroutineThreshold()
    {
      if (m_CoroutineWarnThreshold > 0 && m_ActiveCoroutineCount >= m_CoroutineWarnThreshold)
      {
        Orator.Warn($"Too many concurrent coroutines running on {name}! n={m_ActiveCoroutineCount}", this);
      }
    }


    protected override void OnEnable()
    {
      if (!TryInitialize(this) || s_CoroutineQueue == null)
        return;

      while (s_CoroutineQueue.Count > 0)
      {
        var (routine, contract) = s_CoroutineQueue.Dequeue();
        if (routine is {} && contract is {})
        {
          StartCoroutine(routine, contract);
        }
      }

      s_CoroutineQueue = null;
    }

    protected override void OnDisable()
    {
      base.OnDisable();
      StopAllCoroutines();
      m_ActiveCoroutineCount = 0;
      m_CoroutineMap.Clear();
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


    private sealed class ContractComparator : Comparator<object>
    {
      public override bool IsNone(in object obj)
      {
        if (obj is Object uobj)
          return !uobj;
        return base.IsNone(in obj);
      }
    }

  } // end class ActiveScene

}
