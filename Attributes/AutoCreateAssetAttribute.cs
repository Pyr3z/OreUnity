/*! @file       Attributes/AutoCreateAssetAttribute.cs
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
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class AutoCreateAssetAttribute : System.Attribute
  {
    public readonly string Path;

    /// <param name="path">
    /// Relative path to the Assets/ folder, e.g., "Resources/GoodBoy.asset"
    /// </param>
    public AutoCreateAssetAttribute([NotNull] string path)
    {
      #if UNITY_EDITOR
      OAssert.True(Paths.IsValidPath(path), $"invalid path: \"{path}\"");
      #endif

      Path = Paths.DetectAssetPathAssumptions(path);
    }
  }
}