/** @file   Static/Orator.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-01-01
**/

using UnityEngine;


namespace Bore
{

  public sealed class Orator : Abstract.OAssetSingleton<Orator>
  {

  [Header("Orator Settings")]
    [SerializeField]
    private string m_LogPrefix = "[KooBox] ";


    public static void Log(object ctx)
    {
      // TODO
    }

  } // end class Orator

}
