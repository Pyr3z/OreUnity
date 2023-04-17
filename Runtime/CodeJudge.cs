/*! @file       Runtime/CodeJudge.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-11
**/

using UnityEngine;

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  public struct CodeJudge : System.IDisposable
  {
    public CodeJudge([NotNull] string identifier, uint increment = 1u)
    {
      m_Identifier = identifier;
      m_Increment  = increment;
      m_StartTicks = System.DateTime.UtcNow.Ticks;
    }

    public void Dispose()
    {
      if (m_Identifier is null)
        return;

      long elapsed = System.DateTime.UtcNow.Ticks - m_StartTicks;

      ref var kase = ref s_AllCases.FindRef(m_Identifier, out bool found);

      if (found)
      {
        double deviatn = kase.Count == 0 ? 0 : kase.Time / kase.Count - elapsed;
        kase.Time       += elapsed;
        kase.Count      += m_Increment;
        kase.Deviations += deviatn * deviatn;
      }
      else
      {
        s_AllCases.Map(m_Identifier, new Case
        {
          Time       = elapsed,
          Count      = m_Increment,
          Deviations = 0,
          OnGUILine  = -1
        });
      }

      m_Identifier = null;
    }


    string        m_Identifier;
    readonly uint m_Increment;
    readonly long m_StartTicks;


    public static long GetCount([NotNull] string identifier)
    {
      _ = s_AllCases.Find(identifier, out Case kase);
      return kase.Count;
    }

    public static void Report([NotNull] string identifier, out TimeInterval totalTime,
                                                           out long callCount)
    {
      _ = s_AllCases.Find(identifier, out var kase);

      totalTime = new TimeInterval((long)(kase.Time + 0.5));
      callCount = kase.Count;
    }

    public static void Report([NotNull] string identifier, out double totalTime,
                                                           out long   callCount,
                                                           out double average,
                                                           out double variance,
                                                           out int    guiLine)
    {
      totalTime =  0;
      callCount =  0;
      average   =  0;
      variance  =  0;
      guiLine   = -1;

      if (!s_AllCases.Find(identifier, out var kase))
        return;

      totalTime = kase.Time;
      callCount = kase.Count;

      if (callCount > 0)
      {
        average  = kase.Time / callCount;
        variance = kase.Deviations / callCount;
      }

      guiLine = kase.OnGUILine;
    }

    public static void ReportJson([NotNull] string identifier, out string json, TimeInterval.Units units = TimeInterval.Units.Milliseconds, string fmt = "F1")
    {
      Report(identifier, out double time, out long callCount, out double avg, out double varp, out _ );

      using (new RecycledStringBuilder(out var bob))
      {
        bob.Append("{\n");

        bob.Append("  \"identifier\": \"").Append(identifier).Append("\",\n");

        bob.Append("  \"callCount\": ").Append(callCount.ToInvariant()).Append(",\n");

        var ti = new TimeInterval((long)(time + 0.5));
        bob.Append("  \"totalTime\": \"").Append(ti.ToString(units, fmt)).Append("\",\n");

        ti.Ticks = (long)(avg + 0.5);
        bob.Append("  \"average\": \"").Append(ti.ToString(units, fmt)).Append("\",\n");

        ti.Ticks = (long)(varp + 0.5);
        bob.Append("  \"variance\": \"").Append(ti.ToString(units, fmt)).Append("\",\n");

        ti.Ticks = (long)(System.Math.Sqrt(varp) + 0.5);
        bob.Append("  \"stdev\": \"").Append(ti.ToString(units, fmt)).Append("\"\n");

        bob.Append('}');

        json = bob.ToString();
      }
    }

    public static void ReportOnGUI([NotNull] string identifier, TimeInterval.Units units = TimeInterval.Units.Milliseconds, string fmt = "F1")
    {
      Report(identifier, out double ticks, out long callCount, out double avg, out double varp, out int line);

      if (line < 0)
      {
        ref var kase = ref s_AllCases.FindRef(identifier, out bool found);
        if (!found)
          return;

        kase.OnGUILine = line = s_OnGUILine++;
      }

      var total = new TimeInterval((long)(ticks + 0.5));
      var mean  = new TimeInterval((long)(avg + 0.5));
      var stdev = new TimeInterval((long)(System.Math.Sqrt(varp) + 0.5));

      string text;
      using (new RecycledStringBuilder(identifier, out var bob))
      {
        bob.Append(": count=").Append(callCount.ToInvariant());
        bob.Append(", total=").Append(total.ToString(units, fmt));
        bob.Append(", mean=" ).Append(mean.ToString(units, fmt));
        bob.Append(", stdev=").Append(stdev.ToString(units, fmt));
        text = bob.ToString();
      }

      const float X = 5f, Y = 5f, H = 22f;

      var pos = new Rect(x: X,
                         y: Y + line * (H + 2f),
                     width: Screen.width - 2 * X,
                    height: H);

      // const float S = 1f;
      // var scale = new Vector2(S, S);
      // var pivot = new Vector2(pos.x + pos.width / 2, pos.y + pos.height);
      //
      // GUIUtility.ScaleAroundPivot(scale, pivot);

      GUI.Label(pos, text);
    }

    public static void Reset([NotNull] string identifier)
    {
      ref var kase = ref s_AllCases.FindRef(identifier, out bool found);
      if (found)
      {
        kase.Time = 0;
        kase.Count = 0;
        kase.Deviations = 0;
      }
    }

    public static void Remove([NotNull] string identifier)
    {
      if (s_AllCases.Pop(identifier, out Case pop) && pop.OnGUILine > -1)
      {
        // TODO hack less
        using (var iter = s_AllCases.GetEnumerator())
        {
          while (iter.MoveNext())
          {
            iter.CurrentValueRef.OnGUILine = -1;
          }
        }

        s_OnGUILine = 0;
      }
    }

    public static void RemoveAll()
    {
      s_AllCases.Clear();
      s_OnGUILine = 0;
    }


    struct Case
    {
      public double Time;
      public long   Count;
      public double Deviations;
      public int    OnGUILine;
    }

    static readonly HashMap<string,Case> s_AllCases = new HashMap<string,Case>();

    static int s_OnGUILine;

  } // end class CodeJudge
}