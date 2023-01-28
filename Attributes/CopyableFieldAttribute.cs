/*! @file       Attributes/CopyableFieldAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-24
**/

namespace Ore
{
  /// <summary>
  ///   Apply to a serialized field to render a "Copy" button beside it in the inspector.
  /// </summary>
  ///
  /// <remarks>
  ///   Pairs well with a [<see cref="ReadOnlyAttribute">ReadOnly</see>] attribute!
  /// </remarks>
  [System.AttributeUsage(System.AttributeTargets.Field)]
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public class CopyableFieldAttribute : UnityEngine.PropertyAttribute
  {
    // see Editor/CopyableFieldDrawer.cs
  }
}
