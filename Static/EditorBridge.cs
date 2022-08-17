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

    public static bool IsMainAsset(Object asset)
    {
      return asset && AssetDatabase.IsMainAsset(asset.GetInstanceID());
    }

    public static bool TrySetPreloadedAsset(Object asset, bool set)
    {
      if (!IsMainAsset(asset))
        return false;

      var   buffer  = new List<Object>(PlayerSettings.GetPreloadedAssets());
      bool  changed = set;

      if (set)
      {
        if (buffer.Contains(asset))
          return true;
        buffer.Add(asset);
      }
      else
      {
        int i = buffer.Count;
        while (i --> 0)
        {
          if (buffer[i] != asset)
            continue;
          buffer.RemoveAt(i);
          changed = true;
          // intentionally no break; should remove duplicates too!
        }
      }

      if (buffer.RemoveAll(obj => !obj) > 0 || changed)
      {
        PlayerSettings.SetPreloadedAssets(buffer.ToArray());
      }

      return true;
    }

  } // end static class EditorBridge
  
#else // if !UNITY_EDITOR

  public static class EditorBridge
  {

    public static bool IsMainAsset(Object asset)
    {
      return asset;
    }

    public static bool TrySetPreloadedAsset(Object asset, bool set)
    {
      return true;
    }

  } // end class EditorBridge
  
#endif // UNITY_EDITOR
  
}
