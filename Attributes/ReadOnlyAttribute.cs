/*! @file       Attributes/ReadOnlyAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2021-12-20
**/

using JetBrains.Annotations;


namespace Ore
{

  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  [System.Diagnostics.Conditional("UNITY_EDITOR")]
  public sealed class ReadOnlyAttribute : UnityEngine.PropertyAttribute
  {
  }

}
