/*! @file       Runtime/WaitForFrames.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-21
**/

using System.Collections;


namespace Ore
{
  public sealed class WaitForFrames : IEnumerator
  {
    public WaitForFrames(int nFrames)
    {
      m_Start = nFrames;
      m_Remaining = nFrames;
    }

    public void Reset()
    {
      m_Remaining = m_Start;
    }


    object IEnumerator.Current => null;

    bool IEnumerator.MoveNext() => m_Remaining --> 0;


    private readonly int m_Start;
    private          int m_Remaining;
  }
}