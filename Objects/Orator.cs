/*! @file       Objects/Orator.cs
    @author     levianperez\@gmail.com
    @author     levi\@leviperez.dev
    @date       2022-01-01

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

using JetBrains.Annotations;

using System.ComponentModel;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Diagnostics; // do not remove if unused

using ObsoleteAttribute = System.ObsoleteAttribute;
using AssException      = UnityEngine.Assertions.AssertionException;
using Type              = System.Type;


namespace Ore
{
  #if !DEBUG_KONSOLE
  [System.Diagnostics.DebuggerStepThrough]
  #endif
  [DefaultExecutionOrder(-1337)]
  [AutoCreateAsset("Resources/Orator.asset")]
  [PublicAPI]
  public sealed class Orator : OAssetSingleton<Orator>
  {

  #region Static public API

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

    public static void Reached([NotNull] Type tctx, string msg = DEFAULT_REACHED_MSG)
    {
      Reached($"[{tctx.Name}] {msg}");
    }

    public static void Reached<TContext>(string msg = DEFAULT_REACHED_MSG)
    {
      Reached(typeof(TContext), msg);
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

    public static void Log([NotNull] Type tctx, string msg)
    {
      Log($"[{tctx.Name}] {msg}");
    }

    public static void Log<TContext>(string msg)
    {
      Log(typeof(TContext), msg);
    }


    /// <summary>
    ///   Logs a "Non-Fatal Error" via the vanilla means.
    /// </summary>
    /// <param name="ex">
    ///   The Exception representing the error to log.
    ///   If null is supplied, this function is an immediate no-op.
    /// </param>
    /// <param name="ctx">
    ///   A context <see cref="Object"/> for the error.
    ///   If supplied and you are running in the editor, the editor will attempt
    ///   to ping this object. 
    /// </param>
    public static void NFE([CanBeNull] System.Exception ex, Object ctx = null)
    {
      if (ex is null)
        return;

      if (ex is FauxException faux)
      {
        #if DEBUG
          LogOnce(faux.Message, ctx);
        #endif
      }
      else
      {
        // TODO replace this lazy wrapper
        Debug.LogException(ex, ctx);
      }
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

    public static void Warn([NotNull] Type tctx, string msg)
    {
      Warn($"[{tctx.Name}] {msg}");
    }

    public static void Warn<TContext>(string msg)
    {
      Warn(typeof(TContext), msg);
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

    public static void Error([NotNull] Type tctx, string msg)
    {
      Error($"[{tctx.Name}] {msg}");
    }

    public static void Error<TContext>(string msg)
    {
      Error(typeof(TContext), msg);
    }


    public static void FailAssertion(string msg)
    {
      FailAssertion(msg, null);
    }

    public static void FailAssertion(string msg, Object ctx)
    {
      #pragma warning disable CS0162
      // ReSharper disable HeuristicUnreachableCode

      if (Instance)
      {
        Instance.assertionFailed(msg, ctx);
        return;
      }

      if (DEFAULT_INCLUDE_CONTEXT && ctx)
      {
        msg = AppendContext(msg, ctx);
      }

      if (DEFAULT_ASSERT_EXCEPTIONS)
      {
        throw new AssException(DEFAULT_ASSERT_MSG, msg);
      }

      Debug.LogFormat(DEFAULT_ASSERT_LOGTYPE, DEFAULT_ASSERT_LOGOPT,
                      ctx, "{0} {1}", DEFAULT_ASSERT_MSG, msg);

      // ReSharper restore HeuristicUnreachableCode
      #pragma warning restore CS0162
    }

    public static void FailAssertionNoThrow(string msg)
    {
      FailAssertionNoThrow(msg, null);
    }

    public static void FailAssertionNoThrow(string msg, Object ctx)
    {
      if (Instance)
      {
        Instance.assertionFailedNoThrow(msg, ctx);
        return;
      }

      if (DEFAULT_INCLUDE_CONTEXT && ctx)
      {
        msg = AppendContext(msg, ctx);
      }

      Debug.LogFormat(DEFAULT_ASSERT_LOGTYPE, DEFAULT_ASSERT_LOGOPT,
                      ctx, "{0} {1}", DEFAULT_ASSERT_MSG, msg);
    }


    public static void Panic(string msg)
    {
      Panic(msg, Instance);
    }

    public static void Panic(Object ctx)
    {
      Panic(string.Empty, ctx);
    }

    public static void Panic(string msg, Object ctx)
    {
      Error($"PANIC! {msg}", ctx);

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

    public static void Panic([NotNull] Type tctx)
    {
      Panic($"[{tctx.Name}]", null);
    }

    public static void Panic<TContext>()
    {
      Panic(typeof(TContext));
    }

  #endregion Static public API

  #region Static "Log Once" API

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

      if (cap <= 0)
      {
        return false;
      }

      int hash;

      #if UNITY_EDITOR

        #if UNITY_2021_1_OR_NEWER
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        #else
        var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        #endif

        #if UNITY_2020_3_OR_NEWER
        if (stage)
        {
          hash = Hashing.MakeHash(msg, stage.assetPath);
        }
        #else
        if (stage != null && stage.prefabContentsRoot)
        {
          hash = Hashing.MakeHash(msg, stage.prefabAssetPath);
        }
        #endif
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

  #endregion Static "Log Once" API

  #region Instance public API

    /* IDE1006 => public member name does not match style guide */
    #pragma warning disable IDE1006
    // lowercase function names = convention for instance versions of static methods


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void reached()
    {
      reached(string.Empty, null);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void reached(Object ctx, string msg = null)
    {
      reached(msg, ctx);
    }

    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void reached(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      // TODO improved stacktrace info from PyroDK

      Debug.LogFormat(m_ReachedFormat.LogType, m_ReachedFormat.LogOption, ctx, "{0} {1}", ReachedMessage, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void assertionFailed(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      // TODO improved stacktrace info from PyroDK

      if (m_AssertionsRaiseExceptions)
        throw new AssException(AssertMessage, msg);

      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption,
                      ctx, "{0} {1}", AssertMessage, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void assertionFailedNoThrow(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      // TODO improved stacktrace info from PyroDK

      Debug.LogFormat(m_AssertionFailedFormat.LogType, m_AssertionFailedFormat.LogOption,
                      ctx, "{0} {1}", AssertMessage, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void log(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      Debug.LogFormat(LogType.Log, m_LogStackTracePolicy, ctx, "{0} {1}", m_OratorPrefix, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void warn(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      Debug.LogFormat(LogType.Warning, LogOption.None, ctx, "{0} {1}", m_OratorPrefix, msg);
    }


    [EditorBrowsable(INSTANCE_BROWSABLE_POLICY)]
    private void error(string msg, Object ctx = null)
    {
      FixupMessageContext(ref msg, ref ctx);

      Debug.LogFormat(LogType.Error, LogOption.None, ctx, "{0} {1}", m_OratorPrefix, msg);
    }


    #pragma warning restore IDE1006

  #endregion Instance public API

  #region Instance fields

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
        // TODO: would be SO easy to finish hooking this up
        // TODO: if we moved Ore.Editor.Styles.ColorText(...) into runtime code!
    } // end struct LogFormatDef

    // compile-time default values:

    const EditorBrowsableState INSTANCE_BROWSABLE_POLICY = EditorBrowsableState.Never;

    const string    DEFAULT_KONSOLE_PREFIX        = "<color=\"orange\">[" + nameof(Orator) + "]</color>";
    const bool      DEFAULT_INCLUDE_CONTEXT       = true;
    const LogOption DEFAULT_LOG_LOGOPT            = LogOption.NoStacktrace;

    const string    DEFAULT_REACHED_MSG           = "<b>Reached!</b>";
    const LogType   DEFAULT_REACHED_LOGTYPE       = LogType.Warning;
    const LogOption DEFAULT_REACHED_LOGOPT        = LogOption.None;

    const LogType   DEFAULT_ASSERT_LOGTYPE        = LogType.Assert;
    const string    DEFAULT_ASSERT_MSG            = "<b>Assertion failed!</b>";
    const LogOption DEFAULT_ASSERT_LOGOPT         = LogOption.None;
    const bool      DEFAULT_ASSERT_EXCEPTIONS     = true;
    const bool      DEFAULT_ASSERTIONS_IN_RELEASE = false;

    const int       DEFAULT_LOGONCE_MEMORY_SIZE   = 256;


    [Header("Orator Properties")]

    [SerializeField]
    private string m_OratorPrefix = DEFAULT_KONSOLE_PREFIX;

    [SerializeField]
    private bool m_IncludeContextInMessages = DEFAULT_INCLUDE_CONTEXT;

    [SerializeField]
    private LogOption m_LogStackTracePolicy = DEFAULT_LOG_LOGOPT;

    [SerializeField]
    private bool m_AssertionsRaiseExceptions = DEFAULT_ASSERT_EXCEPTIONS;

  #pragma warning disable CS0414
    [SerializeField]
    [HideInInspector, Obsolete("Assertions in release have never been fully implemented.", error: true)]
    private bool m_ForceAssertionsInRelease = DEFAULT_ASSERTIONS_IN_RELEASE;  // TODO
  #pragma warning restore CS0414

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

  #endregion Instance fields

  #region Internal section

    #if !UNITY_EDITOR // Remove rich text tags from strings in builds!
    private void Awake()
    {
      m_OratorPrefix = Strings.RemoveHypertextTags(m_OratorPrefix);
      m_ReachedFormat.BaseMessage = Strings.RemoveHypertextTags(m_ReachedFormat.BaseMessage);
      m_AssertionFailedFormat.BaseMessage = Strings.RemoveHypertextTags(m_AssertionFailedFormat.BaseMessage);
    }
    #endif


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
      }
      else if (m_IncludeContextInMessages)
      {
        msg = AppendContext(msg, ctx);
      }

      // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
      if (msg is null)
      {
        msg = string.Empty;
      }
    }

    private static string AppendContext(string msg, [NotNull] Object ctx)
    {
      string name = ctx.name;
      if (name.IsEmpty())
        name = "<no name>";

      if (ctx is UnityEngine.Component c)
      {
        // TODO construct full scene path for scene objects
        return $"[{ctx.GetType().Name}] {msg}\n(scene path: \"{c.gameObject.scene.name}/{name}\")";
      }

      if (ctx is GameObject go)
      {
        // TODO construct full scene path for scene objects
        return $"[{ctx.GetType().Name}] {msg}\n(scene path: \"{go.scene.name}/{name}\")";
      }

      return $"[{ctx.GetType().Name}] {msg}\n(asset name: \"{name}\")";
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

  #endregion Internal section

  } // end class Orator

}
