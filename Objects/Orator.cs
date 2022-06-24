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
**/

using UnityEngine;

using ConditionalAttribute = System.Diagnostics.ConditionalAttribute;


namespace Bore
{

  public sealed class Orator : OAssetSingleton<Orator>, IImmortalSingleton
  {

#region instance fields

  [Header("Orator Properties")]
    [SerializeField]
    private string m_LogPrefix = "[KooBox]";
    [SerializeField]
    private bool m_ForceAssertionsInRelease = false;

#endregion instance fields



    public static void Log(string msg)
    {
      if (Instance)
        Instance.log(msg);
    }

    public static void Warn(string msg)
    {
      if (Instance)
        Instance.warn(msg);
    }

    public static void Error(string msg)
    {
      if (Instance)
        Instance.error(msg);
    }


#region STATIC UNITY_ASSERTIONS

    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool assertion) => Debug.Assert(assertion);
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool assertion, string msg) => Debug.Assert(assertion, msg);
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool assertion, object msg) => Debug.Assert(assertion, msg);
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool assertion, Object ctx) => Debug.Assert(assertion, ctx);
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool assertion, string msg, Object ctx) => Debug.Assert(assertion, msg, ctx);
    [Conditional("UNITY_ASSERTIONS")]
    public static void Assert(bool assertion, object msg, Object ctx) => Debug.Assert(assertion, msg, ctx);


  #if UNITY_ASSERTIONS
    public static bool AssertFails(bool assertion)
    {
      if (assertion)
        return false;
      
      if (Instance)
        Instance.error(DefaultAssertMessage);
      
      return true;
    }

    public static bool AssertFails(bool assertion, object msg)
    {
      if (assertion)
        return false;

      if (Instance)
        Instance.error($"{DefaultAssertMessage} \"{msg}\"");
      return true;
    }
  #else
    #pragma warning disable IDE0060
    public static bool AssertFails(bool _ ) => false;
    public static bool AssertFails(bool _ , object __ ) => false;
    #pragma warning restore IDE0060
  #endif // UNITY_ASSERTIONS

#endregion STATIC UNITY_ASSERTIONS


#region instance methods

    // lowercase function names = convention for instance versions of static methods
    #pragma warning disable IDE1006
    public void log(string msg)
    {
      // TODO fancy stuff from PyroDK
      Debug.Log($"{m_LogPrefix} {msg}");
    }

    public void warn(string msg)
    {
      // TODO fancy stuff from PyroDK
      Debug.LogWarning($"{m_LogPrefix} {msg}");
    }

    public void error(string msg)
    {
      // TODO fancy stuff from PyroDK
      Debug.LogError($"{m_LogPrefix} {msg}");
    }
    #pragma warning restore IDE1006

#endregion instance methods


#region STATIC PRIVATE DATA

    private static string s_DefaultAssertMessage = null;
    private static string DefaultAssertMessage
    {
      get
      {
        if (s_DefaultAssertMessage == null)
          // TODO reimplement w/ PyroDK.RichText API
          return s_DefaultAssertMessage = "Assertion failed!";

        return s_DefaultAssertMessage;
      }
    }

#endregion STATIC PRIVATE DATA

  } // end class Orator

}
