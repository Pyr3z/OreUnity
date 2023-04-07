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
  ///   Base class for ScriptableObjects expected to be serialized as an asset
  ///   (which happens to be <i>most</i> ScriptableObjects).
  /// </summary>
  [PublicAPI]
  public abstract class OAsset : ScriptableObject
  {

    /// <inheritdoc cref="TryCreate{T}(System.Type, out T, string)"/>
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

    /// <summary>
    ///   Try to create a new instance of any <see cref="ScriptableObject"/> type,
    ///   and if ye be in the editar, will also attempt to create an Asset file
    ///   and register it with the <see cref="UnityEditor.AssetDatabase"/>.
    /// </summary>
    /// <typeparam name="T">
    ///   The ScriptableObject type that this method will cast the return type
    ///   to. This may or may not be the same type as given by the "type" parameter.
    ///   If the type given by the "type" parameter is not assignable to T,
    ///   the function will short-circuit and return false. <br/>
    ///   <b>Tip:</b> When in doubt, use T = ScriptableObject.
    /// </typeparam>
    /// <param name="type">
    ///   The fully concrete (non-abstract) ScriptableObject type that this
    ///   method will try to instantiate. This may or may not be the same type
    ///   as given by the generic type parameter T.
    /// </param>
    /// <param name="instance">
    ///   An out parameter refering to the created (or existing) asset, if
    ///   execution was successful. If this function returns false, instance
    ///   will be null.
    /// </param>
    /// <param name="path">
    ///   Optionally specify an asset path to associate the new instance with a
    ///   file in the AssetDatabase (in-editor only). <br/>
    ///   If left empty, or if not in the editor, only a runtime instance of the
    ///   given type will be instantiated (no serialized file). <br/><br/>
    /// </param>
    /// <returns>
    ///   <c>true</c> if the out parameter is a valid asset of type T, and if
    ///   a path argument was provided, this asset exists at the given path.
    /// </returns>
    /// <remarks>
    ///   If executing from the editor and specifying an asset path, and there
    ///   happens to already exist an asset at that path, this function will
    ///   only return true (with said existing asset, sans creation) iff the
    ///   existing asset is exactly of type "type" and assignable to type T. <br/><br/>
    ///   If executing outside the editor and specifying a non-empty asset path,
    ///   <i>and</i> everything was created/casted successfully, then the
    ///   created ScriptableObject's <see cref="Object.name"/> will be assigned
    ///   to the path string you provided.
    /// </remarks>
    //  Jesus christ, documentation hell
    public static bool TryCreate<T>([NotNull] System.Type type, out T instance, [CanBeNull] string path = null)
      where T : ScriptableObject
    {
      if (OAssert.Fails(typeof(T).IsAssignableFrom(type), $"<{type.Name}> is not assignable to <{typeof(T).Name}>!"))
      {
        instance = null;
        return false;
      }

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


    // instance shminstance

    /// <summary>
    ///   This asset instance will be [added to|removed from] the global
    ///   "Preloaded Assets" list if it [is|is not] required on launch. <br/> <br/>
    /// </summary>
    /// <remarks>
    ///   Must be set at edit time to be meaningful.
    /// </remarks>
    public bool IsRequiredOnLaunch
    {
      get => EditorBridge.IsPreloadedAsset(this);
      set => EditorBridge.TrySetPreloadedAsset(this, value);
    }


    /// <summary>
    ///   Destroys this object relatively immediately. <br/>
    ///   If executed in-editor, any associated serialized asset will also be
    ///   destroyed.
    /// </summary>
    ///
    /// <inheritdoc cref="DestroyOther"/>
    public void DestroySelf()
    {
      this.Destroy();
    }

    /// <summary>
    ///   Destroys the given object relatively immediately, if it exists. <br/>
    ///   If executed in-editor, any associated serialized asset will also be
    ///   destroyed.
    /// </summary>
    ///
    /// <remarks>
    ///   The purpose of this method—as well as similar methods provided in this
    ///   base class—is mainly to serve as a convenience utility when working
    ///   with Object references in serialized
    ///   <see cref="UnityEngine.Events.UnityEvent"/>s.
    /// </remarks>
    ///
    /// <seealso cref="UnityObjects.Destroy(Object)">
    ///   Ore.UnityObjects.Destroy(...)
    /// </seealso>
    public void DestroyOther([CanBeNull] Object other)
    {
      if (other)
        other.Destroy();
    }


    protected virtual void OnValidate()
    {
      // empty these days
    }

  } // end class Asset

}
