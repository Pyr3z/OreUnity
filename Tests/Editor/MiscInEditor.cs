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

} // end class MiscInEditor
