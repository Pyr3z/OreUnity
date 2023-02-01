/*! @file       Objects/OnEnableRunner.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-01
**/

using UnityEngine;


namespace Ore
{
  [AddComponentMenu("Ore/On Enable Runner")]
  [DisallowMultipleComponent]
  public class OnEnableRunner : OComponent
  {

    [Space]

    [SerializeField]
    private DelayedEvent[] m_OnEnabled = System.Array.Empty<DelayedEvent>();

    [Space]

    [SerializeField]
    private DelayedEvent[] m_OnDisabled = System.Array.Empty<DelayedEvent>();


    private void OnEnable()
    {
      foreach (var evt in m_OnEnabled)
      {
        evt.TryInvokeOn(this);
      }
    }

    private void OnDisable()
    {
      foreach (var evt in m_OnDisabled)
      {
        evt.TryInvokeOn(this);
      }
    }
  } // end class OnEnableRunner
}