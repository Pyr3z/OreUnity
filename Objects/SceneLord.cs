/*! @file       Objects/SceneLord.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-21
**/

using JetBrains.Annotations;
using UnityEngine.SceneManagement;


namespace Ore
{
  public class SceneLord : OAssetSingleton<SceneLord>
  {
    [PublicAPI]
    public static void LoadSceneAdditiveAsync(int build_idx)
    {
      var load = SceneManager.LoadSceneAsync(build_idx, LoadSceneMode.Additive);
      load.allowSceneActivation = true;
    }
    
    [PublicAPI]
    public static void LoadSceneAsync(int build_idx)
    {
      var load = SceneManager.LoadSceneAsync(build_idx, LoadSceneMode.Additive);
      
      load.completed += _ =>
      {
        var curr = SceneManager.GetActiveScene();
        var next = SceneManager.GetSceneAt(build_idx);
        if (curr != next)
        {
          SceneManager.SetActiveScene(next);
          SceneManager.UnloadSceneAsync(curr, UnloadSceneOptions.None);
        }
      };
      
      load.allowSceneActivation = true;
    }
    
  } // end class SceneLord
}