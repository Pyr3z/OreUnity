/*! @file       Runtime/Timer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-08
**/

using UnityEngine;


namespace Ore
{

  [System.Serializable]
  public sealed class Timer
  {

    [SerializeField]
    private TimeInterval m_Interval;
    [SerializeField]
    private int m_Cycles;


    public Timer(float interval, int cycles = 1)
    {
      m_Interval = TimeInterval.OfSeconds(interval);
      m_Cycles = cycles;
    }

  } // end class Timer
}
