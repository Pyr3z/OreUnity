/** @file   Objects/Orator.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-01-01

    @brief  Debug logging API that simultaneously provides static interfaces as well as
            UnityEngine.Object-based INSTANCE equivalents.

    @remark If an asset for this Object does not exist, one will be auto-created in the
            Editor

    @remark See `UnityEngine.BuildOptions.ForceEnableAssertions` to turn on assertions
            even in non-development builds!

    @remark Style Guide divergence: lowercase methods in this class denote
            INSTANCE methods, and PascalCase methods denote STATIC methods
            (which typically call the instance methods on the singleton).
**/

#pragma warning disable CS0414

using UnityEngine;

using ConditionalAttribute          = System.Diagnostics.ConditionalAttribute;
using DebuggerStepThroughAttribute  = System.Diagnostics.DebuggerStepThroughAttribute;

using Debug       = UnityEngine.Debug;
using UnityAssert = UnityEngine.Assertions.Assert;


namespace Bore
{
  [DebuggerStepThrough]
  [DefaultExecutionOrder(-1337)]
  public sealed class Orator : OAssetSingleton<Orator>, IImmortalSingleton
  {
    [System.Serializable]
    private struct LogFormatDef
    {
      [SerializeField]
      public LogType    LogType;
      [SerializeField]
      public LogOption  LogOption;
      [SerializeField, Delayed]
      public string     BaseMessage;

      [SerializeField]
      public Color32    RichTextColor;
      // rich text etc... (TODO)
    } // end struct LogFormatDef

#region instance fields

    // compile-time default values:

    private const string    DEFAULT_KONSOLE_PREFIX        = "[Orator] ";
    private const LogOption DEFAULT_LOG_LOGOPT            = LogOption.NoStacktrace;

    private const string    DEFAULT_REACHED_MSG           = "Reached!";
    private const LogType   DEFAULT_REACHED_LOGTYPE       = LogType.Warning;
    private const LogOption DEFAULT_REACHED_LOGOPT        = LogOption.None;

    private const LogType   DEFAULT_ASSERT_LOGTYPE        = LogType.Assert;
    private const string    DEFAULT_ASSERT_MSG            = "Assertion failed!";
    private const LogOption DEFAULT_ASSERT_LOGOPT         = LogOption.None;

    private const bool      DEFAULT_ASSERT_EXCEPTIONS     = false;
    private const bool      DEFAULT_ASSERTIONS_IN_RELEASE = false;


  [Header("Orator Properties")]

    [SerializeField]
    private string m_OratorPrefix = DEFAULT_KONSOLE_PREFIX;

    [SerializeField]
    private LogOption m_LogStackTracePolicy = DEFAULT_LOG_LOGOPT;

    [SerializeField]
    private bool m_AssertionsRaiseExceptions = DEFAULT_ASSERT_EXCEPTIONS;      // TODO
    [SerializeField]
    private bool m_ForceAssertionsInRelease  = DEFAULT_ASSERTIONS_IN_RELEASE;  // TODO

    [Space]

    [SerializeField]
    private LogFormatDef m_ReachedFormat = new LogFormatDef {
      LogType     = DEFAULT_REACHED_LOGTYPE,
      LogOption   = DEFAULT_REACHED_LOGOPT,
      BaseMessage = DEFAULT_REACHED_MSG
    };

    [Space]

    [SerializeField]
    private LogFormatDef m_AssertionFailedFormat = new LogFormatDef {
      LogType     = DEFAULT_ASSERT_LOGTYPE,
      LogOption   = DEFAULT_ASSERT_LOGOPT,
      BaseMessage = DEFAULT_ASSERT_MSG
    };


    protected override void OnValidate()
    {
      base.OnValidate();

      // reset cached message strings
      m_FormattedReachedMessage = null;
      m_FormattedAssertMessage  = null;
    }

    #endregion


    #region instance methods

