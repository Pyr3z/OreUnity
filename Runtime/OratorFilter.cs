/*! @file       Runtime/OratorFilter.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-03
**/

using JetBrains.Annotations;

using UnityEngine;

using System.Text.RegularExpressions;

using TimeSpan = System.TimeSpan;


namespace Ore
{

  [System.Serializable]
  public struct OratorFilter
  {
    [SerializeField]
    public bool Invert;

    [SerializeField]
    public LogTypeFlags Types;

    public string MessageRegex
    {
      get => m_MessageRegex;
      set
      {
        m_MessageRegex = value;
        RecompileRegex();
      }
    }

    [SerializeField, Delayed, Tooltip("Can be left empty to skip.")]
    private string m_MessageRegex;


    [System.NonSerialized]
    private Regex m_Regex;

    private const RegexOptions REGEX_OPTS = RegexOptions.Compiled   |
                                            RegexOptions.Singleline |
                                            RegexOptions.CultureInvariant;

    private const long REGEX_TIMEOUT = (long)(0.5 / TimeInterval.TICKS2MS + 0.5);


    public OratorFilter(LogTypeFlags types, string regex = null)
    {
      Invert         = false;
      Types          = types;
      m_MessageRegex = regex;
      m_Regex        = null;

      RecompileRegex();
    }


    public bool IsMatch(LogType type, [NotNull] string message)
    {
      int flag = 1 << (int)type;
      return Invert ^ ( ((int)Types & flag) == flag &&
                        ( m_Regex is null || m_Regex.IsMatch(message) ) );
    }

    public bool IsMatch(LogTypeFlags flags, [NotNull] string message)
    {
      return Invert ^ ( (Types & flags) == flags &&
                        ( m_Regex is null || m_Regex.IsMatch(message) ));
    }

    public OratorFilter Inverted()
    {
      return new OratorFilter
      {
        Invert         = !Invert,
        Types          = Types,
        m_MessageRegex = m_MessageRegex,
        m_Regex        = m_Regex
      };
    }


    internal bool HasRegex() // should only be called from Orator
    {
      RecompileRegex();
      return m_Regex != null;
    }


    private void RecompileRegex()
    {
      if (m_MessageRegex.IsEmpty())
      {
        m_Regex = null;
      }
      else
      {
        m_Regex = new Regex(m_MessageRegex, REGEX_OPTS, TimeSpan.FromTicks(REGEX_TIMEOUT));
      }
    }

  } // end struct OratorFilter

}