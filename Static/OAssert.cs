/** @file       Static/OAssert.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
 *  
 *  @remark     Moved from Orator.Assert (which is backwards-maintained).
**/

using UnityEngine;

using Conditional = System.Diagnostics.ConditionalAttribute;

using UnityAssert   = UnityEngine.Assertions.Assert;
using AssException  = UnityEngine.Assertions.AssertionException;


namespace Bore
{

  public /* static */ class OAssert
  {
    private static Orator  Orator => Orator.Instance;
    private const string    DEF_UNITY_ASSERTIONS = "UNITY_ASSERTIONS";
    private const string    MSG_NO_KONSOLE = "(note: " + nameof(Bore.Orator) + " not available)";

    private static readonly string NL = System.Environment.NewLine;
    private static readonly string FMT_NO_KONSOLE = "[OAssert] {0} {1}" + NL + "{2}";


#if UNITY_EDITOR
    [UnityEditor.MenuItem("Ore/Tests/Assertion Exception (No Orator)")]
    private static void Menu_TestAssertException()
    {
      throw new AssException("message", "userMessage");
    }

    [UnityEditor.MenuItem("Ore/Tests/Assertion Log (No Orator)")]
    private static void Menu_TestAssertLog()
    {
      LogNoOrator($"{Orator}");
    }

    [UnityEditor.MenuItem("Ore/Tests/Assertion Log")]
    private static void Menu_TestAssertLogOrator()
    {
      True(false, ctx: null);
    }

    [UnityEditor.MenuItem("Ore/Tests/Assertion Fails Null Checks")]
    private static void Menu_TestAssertNulls()
    {
      _ = FailsNullChecks(new object(), null, new object());
    }
#endif // UNITY_EDITOR


    private static void LogNoOrator(string msg)
    {
      Debug.LogAssertionFormat(FMT_NO_KONSOLE, Orator.DEFAULT_ASSERT_MSG, msg, MSG_NO_KONSOLE);
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void True(bool assertion, Object ctx = null)
    {
      if (!assertion)
      {
        if (Orator)
          Orator.assertionFailed(BoolFailMessage(expected: true), ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, BoolFailMessage(expected: true, ctx));
        else
          LogNoOrator(BoolFailMessage(expected: true, ctx));
      }
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void True(bool assertion, string msg, Object ctx = null)
    {
      if (!assertion)
      {
        if (Orator)
          Orator.assertionFailed(msg, ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, MessageContext(msg, ctx));
        else
          LogNoOrator(MessageContext(msg, ctx));
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllTrue(params bool[] assertions)
    {
      AllTrue(null, assertions);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllTrue(Object ctx, params bool[] assertions)
    {
      for (int i = 0, ilen = assertions?.Length ?? 0; i < ilen; ++i)
      {
        if (!assertions[i])
        {
          if (Orator)
            Orator.assertionFailed($"# {i+1}/{ilen}: {BoolFailMessage(expected: true)}", ctx);
          else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
            throw new AssException(MSG_NO_KONSOLE, $"{BoolFailMessage(expected: true, ctx)}{NL}(parameter: {i + 1}/{ilen})");
          else
            Debug.LogAssertion($"{BoolFailMessage(expected: true, ctx)}{NL}(parameter: {i + 1}/{ilen})", ctx);
          return;
        }
      }
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotNull(object reference, Object ctx = null)
    {
      if (reference == null)
      {
        if (Orator)
          Orator.assertionFailed(NullFailMessage(expected_null: false), ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, NullFailMessage(expected_null: false, ctx));
        else
          Debug.LogAssertion(NullFailMessage(expected_null: false, ctx), ctx);
      }
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotNull(object reference, string msg, Object ctx = null)
    {
      if (reference == null)
      {
        if (Orator)
          Orator.assertionFailed(msg, ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, MessageContext(msg, ctx));
        else
          Debug.LogAssertion(MessageContext(msg, ctx), ctx);
      }
    }


#if UNITY_ASSERTIONS
    // these shouldn't be compiled out, because their bool return values have logical meaning

    public static bool Fails(bool assertion, Object ctx = null)
    {
      if (assertion)
        return false;
      
      if (Orator)
        Orator.assertionFailedNoThrow(BoolFailMessage(expected: true), ctx);
      else
        Debug.LogAssertion(BoolFailMessage(expected: true, ctx), ctx);

      return true;
    }

    public static bool Fails(bool assertion, string msg, Object ctx = null)
    {
      if (assertion)
        return false;

      if (Orator)
        Orator.assertionFailedNoThrow(msg, ctx);
      else
        Debug.LogAssertion($"{Orator.DEFAULT_ASSERT_MSG} {msg}", ctx);

      return true;
    }

      
    public static bool FailsNullCheck(object obj, Object ctx = null)
    {
      if (!(obj is null))
        return false;

      if (Orator)
        Orator.assertionFailedNoThrow(NullFailMessage(expected_null: false), ctx);
      else
        Debug.LogAssertion(NullFailMessage(expected_null: false, ctx), ctx);

      return true;
    }

    public static bool FailsNullCheck(object obj, string msg, Object ctx = null)
    {
      if (!(obj is null))
        return false;

      if (Orator)
        Orator.assertionFailedNoThrow(msg, ctx);
      else
        Debug.LogAssertion($"{Orator.DEFAULT_ASSERT_MSG} {msg}", ctx);

      return true;
    }

    public static bool FailsNullChecks(params object[] objs)
    {
      for (int i = 0, ilen = objs?.Length ?? 0; i < ilen; ++i)
      {
        if (objs[i] is null)
        {
          if (Orator)
            Orator.assertionFailedNoThrow($"{NullFailMessage(expected_null: false)}{NL}(parameter: {i + 1}/{ilen})");
          else
            Debug.LogAssertion($"{NullFailMessage(expected_null: false)}{NL}(parameter: {i + 1}/{ilen})");
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


    private static string BoolFailMessage(bool expected, Object ctx = null)
    {
      if (ctx)
      {
        if (expected)
          return $"[{ctx.GetType().Name}] Value was false  (expected: true, context: \"{ctx.name}\").";
        else
          return $"[{ctx.GetType().Name}] Value was true  (expected: false, context: \"{ctx.name}\").";
      }
      else
      {
        if (expected)
          return $"[{nameof(OAssert)}] Value was false  (expected: true).";
        else
          return $"[{nameof(OAssert)}] Value was true  (expected: false).";
      }
    }

    private static string NullFailMessage(bool expected_null, Object ctx = null)
    {
      if (ctx)
      {
        if (expected_null)
          return $"[{ctx.GetType().Name}] Value was NOT null  (expected: null, context: \"{ctx.name}\").";
        else
          return $"[{ctx.GetType().Name}] Value was null  (expected: NOT null, context: \"{ctx.name}\").";
      }
      else
      {
        if (expected_null)
          return $"[{nameof(OAssert)}] Value was NOT null  (expected: null).";
        else
          return $"[{nameof(OAssert)}] Value was null  (expected: NOT null).";
      }
    }

    private static string MessageContext(string msg, Object ctx = null)
    {
      if (ctx)
        return $"[{ctx.GetType().Name}] {msg}  (context: \"{ctx.name}\")";
      else
        return $"[{nameof(OAssert)}] {msg}";
    }

  } // end static class OAssert

}
