/*! @file       Editor/EditorSettingsValidator.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-06
**/

using System.Collections.Generic;

using UnityEngine;
using UnityEditor;


namespace Ore.Editor
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

        var load = Resources.FindObjectsOfTypeAll(tself);
        if (load.Length > 0)
          continue;

        string filepath = $"Assets/Resources/{tself.Name}.asset"; // TODO implement AssetPathAttribute for specifying location

        if (!Filesystem.PathExists(filepath) && Filesystem.TryMakePathTo(filepath))
        {
          var instance = ScriptableObject.CreateInstance(tself);
          OAssert.Exists(instance);

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

  }

}
