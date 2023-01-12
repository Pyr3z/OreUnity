/*! @file       Runtime/PackageSpy.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-01-11
**/

using Newtonsoft.Json;

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  public sealed class PackageSpy
  {
    [NotNull]
    public string this[[CanBeNull] string property] => m_Data[property];


    private readonly HashMap<string,string> m_Data = new HashMap<string,string>();


    public PackageSpy(string identifier = "dev.leviperez.ore")
    {

    }

  } // end class PackageSpy
}