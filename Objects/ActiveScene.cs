/** @file       Objects/ActiveScene.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
 *
 *  A Scene Singleton that sticks to the current "Active Scene", even as this
 *  status moves around among different loaded Scenes.
**/

using UnityEngine;
using UnityEngine.SceneManagement;


namespace Bore
{

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

      Debug.Assert(curr, "!!buddy");

      curr.SetDontDestroyOnLoad(!next.isLoaded);
    }

    private static ActiveScene Instantiate()
    {
      Debug.Assert(!Current);

      var obj = new GameObject($"[{nameof(ActiveScene)}]");
      obj.hideFlags = HideFlags.DontSave;
      obj.isStatic  = true;

      var bud = obj.AddComponent<ActiveScene>();
      Debug.Assert(Current == bud, nameof(ActiveScene) + " instantiation");

      bud.m_IsReplaceable = true;

      Orator.Log($"Instantiated runtime <{nameof(ActiveScene)}>!");
      return bud;
    }

  } // end class ActiveScene

}
