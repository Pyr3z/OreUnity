/*! @file       Editor/EditorStateValidator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;


namespace Ore.Editor
{

  [InitializeOnLoad]
  public static class EditorStateValidator
  {

    static EditorStateValidator()
    {
      EditorApplication.delayCall += ValidateOAssetSingletons;
      EditorApplication.delayCall += ValidatePreloadedAssets;
    }


    [MenuItem("Ore/Validate/OAssetSingletons")]
    internal static void ValidateOAssetSingletons()
    {
      var silencers = new System.Type[]
      {
        typeof(CreateAssetMenuAttribute),
        typeof(OptionalAssetAttribute),
        typeof(System.ObsoleteAttribute)
      };

      foreach (var tself in TypeCache.GetTypesDerivedFrom(typeof(OAssetSingleton<>)))
      {
        if (tself == null || tself.IsAbstract || tself.IsGenericType || tself.AreAnyDefined(silencers))
          continue;

        if (!AssetDatabase.FindAssets($"t:{tself.Name}").IsEmpty())
          continue;

        string filepath;
        if (tself.GetCustomAttribute(typeof(AssetPathAttribute)) is AssetPathAttribute attr)
        {
          filepath = attr.Path;
        }
        else
        {
          filepath = $"Assets/Resources/{tself.Name}.asset";
        }
        
        if (!Filesystem.PathExists(filepath) && Filesystem.TryMakePathTo(filepath))
        {
          var instance = ScriptableObject.CreateInstance(tself);
          OAssert.Exists(instance);

          AssetDatabase.CreateAsset(instance, filepath);
          AssetDatabase.SaveAssetIfDirty(instance);
          AssetDatabase.ImportAsset(filepath); // overkill?

          Orator.Log($"Created new OAssetSingleton <{tself.Name}> at \"{filepath}\"", instance);
        }
      }
    }

    [MenuItem("Ore/Validate/Preloaded Assets")]
    internal static void ValidatePreloadedAssets()
    {
      var preloaded = new List<Object>(PlayerSettings.GetPreloadedAssets());
      var set = new HashSet<Object>(preloaded);

      set.RemoveWhere(obj => !obj);

      int changed = preloaded.Count - set.Count;
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
        Orator.Log($"{nameof(EditorBridge)}: Cleaning up {changed} null / duplicate \"Preloaded Asset\" entries.");
        PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
      }
    }

  } // end static calss EditorStateValidator

}
