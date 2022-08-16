/*! @file       Attributes/OptionalAssetAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-06
**/


namespace Ore
{
  /// <summary>
  /// Apply me to OAssetSingletons to prevent auto-creation of an asset in the Assets/Resources folder.
  /// (The same logic can be applied simply by using the CreateAssetMenuAttribute as well.)
  /// </summary>
  [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class OptionalAssetAttribute : System.Attribute
  {
  }

}
