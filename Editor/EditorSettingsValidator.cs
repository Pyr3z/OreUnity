/** @file       Editor/EditorSettingsValidator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Bore
{

  [InitializeOnLoad]
  public static class EditorSettingsValidator
  {

    static EditorSettingsValidator()
    {
      ValidateOAssetSingletons();
      ValidatePreloadedAssets();
    }


    internal static void ValidateOAssetSingletons()
    {
      foreach (var tself in TypeCache.GetTypesDerivedFrom(typeof(OAssetSingleton<>)))
      {
        if (tself == null || tself.IsAbstract || tself.IsGenericType)
          continue;

        var load = Resources.FindObjectsOfTypeAll(tself);
        if (load.Length > 0)
          continue;

        string filepath = $"Assets/{tself.Name}.asset"; // TODO implement AssetPathAttribute for specifying location

        if (AssetDatabase.LoadAssetAtPath(filepath, tself))
          continue;

        if (Filesystem.PathExists(filepath))
          filepath = AssetDatabase.GenerateUniqueAssetPath(filepath);
                 // (typically appends incremental numbers to filename until unique)

        if (Filesystem.TryMakePathTo(filepath))
        {
          var instance = ScriptableObject.CreateInstance(tself);
          if (OAssert.Fails(instance))
            continue;

          AssetDatabase.CreateAsset(instance, filepath);
          AssetDatabase.SaveAssetIfDirty(instance);
          AssetDatabase.ImportAsset(filepath); // overkill?

          Orator.Log($"OAssetSingleton: Created new <{tself.Name}> at {filepath}");
        }
      }
    }

    internal static void ValidatePreloadedAssets()
    {
      var preloaded = new List<Object>(PlayerSettings.GetPreloadedAssets());
      var set = new HashSet<Object>(preloaded);

      set.RemoveWhere(obj => !obj);

      int changed = 0;
      int i = preloaded.Count;
      while (i-- > 0)
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
        Orator.Log($"{nameof(EditorBridge)}: Cleaning up {changed} null / duplicate \"Preloaded Asset\" entries.");
        PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
      }
    }

  }

}
