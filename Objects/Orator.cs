/*! @file   Objects/Orator.cs
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

// ReSharper disable MemberCanBePrivate.Global

using System.ComponentModel;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Diagnostics;

using Debug         = UnityEngine.Debug;
using AssException  = UnityEngine.Assertions.AssertionException;
using Object = UnityEngine.Object;


namespace Ore
{
  [DefaultExecutionOrder(-1337)]
  [AssetPath("Resources/Orator.asset")]
  public sealed class Orator : OAssetSingleton<Orator>
  {
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

    public static void NFE(System.Exception ex, Object ctx = null)
    {
      // TODO replace this lazy wrapper
      Debug.LogException(ex, ctx);
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


    public static void Panic(string msg, Object ctx = null)
    {
      Error(msg, ctx);

    #if UNITY_EDITOR
      UnityEditor.EditorApplication.Beep();
      UnityEditor.EditorApplication.Beep();
      UnityEditor.EditorApplication.Beep();

      if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
      {
        UnityEditor.EditorApplication.ExitPlaymode();
      }

      if (ctx)
      {
        UnityEditor.EditorGUIUtility.PingObject(ctx);
      }
    #else
      Utils.ForceCrash(ForcedCrashCategory.Abort);
    #endif // UNITY_EDITOR
    }

    #endregion PUBLIC STATIC METHODS


    #region "Log Once" API

    public static void ReachedOnce(Object ctx)
    {
      if (AlreadyLogged(nameof(ReachedOnce), ctx))
        return;

      Reached(ctx);
    }

    public static void LogOnce(string msg, Object ctx = null)
    {
      if (AlreadyLogged(msg, ctx))
        return;

      Log(msg, ctx);
    }

    public static void WarnOnce(string msg, Object ctx = null)
    {
      if (AlreadyLogged(msg, ctx))
        return;

      Warn(msg, ctx);
    }

    public static void ErrorOnce(string msg, Object ctx = null)
    {
      if (AlreadyLogged(msg, ctx))
        return;

      Error(msg, ctx);
    }


    private static readonly HashSet<int> s_LoggedOnceHashes = new HashSet<int>();
    private static bool AlreadyLogged(string msg, Object ctx)
    {
      int cap = Instance ? Instance.m_LogOnceMemorySize : DEFAULT_LOGONCE_MEMORY_SIZE;
      int hash;

      #if UNITY_EDITOR

      var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
      if (stage)
      {
        hash = Hashing.MakeHash(msg, stage.assetPath);
      }
      else
      {
        hash = Hashing.MakeHash(msg, ctx);
      }

      #else // if !UNITY_EDITOR

      hash = Hashing.MakeHash(msg, ctx);

      #endif // UNITY_EDITOR

      if (s_LoggedOnceHashes.Count >= cap)
      {
        // Log($"LogOnce memory has overflowed. Resetting. (cap={cap})");
        s_LoggedOnceHashes.Clear();

        #if UNITY_EDITOR
        _ = Filesystem.TryDeletePath(CACHE_PATH);
        #endif // UNITY_EDITOR
      }

      return !s_LoggedOnceHashes.Add(hash);
    }

    #if UNITY_EDITOR

    private const string CACHE_PATH = "Temp/Orator.LogOnce.cache";

    [UnityEditor.InitializeOnLoadMethod]
    private static void OnScriptLoad()
    {
      if (!ReadCacheLogOnce() && s_LoggedOnceHashes.Count > 0)
      {
        if (!WriteCacheLogOnce())
        {
          Warn($"could not write to \"{CACHE_PATH}\"");
        }
      }

      UnityEditor.EditorApplication.wantsToQuit += () =>
      {
        _ = Filesystem.TryDeletePath(CACHE_PATH);
        return true;
      };
    }


    private static bool WriteCacheLogOnce()
    {
      if (OAssert.Fails(Filesystem.TryDeletePath(CACHE_PATH), $"could not delete \"{CACHE_PATH}\""))
        return false;
      if (s_LoggedOnceHashes.Count == 0)
        return true;

      var strb = new System.Text.StringBuilder(7 * s_LoggedOnceHashes.Count);

      foreach (int hash in s_LoggedOnceHashes)
      {
        _ = strb.Append(hash.ToInvariant()).Append('\n');
      }

      return Filesystem.TryWriteText(CACHE_PATH, strb.ToString());
    }

    private static bool ReadCacheLogOnce()
    {
      if (!Filesystem.TryReadLines(CACHE_PATH, out string[] lines))
        return false;

      s_LoggedOnceHashes.Clear();
      foreach (var line in lines)
      {
        if (Parsing.TryParseInt32(line, out int hash))
          s_LoggedOnceHashes.Add(hash);
      }

      return true;
    }

    #endif

    #endregion "Log Once" API


    #region STATIC ASSERTION API

    public /* static */ abstract class Assert : OAssert
    {

      // implementation now in `Static/OAssert.cs`.
      // You can still use this interface via `Orator.Assert.X()`, but `OAssert.X()` is shorter.

    } // end static class Assert

    #endregion STATIC ASSERTION API


    #region instance fields

    // compile-time default values:

    internal const EditorBrowsableState INSTANCE_BROWSABLE_POLICY = EditorBrowsableState.Never;

    internal const string DEFAULT_KONSOLE_PREFIX = "<color=\"orange\">[" + nameof(Orator) + "]</color>";
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

    internal const int DEFAULT_LOGONCE_MEMORY_SIZE = 256;


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

    [Space]

    [SerializeField, Tooltip("or, the maximum number of log signatures to keep in RAM to prevent duplicate logging.")]
    private int m_LogOnceMemorySize = DEFAULT_LOGONCE_MEMORY_SIZE;

    #endregion instance fields


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

    #if !UNITY_EDITOR // Remove rich text tags from strings in builds!
    private void Awake()
    {
      m_OratorPrefix = Strings.RemoveHypertextTags(m_OratorPrefix);
      m_ReachedFormat.BaseMessage = Strings.RemoveHypertextTags(m_ReachedFormat.BaseMessage);
      m_AssertionFailedFormat.BaseMessage = Strings.RemoveHypertextTags(m_AssertionFailedFormat.BaseMessage);
    }
    #endif

    #region (private section)

    private string ReachedMessage
    {
      get
      {
        if (m_FormattedReachedMessage == null)
          m_FormattedReachedMessage = $"{m_OratorPrefix} {m_ReachedFormat.BaseMessage}";
        // TODO RichText
        return m_FormattedReachedMessage;
      }
    }

    private string AssertMessage
    {
      get
      {
        if (m_FormattedAssertMessage == null)
          m_FormattedAssertMessage = $"{m_OratorPrefix} {m_AssertionFailedFormat.BaseMessage}";
        // TODO RichText
        return m_FormattedAssertMessage;
      }
    }


    private string m_FormattedReachedMessage = null;

    private string m_FormattedAssertMessage = null;

    // TODO fancy formatting w/ PyroDK.RichText API

    protected override void OnValidate()
    {
      base.OnValidate();

      // reset cached message strings
      m_FormattedReachedMessage = null;
      m_FormattedAssertMessage = null;
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
    [UnityEditor.MenuItem("Ore/Orator/Test All Log Types")]
    private static void Menu_TestLogs()
    {
      ReachedOnce(Instance);

      Reached();
      Log("message");
      Warn("warning");
      Error("error");
      OAssert.True(false, Instance);
      Log("(post-assert failure)");
    }

    [UnityEditor.MenuItem("Ore/Orator/Write Cache")]
    private static void Menu_WriteCache()
    {
      ReachedOnce(Instance);

      if (!WriteCacheLogOnce())
        Error("failed to write Orator cache!");
    }

    [UnityEditor.MenuItem("Ore/Orator/Clear Cache")]
    private static void Menu_ClearCache()
    {
      _ = Filesystem.TryDeletePath(CACHE_PATH);
      s_LoggedOnceHashes.Clear();
    }
#endif // UNITY_EDITOR

    #endregion (private section)

  } // end class Orator

}
