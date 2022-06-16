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


    public static void Log(object msg)
    {
      Current.log(msg);
    }

    public static void Warn(object msg)
    {
      Current.warn(msg);
    }

    public static void Error(object msg)
    {
      Current.error(msg);
    }


    // lowercase function names = convention for instance versions of static methods
#pragma warning disable IDE1006

    public void log(object msg)
    {
      // TODO fancy stuff from PyroDK
      Debug.Log($"{m_LogPrefix} {msg}");
    }

    public void warn(object msg)
    {
      // TODO fancy stuff from PyroDK
      Debug.LogWarning($"{m_LogPrefix} {msg}");
    }

    public void error(object msg)
    {
      // TODO fancy stuff from PyroDK
      Debug.LogError($"{m_LogPrefix} {msg}");
    }

#pragma warning restore IDE1006

  } // end class Orator

}
