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
  public static IEnumerator PromiseSuccess()
  {
    var promise = new Promise<string>();

    static IEnumerator independentRoutine(Promise<string> outPromise)
    {
      yield return new WaitForFrames(Random.Range(1,30));
      outPromise.CompleteWith("as promised!");
    }

    IEnumerator dependentRoutine(Promise<string> inPromise)
    {
      yield return inPromise;

    }

    yield break;
  }

} // end class AsyncTests
