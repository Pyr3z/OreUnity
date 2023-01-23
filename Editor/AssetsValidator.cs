/*! @file       Editor/AssetsValidator.cs
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
  public static class AssetsValidator
  {

    static AssetsValidator()
    {
      EditorApplication.delayCall += ValidateTypeAttributes;
      EditorApplication.delayCall += ValidatePreloadedAssets;
    }


    [MenuItem("Ore/Validate/Asset Type Attributes")]
    internal static void ValidateTypeAttributes()
    {
      var silencers = new []
      {
        typeof(System.ObsoleteAttribute)
      };

      foreach (var tasset in TypeCache.GetTypesWithAttribute<AssetPathAttribute>())
      {
        if (tasset is null || tasset.IsAbstract || tasset.IsGenericType || tasset.AreAnyDefined(silencers))
          continue;

        if (!tasset.IsSubclassOf(typeof(ScriptableObject)))
        {
          Orator.Error($"[{nameof(AssetPathAttribute)}] is only intended for ScriptableObject types! (t:{tasset.Name})");
          continue;
        }

        var basetype = tasset.BaseType;
        if ((basetype?.IsGenericType ?? false)                              &&
            basetype.GetGenericTypeDefinition() == typeof(OAssetSingleton<>) &&
            !AssetDatabase.FindAssets($"t:{tasset.Name}").IsEmpty())
        {
          // already exists, just has been moved to a different path
          continue;
        }

        var attr = tasset.GetCustomAttribute<AssetPathAttribute>(); // shouldn't be heritable

        if (!Filesystem.PathExists(attr.Path) && OAsset.TryCreate(tasset, out ScriptableObject asset, attr.Path))
        {
          Orator.Log($"Created new Asset of type <{tasset.Name}> at \"{attr.Path}\"", asset);
        }
      }

      silencers = new []
      {
        typeof(CreateAssetMenuAttribute),
        typeof(AssetPathAttribute), // we just checked these = skip
        typeof(System.ObsoleteAttribute)
      };

      foreach (var tsingleton in TypeCache.GetTypesDerivedFrom(typeof(OAssetSingleton<>)))
      {
        if (tsingleton is null || tsingleton.IsAbstract || tsingleton.IsGenericType || tsingleton.AreAnyDefined(silencers))
          continue;

        if (!AssetDatabase.FindAssets($"t:{tsingleton.Name}").IsEmpty())
          continue;

        string filepath = $"Assets/Resources/{tsingleton.Name}.asset";

        if (!Filesystem.PathExists(filepath) && OAsset.TryCreate(tsingleton, out OAsset singleton, filepath))
        {
          Orator.Log($"Created new OAssetSingleton <{tsingleton.Name}> at \"{filepath}\"", singleton);
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

  } // end static calss AssetsValidator

}
