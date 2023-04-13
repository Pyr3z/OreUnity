/*! @file       Tests/Editor/MiscInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-28
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Text.RegularExpressions;

using AssException = UnityEngine.Assertions.AssertionException;


// ReSharper disable once CheckNamespace
internal static class MiscInEditor
{

  [Test]
  public static void MultiException()
  {
    var nse = new System.NotSupportedException("(top)");
    var ioe = new System.InvalidOperationException("(middle)");
    var nie = new System.NotImplementedException("(bottom)");

    Assert.Throws<MultiException>(() =>
    {
      var mex = Ore.MultiException.Create(nse, ioe, nie);
      LogAssert.Expect(LogType.Exception, new Regex(@"NotSupportedException: \(top\)"));
      Orator.NFE(mex);
      throw mex;
    });
  }

  [Test]
  public static void AssException()
  {
    Assert.Throws<AssException>(() => throw new AssException("message", "userMessage"));
  }

  [Test]
  public static void OAssertExceptionContract()
  {
    bool expectException = Orator.RaiseExceptions;

    if (expectException)
    {
      Assert.Throws<AssException>(() => OAssert.True(false));
    }
    else
    {
      Assert.DoesNotThrow(() => OAssert.True(false));
    }

    bool ok = false;
    Assert.DoesNotThrow(() =>
    {
      bool pop = LogAssert.ignoreFailingMessages;
      LogAssert.ignoreFailingMessages = true;

      ok = OAssert.Fails(false) &&
          !OAssert.Fails(true) &&
           OAssert.FailsNullCheck(null) &&
           OAssert.FailsNullChecks(new object(), null, new object());

      LogAssert.ignoreFailingMessages = pop;
    });
    Assert.True(ok);
  }


  [Test]
  public static void ActionNullability()
  {
    System.Action action = null;

    Assert.DoesNotThrow(() =>
                        {
                          action += Orator.Reached;
                        });

    Assert.NotNull(action);

    Assert.DoesNotThrow(() =>
                        {
                          action -= Orator.Reached;
                        });

    Assert.Null(action);

    Assert.DoesNotThrow(() =>
                        {
                          action -= Orator.Reached;
                        });

    Assert.Null(action);
  }


  [Test]
  public static void RecycledStringBob()
  {
    int precount = RecycledStringBuilder.AliveCount;

    using (new RecycledStringBuilder(out var bob))
    {
      Assert.NotNull(bob);
      Assert.AreEqual(precount + 1, RecycledStringBuilder.AliveCount, "in scope AliveCount");

      bob.Append("fef");

      Assert.AreEqual(3, bob.Length, "bob.Length");

      Assert.AreEqual("fef", bob.ToString(), "bob.ToString()");
    }

    Assert.AreEqual(precount, RecycledStringBuilder.AliveCount, "post scope AliveCount");
  }

  [Test]
  public static void MappedStructIsImmutable()
  {
    // (a proof)

    var map = new HashMap<string,Vector3>
    {
      ["fef"] = new Vector3(1, 2, 3)
    };

    bool sanity = map.Find("fef", out var strct);
    Assert.True(sanity, "map[\"fef\"]");

    Assert.AreEqual(1f, strct.x, "strct.x");

    strct.x = 3.14f;

    Debug.Log(strct);

    sanity = map.Find("fef", out var again);
    Assert.True(sanity, "map[\"fef\"] (again)");

    Debug.Log(again);

    Assert.AreNotEqual(3.14f, again.x, "again.x");

    // new FindRef method should alleviate this:

    ref var vecref = ref map.FindRef("fef", out sanity);
    Assert.True(sanity, "map.FindRef(\"fef\")");

    vecref.x = 1337f;

    sanity = map.Find("fef", out again);
    Assert.True(sanity, "map[\"fef\"] (again again)");

    Debug.Log(again);

    Assert.AreEqual(1337f, again.x, "again.x");
  }

  [Test]
  public static void StringEqualityIgnoreWhitespace()
  {
    string fef = "fef";
    string[] tests =
    {
      " fef\n\r",
      "\tf e f ",
      " fe  \n f ",
      "fef",
    };

    foreach (var test in tests)
    {
      Assert.True(Strings.AreEqualIgnoreWhitespace(fef, test));
    }

    foreach (var test in tests)
    {
      Assert.True(Strings.AreEqualIgnoreWhitespace(test, fef));
    }

    Assert.False(Strings.AreEqualIgnoreWhitespace(fef, "bub"));
  }

} // end class MiscInEditor
