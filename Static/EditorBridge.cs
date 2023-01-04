/*! @file       Static/EditorBridge.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-28
**/

// ReSharper disable MemberCanBePrivate.Global

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Ore
{
#if UNITY_EDITOR // periodically test me by flipping with '!'
  public static class EditorBridge
  {

    // constant value like these are REQUIRED in certain contexts,
    // e.g. default parameter values
    public const bool IS_EDITOR = true;
    #if DEBUG
    public const bool IS_DEBUG = true;
    #else
    public const bool IS_DEBUG = false;
    #endif


    public static bool IsMainAsset(Object asset)
    {
      return asset && AssetDatabase.IsMainAsset(asset.GetInstanceID());
    }

    public static bool TrySetPreloadedAsset(Object asset, bool set)
    {
      if (!IsMainAsset(asset))
        return false;

      var buffer = new List<Object>(PlayerSettings.GetPreloadedAssets());

      int changed = buffer.RemoveAll(obj => !obj || (!set && obj == asset));

      if (set && !buffer.Contains(asset))
      {
        buffer.Add(asset);
        ++ changed;
      }

      if (changed > 0)
      {
        PlayerSettings.SetPreloadedAssets(buffer.ToArray());
      }

      return true;
    }

  } // end static class EditorBridge

#else // if !UNITY_EDITOR

  public static class EditorBridge
  {

    // constant value like these are REQUIRED in certain contexts,
    // e.g. default parameter values
    public const bool IS_EDITOR = true;
    #if DEBUG
    public const bool IS_DEBUG = true;
    #else
    public const bool IS_DEBUG = false;
    #endif


    public static bool IsMainAsset(Object asset)
    {
      return asset;
    }

    public static bool TrySetPreloadedAsset(Object asset, bool set)
    {
      return true;
    }

  } // end static class EditorBridge

#endif // UNITY_EDITOR

}
