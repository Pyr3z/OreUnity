/*! @file       Attributes/CopyableFieldAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-24
**/

namespace Ore
{
  /// <summary>
  ///   Apply to serialized fields to make them read-only and render a functioning
  ///   "Copy" button in the inspector.
  /// </summary>
  [System.AttributeUsage(System.AttributeTargets.Field)]
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class CopyableFieldAttribute : UnityEngine.PropertyAttribute
  {
    // see Editor/CopyableFieldDrawer.cs
  }
}