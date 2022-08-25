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
    public static bool Create<T>(out T instance, [CanBeNull] string path = null)
      where T : ScriptableObject
    {
      if (Create(typeof(T), out ScriptableObject so, path))
      {
        instance = (T)so;
        return true;
      }
      
      instance = null;
      return false;
    }
    
    [PublicAPI]
    public static bool Create<T>([NotNull] System.Type type, out T instance, [CanBeNull] string path = null)
      where T : ScriptableObject
    {
      #if UNITY_EDITOR
      if (OAssert.Fails(!Filesystem.PathExists(path), "OAsset.Create assumes asset path does not already exist."))
      {
        instance = null;
        return false;
      }
      #endif
      
      instance = (T)CreateInstance(type);
      if (OAssert.Fails(instance, "object allocation returned null"))
        return false;

      if (!path.IsEmpty())
      {
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
        #else // if !UNITY_EDITOR
        instance.name = path;
        #endif // UNITY_EDITOR
      }
      
      return true;
    }
    
  } // end class Asset

}
