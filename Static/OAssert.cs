/*! @file       Static/OAssert.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-01
 *
 *  @remark     Moved from Orator.Assert, which was entirely removed in 2.12.
**/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

using Conditional  = System.Diagnostics.ConditionalAttribute;
using UnityAssert  = UnityEngine.Assertions.Assert;
using AssException = UnityEngine.Assertions.AssertionException;


namespace Ore
{
  [PublicAPI]
  public static class OAssert
  {
    private const string DEF_UNITY_ASSERTIONS = "UNITY_ASSERTIONS";

  #region Public API

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void True(bool value, Object ctx = null)
    {
      if (value)
        return;

      Orator.FailAssertion(FAIL_BOOL_F, ctx);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void True(bool value, string msg, Object ctx = null)
    {
      if (value)
        return;

      Orator.FailAssertion(msg, ctx);
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
        if (values[i])
          continue;

        Orator.FailAssertion(ForParameter(FAIL_BOOL_F, i, ilen), ctx);

        return;
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void False(bool value, Object ctx = null)
    {
      if (!value)
        return;

      Orator.FailAssertion(FAIL_BOOL_T, ctx);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void False(bool value, string msg, Object ctx = null)
    {
      if (!value)
        return;

      Orator.FailAssertion(msg, ctx);
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
        if (!values[i])
          continue;

        Orator.FailAssertion(ForParameter(FAIL_BOOL_T, i, ilen), ctx);

        return;
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotNull(object reference, Object ctx = null)
    {
      if (reference != null)
        return;

      Orator.FailAssertion(FAIL_NULL_T, ctx);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotNull(object reference, string msg, Object ctx = null)
    {
      if (reference != null)
        return;

      Orator.FailAssertion(msg, ctx);
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
        if (references[i] != null)
          continue;

        Orator.FailAssertion(ForParameter(FAIL_NULL_T, i, ilen), ctx);

        return;
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void Exists(Object obj, Object ctx = null)
    {
      if (obj)
        return;

      Orator.FailAssertion(FAIL_NULL_T, ctx);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void Exists(Object obj, string msg, Object ctx = null)
    {
      if (obj)
        return;

      Orator.FailAssertion(msg, ctx);
    }

    public static void AllExist(params Object[] objs)
    {
      for (int i = 0, ilen = objs?.Length ?? 0; i < ilen; ++i)
      {
        if (objs[i])
          continue;

        Orator.FailAssertion(ForParameter(FAIL_NULL_T, i, ilen));

        return;
      }
    }


    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotEmpty<T>(ICollection<T> list, Object ctx = null)
    {
      if (list != null && list.Count != 0)
        return;

      Orator.FailAssertion(FAIL_EMPTY_T, ctx);
    }

    [Conditional(DEF_UNITY_ASSERTIONS)]
    public static void NotEmpty<T>(ICollection<T> list, string msg, Object ctx = null)
    {
      if (list != null && list.Count != 0)
        return;

      Orator.FailAssertion(msg, ctx);
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
        if (lists[i] != null && lists[i].Count != 0)
          continue;

        Orator.FailAssertion(ForParameter(FAIL_EMPTY_T, i, ilen), ctx);

        return;
      }
    }

  #endregion Public API

  #region Boolean return value API

    #if UNITY_ASSERTIONS
    // This section shouldn't be compiled out, because their bool return values have logical meaning.
    // It also shouldn't throw exceptions, even if Orator has "Assertions Throw Exceptions" turned on.

      public static bool Fails(bool assertion, Object ctx = null)
      {
        if (assertion)
          return false;

        Orator.FailAssertionNoThrow(FAIL_BOOL_F, ctx);

        return true;
      }

      public static bool Fails(bool assertion, string msg, Object ctx = null)
      {
        if (assertion)
          return false;

        Orator.FailAssertionNoThrow(msg, ctx);

        return true;
      }


      public static bool FailsNullCheck(object obj, Object ctx = null)
      {
        if (!(obj is null))
          return false;

        Orator.FailAssertionNoThrow(FAIL_NULL_T, ctx);

        return true;
      }

      public static bool FailsNullCheck(object obj, string msg, Object ctx = null)
      {
        if (!(obj is null))
          return false;

        Orator.FailAssertionNoThrow(msg, ctx);

        return true;
      }

      public static bool FailsNullChecks(params object[] objs)
      {
        for (int i = 0, ilen = objs?.Length ?? 0; i < ilen; ++i)
        {
          if (!(objs[i] is null))
            continue;

          Orator.FailAssertionNoThrow(ForParameter(FAIL_NULL_T, i, ilen));

          return true;
        }

        return false;
      }

    #else // !UNITY_ASSERTIONS

      #pragma warning disable IDE0060

      public static bool Fails(bool assertion, Object ctx = null) => false;
      public static bool Fails(bool assertion, string msg, Object ctx = null) => false;
      public static bool FailsNullCheck(object obj, Object ctx = null) => false;
      public static bool FailsNullCheck(object obj, string msg, Object ctx = null) => false;
      public static bool FailsNullChecks(params object[] objs) => false;

      #pragma warning restore IDE0060

    #endif // UNITY_ASSERTIONS

  #endregion Boolean return value API


  #region Private section

    // MenuItems moved to unit tests (try in: MiscInEditor.cs)

    private const string FAIL_BOOL_T  = "Value was true  (expected: false).";
    private const string FAIL_BOOL_F  = "Value was false  (expected: true).";

    private const string FAIL_NULL_T  = "Value was null  (expected: NOT null).";
    private const string FAIL_NULL_F  = "Value was NOT null  (expected: null).";

    private const string FAIL_EMPTY_T = "Collection was empty  (expected: NOT empty).";
    private const string FAIL_EMPTY_F = "Collection was NOT empty  (expected: empty).";

    private static string ForParameter(string msg, int i, int ilen)
    {
      return $"Parameter #{i+1} (of {ilen}): {msg}";
    }

  #endregion Private section

  } // end static class OAssert

}
