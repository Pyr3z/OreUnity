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

using System.Diagnostics;
using System.ComponentModel;

using UnityEngine;

using Debug         = UnityEngine.Debug;
using UnityAssert   = UnityEngine.Assertions.Assert;
using AssException  = UnityEngine.Assertions.AssertionException;


namespace Bore
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
      public LogType    LogType;
      [SerializeField]
      public LogOption  LogOption;
      [SerializeField, Delayed]
      public string     BaseMessage;

      [SerializeField]
      public Color32    RichTextColor;
      // rich text etc... (TODO)
    } // end struct LogFormatDef


    // compile-time default values:

    private const EditorBrowsableState INSTANCE_BROWSABLE_POLICY = EditorBrowsableState.Never;

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
    private bool m_AssertionsRaiseExceptions = DEFAULT_ASSERT_EXCEPTIONS;     // TODO
    [SerializeField]
    private bool m_ForceAssertionsInRelease = DEFAULT_ASSERTIONS_IN_RELEASE;  // TODO

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


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void reached()
    {
      reached("", this);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void reached(Object ctx, string msg = null)
    {
      reached(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void reached(string msg, Object ctx = null)
    {
      if (!ctx)
      {
        ctx   = this;
        msg ??= string.Empty;
      }
      else
      {
        msg = $"{msg} ({ctx})";
      }

      // TODO improved stacktrace info from PyroDK
      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, ctx, "{0} {1}", ReachedMessage, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailed()
    {
      assertionFailed("", this);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailed(Object ctx, string msg = null)
    {
      assertionFailed(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void assertionFailed(string msg, Object ctx = null)
    {
      if (!ctx)
      {
        ctx   = this;
        msg ??= string.Empty;
      }
      else
      {
        msg = $"{msg} ({ctx})";
      }

      if (m_AssertionsRaiseExceptions)
      {
        throw new AssException(AssertMessage, msg);
      }
      else
      {
        Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption, ctx, "{0} {1}", AssertMessage, msg);
      }
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void log(Object ctx, string msg = null)
    {
      log(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void log(string msg, Object ctx = null)
    {
      if (!ctx)
      {
        ctx   = this;
        msg ??= string.Empty;
      }
      else
      {
        msg = $"{msg} ({ctx})";
      }

      Debug.LogFormat(LogType.Log, m_LogStackTracePolicy, ctx, "{0}{1}", m_OratorPrefix, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void warn(Object ctx, string msg = null)
    {
      warn(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void warn(string msg, Object ctx = null)
    {
      if (!ctx)
      {
        ctx   = this;
        msg ??= string.Empty;
      }
      else
      {
        msg = $"{msg} ({ctx})";
      }

      Debug.LogFormat(LogType.Warning, LogOption.None, ctx, "{0}{1}", m_OratorPrefix, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void error(Object ctx, string msg = null)
    {
      error(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    public void error(string msg, Object ctx = null)
    {
      if (!ctx)
      {
        ctx   = this;
        msg ??= string.Empty;
      }
      else
      {
        msg = $"{msg} ({ctx})";
      }

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
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{DEFAULT_REACHED_MSG} (name=\"{ctx}\")", ctx);
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
        Debug.Log($"{DEFAULT_KONSOLE_PREFIX}{msg} (name=\"{ctx}\")", ctx);
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
        Debug.LogWarning($"{DEFAULT_KONSOLE_PREFIX}{msg} (name=\"{ctx}\")", ctx);
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
        Debug.LogError($"{DEFAULT_KONSOLE_PREFIX}{msg} (name=\"{ctx}\")", ctx);
    }

#endregion


#region STATIC ASSERTION API

    public static class Assert
    {
      private const string DEF_UNITY_ASSERTIONS = "UNITY_ASSERTIONS";


      [Conditional(DEF_UNITY_ASSERTIONS)]
      public static void True(bool assertion, Object ctx = null)
      {
        if (!assertion)
        {
          if (Instance)
            Instance.assertionFailed(ctx);
          else
            Debug.LogAssertion(ctx);
        }
      }

      [Conditional(DEF_UNITY_ASSERTIONS)]
      public static void True(bool assertion, string msg, Object ctx = null)
      {
        if (!assertion)
        {
          if (Instance)
            Instance.assertionFailed(msg, ctx);
          else
            Debug.LogAssertion(msg, ctx);
        }
      }


      [Conditional(DEF_UNITY_ASSERTIONS)]
      public static void AllTrue(params bool[] assertions)
      {
        for (int i = 0, ilen = assertions?.Length ?? 0; i < ilen; ++i)
        {
          if (!assertions[i])
          {
            if (Instance)
              Instance.assertionFailed(msg: $"condition ({i+1}/{ilen})");
            else
              Debug.LogAssertion($"{DEFAULT_ASSERT_MSG} condition ({i+1}/{ilen})");
            return;
          }
        }
      }

      [Conditional(DEF_UNITY_ASSERTIONS)]
      public static void AllTrue(Object ctx, params bool[] assertions)
      {
        for (int i = 0, ilen = assertions?.Length ?? 0; i < ilen; ++i)
        {
          if (!assertions[i])
          {
            if (Instance)
              Instance.assertionFailed(msg: $"condition ({i+1}/{ilen})", ctx);
            else
              Debug.LogAssertion($"{DEFAULT_ASSERT_MSG} condition ({i+1}/{ilen})", ctx);
            return;
          }
        }
      }


    #if UNITY_ASSERTIONS
      // these shouldn't be compiled out, because their bool return values have logical meaning

      public static bool Fails(bool assertion, Object ctx = null)
      {
        if (assertion)
          return false;
      
        if (Instance)
          Instance.assertionFailed(ctx);
        else
          Debug.LogAssertion(string.Empty, ctx);

        return true;
      }

      public static bool Fails(bool assertion, string msg, Object ctx = null)
      {
        if (assertion)
          return false;

        if (Instance)
          Instance.assertionFailed(msg, ctx);
        else
          Debug.LogAssertion(msg, ctx);

        return true;
      }

      
      public static bool FailsNullCheck(object obj, Object ctx = null)
      {
        if (!(obj is null))
          return false;

        if (Instance)
          Instance.assertionFailed(ctx);
        else
          Debug.LogAssertion(string.Empty, ctx);

        return true;
      }

      public static bool FailsNullCheck(object obj, string msg, Object ctx = null)
      {
        if (!(obj is null))
          return false;

        if (Instance)
          Instance.assertionFailed(msg, ctx);
        else
          Debug.LogAssertion(msg, ctx);

        return true;
      }

      public static bool FailsNullChecks(params object[] objs)
      {
        for (int i = 0, ilen = objs?.Length ?? 0; i < ilen; ++i)
        {
          if (objs[i] is null)
          {
            if (Instance)
              Instance.assertionFailed(msg: $"null check ({i+1}/{ilen})");
            else
              Debug.LogAssertion($"{DEFAULT_ASSERT_MSG} null check ({i + 1}/{ilen})");
            return true;
          }
        }

        return false;
      }

    #else // !UNITY_ASSERTIONS
      
      #pragma warning disable IDE0060
      public static bool Fails(bool assertion) => false;
      public static bool Fails(bool assertion, object msg) => false;
      public static bool FailsNullCheck(object obj, Object ctx = null)
      public static bool FailsNullCheck(object obj, string msg, Object ctx = null) => false;
      public static bool FailsNullChecks(params object[] objs) => false;
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