    /* IDE1006 => public member name does not match style guide */
#pragma warning disable IDE1006
    // lowercase function names = convention for instance versions of static methods

    public void reached()
    {
      // TODO improved stacktrace info from PyroDK
      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, this, "{0}", ReachedMessage);
    }

    public void reached(Object ctx)
    {
      if (!ctx) ctx = this;

      // TODO improved stacktrace info from PyroDK
      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, ctx, "{0}", ReachedMessage);
    }

    public void reached(string msg)
    {
      msg ??= string.Empty;

      // TODO improved stacktrace info from PyroDK
      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, this, "{0} \"{1}\"", ReachedMessage, msg);
    }

    public void reached(string msg, Object ctx)
    {
      msg ??= string.Empty;
      if (!ctx) ctx = this;

      // TODO improved stacktrace info from PyroDK
      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, ctx, "{0} \"{1}\"", ReachedMessage, msg);
    }


    public void assertFailed()
    {
      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, this, "{0}", AssertMessage);
    }

    public void assertFailed(Object ctx)
    {
      if (!ctx) ctx = this;

      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, ctx, "{0}", AssertMessage);
    }

    public void assertFailed(string msg)
    {
      msg ??= string.Empty;

      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, this, "{0} \"{1}\"", AssertMessage, msg);
    }

    public void assertFailed(string msg, Object ctx)
    {
      msg ??= string.Empty;
      if (!ctx) ctx = this;

      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, ctx, "{0} \"{1}\"", AssertMessage, msg);
    }


    public void log(string msg)
    {
      msg ??= string.Empty;

      Debug.LogFormat(LogType.Log, m_LogStackTracePolicy, this, "{0}{1}", m_OratorPrefix, msg);
    }

    public void log(string msg, Object ctx)
    {
      msg ??= string.Empty;
      if (!ctx) ctx = this;

      Debug.LogFormat(LogType.Log, m_LogStackTracePolicy, ctx, "{0}{1}", m_OratorPrefix, msg);
    }


    public void warn(string msg)
    {
      msg ??= string.Empty;

      Debug.LogFormat(LogType.Warning, LogOption.None, this, "{0}{1}", m_OratorPrefix, msg);
    }

    public void warn(string msg, Object ctx)
    {
      msg ??= string.Empty;
      if (!ctx) ctx = this;

      Debug.LogFormat(LogType.Warning, LogOption.None, ctx, "{0}{1}", m_OratorPrefix, msg);
    }


    public void error(string msg)
    {
      msg ??= string.Empty;

      Debug.LogFormat(LogType.Error, LogOption.None, this, "{0}{1}", m_OratorPrefix, msg);
    }

    public void error(string msg, Object ctx)
    {
      msg ??= string.Empty;
      if (!ctx) ctx = this;

      Debug.LogFormat(LogType.Error, LogOption.None, ctx, "{0}{1}", m_OratorPrefix, msg);
    }

