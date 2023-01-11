/*! @file       Static/SceneObjects.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-10
 *
 *  Extension methods for SceneObjects
**/

using JetBrains.Annotations;

using UnityEngine;


namespace Ore
{
  [PublicAPI]
  public static class SceneObjects
  {

    public static bool IsInPrefabAsset([CanBeNull] this GameObject obj)
    {
      if (!obj)
        return false;

      #if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
      return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(obj);
      #else
      return obj.scene.name.IsEmpty();
      #endif
    }

    public static bool IsInPrefabAsset([CanBeNull] this Component comp)
    {
      if (!comp)
        return false;

      #if UNITY_EDITOR && UNITY_2018_3_OR_NEWER
      return UnityEditor.PrefabUtility.IsPartOfPrefabAsset(comp);
      #else
      return comp.gameObject.scene.name.IsEmpty();
      #endif
    }

  } // end static class SceneObjects
}