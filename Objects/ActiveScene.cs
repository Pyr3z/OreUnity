/*! @file       Objects/ActiveScene.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
 *
 *  A Scene Singleton that sticks to the current "Active Scene", even as this
 *  status moves around among different loaded Scenes.
**/

// ReSharper disable HeapView.DelegateAllocation

using UnityEngine;
using UnityEngine.SceneManagement;


namespace Ore
{

  [DefaultExecutionOrder(-500)]
  public sealed class ActiveScene : OSingleton<ActiveScene>
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterGlobalCallbacks()
    {
      SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private static void OnActiveSceneChanged(Scene prev, Scene next)
    {
      var curr = Current;
      if (!curr)
        curr = Instantiate();

      OAssert.True(curr, $"{nameof(ActiveScene)}.{nameof(Current)}");

      curr.SetDontDestroyOnLoad(!next.isLoaded);
    }

    private static ActiveScene Instantiate()
    {
      Debug.Assert(!Current);

      var obj = new GameObject($"[{nameof(ActiveScene)}]")
      {
        hideFlags = HideFlags.DontSave,
        isStatic  = true
      };

      var bud = obj.AddComponent<ActiveScene>();
      Debug.Assert(Current == bud, nameof(ActiveScene) + " instantiation");

      bud.m_IsReplaceable = true;

      Orator.Log($"Instantiated runtime <{nameof(ActiveScene)}>!");
      return bud;
    }

  } // end class ActiveScene

}
