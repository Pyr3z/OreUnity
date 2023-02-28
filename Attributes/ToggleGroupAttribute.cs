/*! @file       Attributes/ToggleGroupAttribute.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  [System.AttributeUsage(System.AttributeTargets.Field)]
  public class ToggleGroupAttribute : UnityEngine.PropertyAttribute
  {

    public readonly string ToggleSource;

    public readonly string Title;


    public ToggleGroupAttribute(string toggleSource)
    {
      ToggleSource = toggleSource;
    }

    public ToggleGroupAttribute(string toggleSource, string title)
    {
      ToggleSource = toggleSource;
      Title        = title;
    }

  } // end class ToggleGroupAttribute
}