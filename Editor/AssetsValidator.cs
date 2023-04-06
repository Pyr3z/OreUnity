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

      foreach (var tasset in TypeCache.GetTypesWithAttribute<AutoCreateAssetAttribute>())
      {
        if (tasset is null || tasset.IsAbstract || tasset.IsGenericType || tasset.AreAnyDefined(silencers))
          continue;

        if (!tasset.IsSubclassOf(typeof(ScriptableObject)))
        {
          Orator.Error($"[{nameof(AutoCreateAssetAttribute)}] is only intended for ScriptableObject types! (t:{tasset.Name})");
          continue;
        }

        string filepath = tasset.GetCustomAttribute<AutoCreateAssetAttribute>().Path;

        if (filepath is null) // package user has squelched us
          continue;

        if (filepath.Length == 0)
          filepath = $"Assets/Resources/{tasset.Name}.asset";

        if (Filesystem.PathExists(filepath) || AssetTypeHasInstance(tasset))
          continue;

        if (OAsset.TryCreate(tasset, out ScriptableObject asset, filepath))
        {
          Orator.Log($"Created new Asset of type <{tasset.Name}> at \"{filepath}\"", asset);
        }
      } // end [AutoCreateAsset] loop

      silencers = new []
      {
        typeof(CreateAssetMenuAttribute),
        typeof(AutoCreateAssetAttribute), // we just checked these = skip
        typeof(System.ObsoleteAttribute)
      };

      foreach (var tsingleton in TypeCache.GetTypesDerivedFrom(typeof(OAssetSingleton<>)))
      {
        if (tsingleton is null || tsingleton.IsAbstract || tsingleton.IsGenericType || tsingleton.AreAnyDefined(silencers))
          continue;

        string filepath = $"Assets/Resources/{tsingleton.Name}.asset";

        if (Filesystem.PathExists(filepath) || AssetTypeHasInstance(tsingleton))
          continue;

        if (OAsset.TryCreate(tsingleton, out OAsset singleton, filepath))
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

      // add missing OAssetSingleton instances (that have "Is Required On Launch" [x])

      foreach (var tasset in TypeCache.GetTypesDerivedFrom(typeof(Asset)))
      {
        if (tasset is null || tasset.IsAbstract || tasset.IsGenericType || tasset.IsDefined<System.ObsoleteAttribute>())
          continue;

        foreach (var instance in GetAssetInstances(tasset))
        {
          var asset = (Asset)instance;
          if (asset.IsRequiredOnLaunch)
          {
            if (set.Add(asset))
            {
              // preloaded.Add(asset);
              // will get added by asset's OnValidate after being loaded
              ++ changed;
            }
          }
          else
          {
            set.Remove(asset);
          }
        }
      }

      // update preloaded via set

      int i = preloaded.Count;
      while (i --> 0)
      {
        if (set.Contains(preloaded[i]))
        {
          set.Remove(preloaded[i]);
        }
        else
        {
          preloaded.RemoveAt(i);
          ++ changed;
        }
      }

      if (changed > 0)
      {
        PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
        Orator.Log(typeof(AssetsValidator), $"Changed {changed} \"Preloaded Asset\" entries.");
      }
    }


    static bool AssetTypeHasInstance(System.Type tasset)
    {
      using (var enumu = GetAssetInstances(tasset).GetEnumerator())
      {
        return enumu.MoveNext();
      }
    }

    static IEnumerable<Object> GetAssetInstances(System.Type tasset)
    {
      string[] guids = AssetDatabase.FindAssets("t:" + tasset.Name);

      if (guids.IsEmpty())
        yield break;

      foreach (string guid in guids)
      {
        var maybes = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid));

        foreach (var maybe in maybes)
        {
          if (maybe.GetType() == tasset)
            yield return maybe;
        }
      }
    }

  } // end static calss AssetsValidator

}
