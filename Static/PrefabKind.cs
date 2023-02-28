/*! @file       Static/PrefabKind.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

namespace Ore
{
  public enum PrefabKind
  {
    None,

    /// <summary>
    ///   Regular prefab assets
    /// </summary>
    Regular,

    /// <summary>
    ///   Prefab variant assets
    /// </summary>
    Variant,

    /// <summary>
    ///   Instances of regular prefabs, and prefab variants in scenes or nested
    ///   in other prefabs
    /// </summary>
    PrefabInstance,

    /// <summary>
    ///   Prefab assets and prefab variant assets
    /// </summary>
    PrefabAsset,

    /// <summary>
    ///   Instances of prefabs in scenes
    /// </summary>
    InstanceInScene,

    /// <summary>
    ///   Instances of prefabs nested inside other prefabs
    /// </summary>
    InstanceInPrefab,

    /// <summary>
    ///   Non-prefab component or gameobject instances in scenes
    /// </summary>
    NonPrefabInstance,

    /// <summary>
    ///   Prefab instances, as well as non-prefab instances
    /// </summary>
    PrefabInstanceAndNonPrefabInstance
  }
}