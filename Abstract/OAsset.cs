/*! @file       Abstract/OAsset.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-02-17
**/

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;


namespace Ore
{

  /// <summary>
  /// Base class for ScriptableObjects expected to be serialized as an asset.
  /// </summary>
  public abstract class OAsset : ScriptableObject
  {
    [SerializeField, FormerlySerializedAs("m_AdvancedFlags")]
    protected HideFlags m_HideFlags;

    protected virtual void OnValidate()
    {
      hideFlags = m_HideFlags;
    }


    [PublicAPI]
    public static bool TryCreate<T>(out T instance, [CanBeNull] string path = null)
      where T : ScriptableObject
    {
      if (TryCreate(typeof(T), out instance, path))
      {
        return true;
      }

      instance = null;
      return false;
    }

    [PublicAPI]
    public static bool TryCreate<T>([NotNull] System.Type type, out T instance, [CanBeNull] string path = null)
      where T : ScriptableObject
    {
      #if UNITY_EDITOR
        path = Paths.DetectAssetPathAssumptions(path);

        if (!ActiveScene.IsPlaying && Filesystem.PathExists(path))
        {
          instance = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type) as T;
          if (instance)
          {
            Orator.Log($"SKIPPED creating Asset of type <{type.Name}> - file already exists at \"{path}\".", instance);
            return true;
          }

          Orator.Warn($"NOT creating Asset of type <{type.Name}> at path \"{path}\" - something already exists there and it isn't a <{type.Name}>.");
          return false;
        }
      #endif

      instance = (T)CreateInstance(type);

      if (OAssert.Fails(instance, $"Object creation for {type.Name} returned null"))
        return false;

      if (path.IsEmpty())
      {
        #if UNITY_EDITOR
          if (path is null && !ActiveScene.IsPlaying)
          {
            Orator.Warn($"You are trying to create a runtime Asset of type <{type.Name}> at edit-time without specifying a path; this might be an oopsie.\n" +
                          $"Pass `string.Empty` for the {nameof(path)} parameter to squelch this warning.");
          }
        #endif

        instance.name = type.Name;

        return true;
      }

      #if UNITY_EDITOR
        if (Filesystem.TryMakePathTo(path))
        {
          UnityEditor.AssetDatabase.CreateAsset(instance, path);
          // UnityEditor.AssetDatabase.SaveAssetIfDirty(instance);
          UnityEditor.AssetDatabase.ImportAsset(path);
        }
        else
        {
          Filesystem.LogLastException();
          Destroy(instance);
          return false;
        }
      #else
        instance.name = path;
      #endif

      return true;
    }

  } // end class Asset

}
