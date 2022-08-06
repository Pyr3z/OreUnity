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

using System.ComponentModel;

using UnityEngine;

using Debug         = UnityEngine.Debug;
using AssException  = UnityEngine.Assertions.AssertionException;


namespace Ore
{
  //[DebuggerStepThrough]
  [DefaultExecutionOrder(-1337)]
  public sealed class Orator : OAssetSingleton<Orator>, IImmortalSingleton
  {

    #region instance fields

    [System.Serializable]
    private struct LogFormatDef
    {
      [SerializeField]
      public LogType LogType;
      [SerializeField]
      public LogOption LogOption;
      [SerializeField, Delayed]
      public string BaseMessage;

      [SerializeField]
      public Color32 RichTextColor;
      // rich text etc... (TODO)
    } // end struct LogFormatDef


    // compile-time default values:

    internal const EditorBrowsableState INSTANCE_BROWSABLE_POLICY = EditorBrowsableState.Never;

    internal const string DEFAULT_KONSOLE_PREFIX = "[" + nameof(Orator) + "]";
    internal const LogOption DEFAULT_LOG_LOGOPT = LogOption.NoStacktrace;
    internal const bool DEFAULT_INCLUDE_CONTEXT = true;

    internal const string DEFAULT_REACHED_MSG = "<b>Reached!</b>";
    internal const LogType DEFAULT_REACHED_LOGTYPE = LogType.Warning;
    internal const LogOption DEFAULT_REACHED_LOGOPT = LogOption.None;

    internal const LogType DEFAULT_ASSERT_LOGTYPE = LogType.Assert;
    internal const string DEFAULT_ASSERT_MSG = "<b>Assertion failed!</b>";
    internal const LogOption DEFAULT_ASSERT_LOGOPT = LogOption.None;

    internal const bool DEFAULT_ASSERT_EXCEPTIONS = false;
    internal const bool DEFAULT_ASSERTIONS_IN_RELEASE = false;


    [Header("Orator Properties")]

    [SerializeField]
    private string m_OratorPrefix = DEFAULT_KONSOLE_PREFIX;

    [SerializeField]
    private bool m_IncludeContextInMessages = DEFAULT_INCLUDE_CONTEXT;

    [SerializeField]
    private LogOption m_LogStackTracePolicy = DEFAULT_LOG_LOGOPT;

    [SerializeField]
    private bool m_AssertionsRaiseExceptions = DEFAULT_ASSERT_EXCEPTIONS;     // TODO
    [SerializeField]
    private bool m_ForceAssertionsInRelease = DEFAULT_ASSERTIONS_IN_RELEASE;  // TODO

    [Space]

    [SerializeField]
    private LogFormatDef m_ReachedFormat = new LogFormatDef
    {
      LogType = DEFAULT_REACHED_LOGTYPE,
      LogOption = DEFAULT_REACHED_LOGOPT,
      BaseMessage = DEFAULT_REACHED_MSG
    };

    [Space]

    [SerializeField]
    private LogFormatDef m_AssertionFailedFormat = new LogFormatDef
    {
      LogType = DEFAULT_ASSERT_LOGTYPE,
      LogOption = DEFAULT_ASSERT_LOGOPT,
      BaseMessage = DEFAULT_ASSERT_MSG
    };


    protected override void OnValidate()
    {
      base.OnValidate();

      // reset cached message strings
      m_FormattedReachedMessage = null;
      m_FormattedAssertMessage = null;
    }

    #endregion


    #region instance methods

