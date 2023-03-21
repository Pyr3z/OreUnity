/*! @file       Static/UnityObjects.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-10
**/

using JetBrains.Annotations;

using UnityEngine;


namespace Ore
{
  [PublicAPI]
  public static class UnityObjects
  {

    const float EPSILON_SECONDS = 0.01f; // used as an epsilon for Unity APIs that deal in float seconds

    public static void Destroy([NotNull] this Object obj)
    {
      // assumes we've already checked that obj isn't already destroyed

      if (Application.isEditor)
        Object.DestroyImmediate(obj, allowDestroyingAssets: obj is Asset);
      else
        Object.Destroy(obj);
    }

    public static void Destroy([NotNull] this Object obj, float inSeconds)
    {
      // assumes we've already checked that obj isn't already destroyed

      if (Application.isEditor && inSeconds < EPSILON_SECONDS)
        Object.DestroyImmediate(obj, allowDestroyingAssets: obj is Asset);
      else
        Object.Destroy(obj, inSeconds);
    }

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

  } // end static class UnityObjects
}