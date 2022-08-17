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
  [DefaultExecutionOrder(-500)] // rationale: Many things might depend on this class early-on.
  public sealed class ActiveScene : OSingleton<ActiveScene>
  {
    
    private static Queue<IEnumerator> s_CoroutineQueue;
    
    
    /// <summary>
    /// Primarily useful for non-Scene-bound code to enqueue a coroutine even if
    /// no scenes are loaded yet.
    /// </summary>
    /// <param name="coroutine"></param>
    public static void EnqueueCoroutine([NotNull] IEnumerator coroutine)
    {
      if (OAssert.FailsNullCheck(coroutine, nameof(coroutine)))
        return;
      
      if (IsActive)
      {
        // late comers = just run it now
        _ = Instance.StartCoroutine(coroutine);
        return;
      }
      
      s_CoroutineQueue ??= new Queue<IEnumerator>();
      s_CoroutineQueue.Enqueue(coroutine);
    }


    protected override void OnEnable()
    {
      if (!TryInitialize(this) || s_CoroutineQueue == null)
        return;
      
      while (s_CoroutineQueue.Count > 0)
      {
        var subroutine = s_CoroutineQueue.Dequeue();
        if (subroutine != null)
        {
          _ = StartCoroutine(subroutine);
        }
      }
      
      s_CoroutineQueue = null;
    }
    
    
    
    // the rest ensures this singleton is ALWAYS on the "active" scene.
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterActiveSceneListener()
    {
      SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private static void OnActiveSceneChanged(Scene prev, Scene next)
    {
      var curr = Current;
      if (!curr)
        curr = Instantiate();

      OAssert.Exists(curr, $"{nameof(ActiveScene)}.{nameof(Current)}");

      curr.SetDontDestroyOnLoad(!next.isLoaded);
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

      Orator.Log($"Auto-Instantiated: <{nameof(ActiveScene)}>");
      return bud;
    }

  } // end class ActiveScene

}
