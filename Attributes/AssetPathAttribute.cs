/*! @file       Attributes/AssetPathAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-06
**/

using JetBrains.Annotations;


namespace Ore
{
  /// <summary>
  ///   Apply me to ScriptableObjects to supply them with a custom path to be created in.
  /// </summary>
  /// <remarks>
  ///   (1) 'Assets/' is already implied in the relative path.<br/>
  ///   (2) Please use forward slashes like a good lad.
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class AssetPathAttribute : System.Attribute
  {
    public readonly string Path;

    /// <param name="path">
    /// Relative path to the Assets/ folder, e.g., "Resources/GoodBoy.asset"
    /// </param>
    public AssetPathAttribute([NotNull] string path)
    {
      OAssert.True(Paths.IsValidPath(path), $"invalid path: \"{path}\"");

      if (!path.StartsWith("Assets/"))
        path = $"Assets/{path}";

      if (!path.EndsWith(".asset"))
        path = $"{path}.asset";

      Path = path;
    }
  }
}