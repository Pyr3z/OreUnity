/** @file       Objects/SceneBuddy.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/

using UnityEngine;
using UnityEngine.SceneManagement;


namespace Bore
{

  public sealed class SceneBuddy : OSingleton<SceneBuddy>
  {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterGlobalCallbacks()
    {
      SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private static void OnActiveSceneChanged(Scene prev, Scene next)
    {
      var buddy = Current;
      if (!buddy)
        buddy = Instantiate();

      Debug.Assert(!!buddy, "!!buddy");

      buddy.SetDontDestroyOnLoad(!next.isLoaded);
    }

    private static SceneBuddy Instantiate()
    {
      Debug.Assert(!Current);

      var obj = new GameObject($"[{nameof(SceneBuddy)}]");
      obj.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
      obj.isStatic = true;

      var bud = obj.AddComponent<SceneBuddy>();

      Debug.Assert(Current == bud, nameof(SceneBuddy) + " instantiation");

      return bud;
    }

  }

}
