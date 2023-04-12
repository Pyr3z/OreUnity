/*! @file       Runtime/CodeJudge.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-11
**/

using JetBrains.Annotations;

using Stopwatch = System.Diagnostics.Stopwatch;


namespace Ore
{
  [PublicAPI]
  public class CodeJudge : System.IDisposable
  {
    public CodeJudge([NotNull] string identifier)
    {
      m_Identifier = identifier;
      m_Watch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
      m_Watch.Stop();
      if (s_AllCases.Find(m_Identifier, out var kase) && kase != null)
      {
        kase.Ticks += m_Watch.ElapsedTicks;
        ++ kase.Count;
      }
      else
      {
        s_AllCases.Map(m_Identifier, new Case
        {
          Ticks = m_Watch.ElapsedTicks,
          Count = 1
        });
      }
    }


    string    m_Identifier;
    Stopwatch m_Watch;


    public static void Report(string identifier, out long ticks, out int callCount, bool clear = true)
    {
      ticks     = 0L;
      callCount = 0;

      if (!s_AllCases.Find(identifier, out var kase))
        return;

      if (kase != null)
      {
        ticks     = kase.Ticks;
        callCount = kase.Count;
      }

      if (clear)
        s_AllCases.Unmap(identifier);
    }

    public static void ClearAll()
    {
      s_AllCases.Clear();
    }


    class Case
    {
      public long Ticks;
      public int  Count;
    }

    static readonly HashMap<string,Case> s_AllCases = new HashMap<string,Case>();
  } // end class CodeJudge
}