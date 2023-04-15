/*! @file     Runtime/SerialDeviceDecider.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-06-01
**/


namespace Ore
{

  /// <summary>
  ///   Serializable proxy for a DeviceDecider.
  /// </summary>
  [System.Serializable]
  public struct DeviceDeciderData
  {
    [System.Serializable]
    public struct Row
    {
      public string Dimension, Key, Weight;

      public override string ToString()    => $"({Dimension},{Key},{Weight})";

      public override int    GetHashCode() => Hashing.MakeHash(Dimension, Key, Weight);
    } // end struct Row


    public Row[] Rows;

    public bool EaseCurves;

    public float SmoothCurves;

  } // end struct SerialDeviceDecider

}