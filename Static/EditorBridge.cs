/** @file       Static/EditorBridge.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-28
**/

#if UNITY_EDITOR // periodically test me by flipping with '!'

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Bore
{

  public static class EditorBridge
  {

    public static bool IsMainAsset(Object asset)
    {
      return asset && AssetDatabase.IsMainAsset(asset.GetInstanceID());
    }

    public static bool TrySetPreloadedAsset(Object asset, bool set)
    {
      if (!IsMainAsset(asset))
      {
        //Debug.LogWarning($"Object \"{asset}\" is not a main asset reference and cannot be preloaded.");
        return false;
      }

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
          if (buffer[i] == asset)
          {
            buffer.RemoveAt(i);
            changed = true;
            // intentionally no break; should remove duplicates too!
          }
        }
      }

      if (changed)
      {
        buffer.RemoveAll(obj => !obj);
        PlayerSettings.SetPreloadedAssets(buffer.ToArray());
      }

      return true;
    }


    // TODO this functionality is probably better compartmentalized elsewhere:
    [InitializeOnLoadMethod]
    private static void ValidateEditor()
    {
      var preloaded = new List<Object>(PlayerSettings.GetPreloadedAssets());
      var set = new HashSet<Object>(preloaded);

      set.Remove(null);

      int changed = 0;
      int i = preloaded.Count;
      while (i --> 0)
      {
        if (!set.Contains(preloaded[i]))
        {
          preloaded.RemoveAt(i);
          ++changed;
        }
        else
        {
          set.Remove(preloaded[i]);
        }
      }

      if (changed > 0)
      {
        Debug.Log($"{nameof(EditorBridge)}: Cleaning up {changed} null / duplicate \"Preloaded Asset\" entries.");
        PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
      }
    }

  } // end static class EditorBridge

}

#else // !UNITY_EDITOR

using UnityEngine;

namespace Bore
{
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

}

#endif // UNITY_EDITOR