    /* IDE1006 => public member name does not match style guide */
#pragma warning disable IDE1006
    // lowercase function names = convention for instance versions of static methods


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void reached()
    {
      reached(string.Empty, null);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void reached(Object ctx, string msg = null)
    {
      reached(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void reached(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      // TODO improved stacktrace info from PyroDK

      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, ctx, "{0} {1}", ReachedMessage, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailed()
    {
      assertionFailed(string.Empty, null);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailed(Object ctx, string msg = null)
    {
      assertionFailed(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailed(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      // TODO improved stacktrace info from PyroDK

      if (m_AssertionsRaiseExceptions)
        throw new AssException(AssertMessage, msg);
      else
        Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, ctx, "{0} {1}", AssertMessage, msg);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailedNoThrow()
    {
      assertionFailedNoThrow(string.Empty, null);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailedNoThrow(Object ctx, string msg = null)
    {
      assertionFailedNoThrow(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailedNoThrow(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      // TODO improved stacktrace info from PyroDK

      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, ctx, "{0} {1}", AssertMessage, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void log(Object ctx, string msg = null)
    {
      log(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void log(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      Debug.LogFormat(LogType.Log, m_LogStackTracePolicy, ctx, "{0} {1}", m_OratorPrefix, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void warn(Object ctx, string msg = null)
    {
      warn(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void warn(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      Debug.LogFormat(LogType.Warning, LogOption.None, ctx, "{0} {1}", m_OratorPrefix, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void error(Object ctx, string msg = null)
    {
      error(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void error(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      Debug.LogFormat(LogType.Error, LogOption.None, ctx, "{0} {1}", m_OratorPrefix, msg);
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
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX} {DEFAULT_REACHED_MSG}");
    }

    public static void Reached(Object ctx)
    {
      if (Instance)
        Instance.reached(ctx);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX} {DEFAULT_REACHED_MSG} (name=\"{ctx}\")", ctx);
    }

    public static void Reached(string msg)
    {
      if (Instance)
        Instance.reached(msg);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX} {DEFAULT_REACHED_MSG} \"{msg}\"");
    }


    public static void Log(string msg)
    {
      if (Instance)
        Instance.log(msg);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX} {msg}");
    }

    public static void Log(string msg, Object ctx)
    {
      if (Instance)
        Instance.log(msg, ctx);
      else
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX} {msg}", ctx);
    }


    public static void Warn(string msg)
    {
      if (Instance)
        Instance.warn(msg);
      else
        Debug.LogWarning($"{DEFAULT_KONSOLE_PREFIX} {msg}");
    }

    public static void Warn(string msg, Object ctx)
    {
      if (Instance)
        Instance.warn(msg, ctx);
      else
        Debug.LogWarning($"{DEFAULT_KONSOLE_PREFIX} {msg}", ctx);
    }


    public static void Error(string msg)
    {
      if (Instance)
        Instance.error(msg);
      else
        Debug.LogError($"{DEFAULT_KONSOLE_PREFIX} {msg}");
    }

    public static void Error(string msg, Object ctx)
    {
      if (Instance)
        Instance.error(msg, ctx);
      else
        Debug.LogError($"{DEFAULT_KONSOLE_PREFIX} {msg}", ctx);
    }

    #endregion


    #region STATIC ASSERTION API

    public /* static */ sealed class Assert : OAssert
    {

      // implementation now in `Static/OAssert.cs`.
      // You can still use this interface via `Orator.Assert.X()`, but `OAssert.X()` is shorter.

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
          m_FormattedReachedMessage = $"{m_OratorPrefix} {m_ReachedFormat.BaseMessage}";

        return m_FormattedReachedMessage;
      }
    }

    private string m_FormattedAssertMessage = null;
    private string AssertMessage
    {
      get
      {
        if (m_FormattedAssertMessage == null)
          m_FormattedAssertMessage = $"{m_OratorPrefix} {m_AssertionFailedFormat.BaseMessage}";

        return m_FormattedAssertMessage;
      }
    }

    private void FixupMessageContext(ref string msg, ref Object ctx)
    {
      if (!ctx)
      {
        ctx = this;
        msg ??= string.Empty;
      }
      else if (m_IncludeContextInMessages)
      {
        if (ctx is UnityEngine.Component c)
        {
          // TODO construct full scene path for scene objects
          msg = $"[{ctx.GetType().Name}] {msg}\n(scene path: \"{c.gameObject.scene.name}/{ctx.name}\")";
        }
        else if (ctx is GameObject go)
        {
          // TODO construct full scene path for scene objects
          msg = $"[{ctx.GetType().Name}] {msg}\n(scene path: \"{go.scene.name}/{ctx.name}\")";
        }
        else
        {
          msg = $"[{ctx.GetType().Name}] {msg}\n(asset name: \"{ctx.name}\")";
        }
      }
      else if (msg == null)
      {
        msg = string.Empty;
      }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Ore/Tests/Orator Logs")]
    private static void Menu_TestLogs()
    {
      Reached();
      Log("message");
      Warn("warning");
      Error("error");
      Assert.True(false, Instance);
      Log("(post-assert failure)");
    }
#endif // UNITY_EDITOR

    #endregion

  } // end class Orator

}
