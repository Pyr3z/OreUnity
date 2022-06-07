/** @file   Static/Orator.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-01-01
**/

using UnityEngine;


namespace Bore
{

  public sealed class Orator : OAssetSingleton<Orator>
  {

  [Header("Orator Settings")]
    [SerializeField]
    private string m_LogPrefix = "[KooBox]";


    public static void Log(object ctx)
    {
      Current.log(ctx);
    }


    // lowercase function names = convention for instance versions of static methods
#pragma warning disable IDE1006

    public void log(object ctx)
    {
      // TODO fancy stuff from PyroDK
      Debug.Log($"{m_LogPrefix} {ctx}");
    }

#pragma warning restore IDE1006

  } // end class Orator

}
