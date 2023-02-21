/*! @file       Runtime/WaitForFrames.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-21
**/

using UnityEngine;


namespace Ore
{
  public sealed class WaitForFrames : CustomYieldInstruction
  {
    public override bool keepWaiting => m_Remaining -- > 0;

    public WaitForFrames(int nFrames)
    {
      m_Remaining = nFrames;
    }

    private int m_Remaining;
  }
}