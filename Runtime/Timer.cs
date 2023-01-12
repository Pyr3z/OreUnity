/*! @file       Runtime/Timer.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-11-08
**/

using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;


namespace Ore
{

  [System.Serializable]
  public sealed class Timer
  {
    public event UnityAction OnTimerDone
    {
      add    => m_Action.AddListener(value);
      remove => m_Action.RemoveListener(value);
    }

    public TimeInterval Interval
    {
      get => m_Interval;
      set => m_Interval = value;
    }

    public int Cycles
    {
      get => m_Cycles;
      set => m_Cycles = value;
    }


    [SerializeField, Tooltip("(N < 0) = disabled\n(N == 0) = every frame")]
    private TimeInterval m_Interval;

    [SerializeField, Min(-1), Tooltip("(N < 0) = disabled\n(N == 0) = infinite")]
    private int m_Cycles;

    [SerializeField]
    private VoidEvent m_Action = new VoidEvent();


    [System.NonSerialized]
    private TimeInterval m_Clock;


    public Timer(TimeInterval interval, int cycles = 1)
    {
      m_Interval = interval;
      m_Cycles   = cycles;
    }


    public void Tick(TimeInterval dt)
    {
      if (m_Cycles < 0 | m_Interval.Ticks < 0)
        return;

      m_Clock += dt;

      if (m_Clock >= m_Interval)
      {

      }
    }

  } // end class Timer
}
