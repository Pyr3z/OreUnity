/** @file       Objects/DeviceSpec.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-10
**/

using UnityEngine;


namespace Ore
{
  /// <summary>
  /// TODO: unfinished class, might get cut now that we have `DeviceSpy`.
  /// </summary>
  [System.Serializable]
  public sealed class DeviceSpec
  {
    public enum Platform
    {
      Editor,
      Android,
      iOS,
      WebGL,
      Server
    }

    [SerializeField]
    private Platform m_Platform;

    [SerializeField]
    private VersionID m_OSVersion;


    private DeviceSpec()
    {

    }

  } // end class DeviceSpec

}
