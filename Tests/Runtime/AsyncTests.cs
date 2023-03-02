/*! @file       Tests/Runtime/AsyncTests.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-02
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.TestTools;

using System.Collections;


internal static class AsyncTests
{

  [UnityTest]
  public static IEnumerator Futures()
  {
    IEnumerator subRoutine()
    {
      var promise = new Promise<int>();
      yield break;
    }

    yield return subRoutine();

    yield break;
  }

} // end class AsyncTests