#pragma warning restore IDE1006


    #endregion


    #region PUBLIC STATIC METHODS

    public static string Prefix
    {
      get => Instance ? Instance.m_OratorPrefix : DEFAULT_KONSOLE_PREFIX;
      set
      {
        if (Instance)
          Instance.m_OratorPrefix = value;
      }
    }
    public static bool RaiseExceptions
    {
      get => Instance ? Instance.m_AssertionsRaiseExceptions : DEFAULT_ASSERT_EXCEPTIONS;
      set
      {
        if (Instance)
          Instance.m_AssertionsRaiseExceptions = value;
      }
    }
    public static bool ForceAssertionsInRelease
    {
      get => Instance ? Instance.m_ForceAssertionsInRelease : DEFAULT_ASSERTIONS_IN_RELEASE;
      set
      {
        if (Instance)
          Instance.m_ForceAssertionsInRelease = value;
      }
    }


    public static void Reached()
    {
      if (Instance)
        Instance.reached();
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{DEFAULT_REACHED_MSG}");
    }

    public static void Reached(Object ctx)
    {
      if (Instance)
        Instance.reached(ctx);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{DEFAULT_REACHED_MSG} (name=\"{ctx}\")");
    }

    public static void Reached(string msg)
    {
      if (Instance)
        Instance.reached(msg);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{DEFAULT_REACHED_MSG} \"{msg}\"");
    }


    public static void Log(string msg)
    {
      if (Instance)
        Instance.log(msg);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{msg}");
    }

    public static void Log(string msg, Object ctx)
    {
      if (Instance)
        Instance.log(msg, ctx);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{msg} (name=\"{ctx}\")");
    }


    public static void Warn(string msg)
    {
      if (Instance)
        Instance.warn(msg);
      else
        Debug.LogWarning($"{DEFAULT_KONSOLE_PREFIX}{msg}");
    }

    public static void Warn(string msg, Object ctx)
    {
      if (Instance)
        Instance.warn(msg, ctx);
      else
        Debug.LogWarning($"{DEFAULT_KONSOLE_PREFIX}{msg} (name=\"{ctx}\")");
    }


    public static void Error(string msg)
    {
      if (Instance)
        Instance.error(msg);
      else
        Debug.LogError($"{DEFAULT_KONSOLE_PREFIX}{msg}");
    }

    public static void Error(string msg, Object ctx)
    {
      if (Instance)
        Instance.error(msg, ctx);
      else
        Debug.LogError($"{DEFAULT_KONSOLE_PREFIX}{msg} (name=\"{ctx}\")");
    }

#endregion


#region STATIC ASSERTION API

    public static class Assert
    {

      [Conditional("UNITY_ASSERTIONS")]
      public static void IsTrue(bool assertion, Object ctx = null)
      {
        if (!assertion)
        {
          if (Instance)
            Instance.assertFailed(ctx);
          else
            Debug.LogAssertion(ctx);
        }
      }

      [Conditional("UNITY_ASSERTIONS")]
      public static void IsTrue(bool assertion, string msg, Object ctx = null)
      {
        if (!assertion)
        {
          if (Instance)
            Instance.assertFailed(msg, ctx);
          else
            Debug.LogAssertion(msg, ctx);
        }
      }


    #if UNITY_ASSERTIONS
      // these shouldn't be compiled out, because their return values have logical meaning

      public static bool Fails(bool assertion, Object ctx = null)
      {
        if (assertion)
          return false;
      
        if (Instance)
          Instance.assertFailed(ctx);
        else
          Debug.LogAssertion(ctx);

        return true;
      }

      public static bool Fails(bool assertion, string msg, Object ctx = null)
      {
        if (assertion)
          return false;

        if (Instance)
          Instance.assertFailed(msg, ctx);
        else
          Debug.LogAssertion(msg, ctx);

        return true;
      }

    #else // !UNITY_ASSERTIONS
      
      #pragma warning disable IDE0060
      public static bool Fails(bool assertion) => false;
      public static bool Fails(bool assertion, object msg) => false;
      #pragma warning restore IDE0060

    #endif // UNITY_ASSERTIONS

    } // end static class Assert

#endregion


#region (private section)

    // TODO fancy formatting w/ PyroDK.RichText API

    private string m_FormattedReachedMessage = null;
    private string ReachedMessage
    {
      get
      {
        if (m_FormattedReachedMessage == null)
        {
          m_FormattedReachedMessage = $"{m_OratorPrefix}{m_ReachedFormat.BaseMessage}";
          // TODO rich text
        }

        return m_FormattedReachedMessage;
      }
    }

    private string m_FormattedAssertMessage = null;
    private string AssertMessage
    {
      get
      {
        if (m_FormattedAssertMessage == null)
        {
          m_FormattedAssertMessage = $"{m_OratorPrefix}{m_AssertionFailedFormat.BaseMessage}";
          // TODO rich text
        }

        return m_FormattedAssertMessage;
      }
    }

#endregion

  } // end class Orator

}
