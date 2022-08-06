/** @file       Static/OAssert.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
 *  
 *  @remark     Moved from Orator.Assert (which is backwards-maintained).
**/

using System.Collections.Generic;

using UnityEngine;

using Conditional  = System.Diagnostics.ConditionalAttribute;
using UnityAssert  = UnityEngine.Assertions.Assert;
using AssException = UnityEngine.Assertions.AssertionException;


namespace Ore
{

  public /* static */ class OAssert
  {
    private static Orator Orator => Orator.Instance;
    private const string DEF_UNITY_ASSERTIONS = "UNITY_ASSERTIONS";
    private const string MSG_NO_KONSOLE = "(note: " + nameof(Ore.Orator) + " not available)";

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
    public static void True(bool value, Object ctx = null)
    {
      if (!value)
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
    public static void True(bool value, string msg, Object ctx = null)
    {
      if (!value)
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
    public static void AllTrue(params bool[] values)
    {
      AllTrue(null, values);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllTrue(Object ctx, params bool[] values)
    {
      for (int i = 0, ilen = values?.Length ?? 0; i < ilen; ++i)
      {
        if (!values[i])
        {
          if (Orator)
            Orator.assertionFailed($"# {i + 1}/{ilen}: {BoolFailMessage(expected: true)}", ctx);
          else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
            throw new AssException(MSG_NO_KONSOLE, $"{BoolFailMessage(expected: true, ctx)}{NL}(parameter: {i + 1}/{ilen})");
          else
            Debug.LogAssertion($"{BoolFailMessage(expected: true, ctx)}{NL}(parameter: {i + 1}/{ilen})", ctx);
          return;
        }
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void False(bool value, Object ctx = null)
    {
      if (value)
      {
        if (Orator)
          Orator.assertionFailed(BoolFailMessage(expected: false), ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, BoolFailMessage(expected: false, ctx));
        else
          LogNoOrator(BoolFailMessage(expected: false, ctx));
      }
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void False(bool value, string msg, Object ctx = null)
    {
      if (value)
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
    public static void AllFalse(params bool[] values)
    {
      AllFalse(null, values);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllFalse(Object ctx, params bool[] values)
    {
      for (int i = 0, ilen = values?.Length ?? 0; i < ilen; ++i)
      {
        if (values[i])
        {
          if (Orator)
            Orator.assertionFailed($"# {i + 1}/{ilen}: {BoolFailMessage(expected: false)}", ctx);
          else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
            throw new AssException(MSG_NO_KONSOLE, $"{BoolFailMessage(expected: false, ctx)}{NL}(parameter: {i + 1}/{ilen})");
          else
            Debug.LogAssertion($"{BoolFailMessage(expected: false, ctx)}{NL}(parameter: {i + 1}/{ilen})", ctx);
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


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllNotNull(params object[] references)
    {
      AllNotNull(null, references);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllNotNull(Object ctx, params object[] references)
    {
      for (int i = 0, ilen = references?.Length ?? 0; i < ilen; ++i)
      {
        if (references[i] == null)
        {
          if (Orator)
            Orator.assertionFailed($"# {i + 1}/{ilen}: {NullFailMessage(expected_null: false)}", ctx);
          else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
            throw new AssException(MSG_NO_KONSOLE, $"{NullFailMessage(expected_null: false, ctx)}{NL}(parameter: {i + 1}/{ilen})");
          else
            Debug.LogAssertion($"{NullFailMessage(expected_null: false, ctx)}{NL}(parameter: {i + 1}/{ilen})", ctx);
          return;
        }
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotEmpty<T>(ICollection<T> list, Object ctx = null)
    {
      if (list == null || list.Count == 0)
      {
        if (Orator)
          Orator.assertionFailed(CollectionEmptyMessage(expected_empty: false), ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, CollectionEmptyMessage(expected_empty: false, ctx));
        else
          Debug.LogAssertion(CollectionEmptyMessage(expected_empty: false, ctx), ctx);
      }
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotEmpty<T>(ICollection<T> list, string msg, Object ctx = null)
    {
      if (list == null || list.Count == 0)
      {
        if (Orator)
          Orator.assertionFailed(msg, ctx);
        else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
          throw new AssException(MSG_NO_KONSOLE, MessageContext(msg, ctx));
        else
          Debug.LogAssertion(MessageContext(msg, ctx), ctx);
      }
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllNotEmpty<T>(params ICollection<T>[] lists)
    {
      AllNotEmpty(null, lists);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void AllNotEmpty<T>(Object ctx, params ICollection<T>[] lists)
    {
      for (int i = 0, ilen = lists?.Length ?? 0; i < ilen; ++i)
      {
        if (lists[i] == null || lists[i].Count == 0)
        {
          if (Orator)
            Orator.assertionFailed($"# {i + 1}/{ilen}: {CollectionEmptyMessage(expected_empty: false)}", ctx);
          else if (Orator.DEFAULT_ASSERT_EXCEPTIONS)
            throw new AssException(MSG_NO_KONSOLE, $"{CollectionEmptyMessage(expected_empty: false, ctx)}{NL}(parameter: {i + 1}/{ilen})");
          else
            Debug.LogAssertion($"{CollectionEmptyMessage(expected_empty: false, ctx)}{NL}(parameter: {i + 1}/{ilen})", ctx);
          return;
        }
      }
    }


#if UNITY_ASSERTIONS
// This section shouldn't be compiled out, because their bool return values have logical meaning.
// It also shouldn't throw exceptions, even if Orator has "Assertions Throw Exceptions" turned on.

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

    private static string CollectionEmptyMessage(bool expected_empty, Object ctx = null)
    {
      if (ctx)
      {
        if (expected_empty)
          return $"[{ctx.GetType().Name}] Collection was NOT empty  (expected: EMPTY, context: \"{ctx.name}\").";
        else
          return $"[{ctx.GetType().Name}] Collection was EMPTY  (expected: NOT empty, context: \"{ctx.name}\").";
      }
      else
      {
        if (expected_empty)
          return $"[{nameof(OAssert)}] Collection was NOT empty  (expected: EMPTY).";
        else
          return $"[{nameof(OAssert)}] Collection was EMPTY  (expected: NOT empty).";
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
