/*! @file       Attributes/RequiredInAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  public class RequiredInAttribute : UnityEngine.PropertyAttribute
  {

    public readonly PrefabKind Kind;

    public string ErrorMessage;


    public RequiredInAttribute(PrefabKind kind)
    {
      Kind = kind;
    }

  } // end class RequiredInAttribute
}