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

  class PromiseCoroutineTester : MonoBehaviour, IMonoBehaviourTest
  {
    public bool IsTestFinished { get; private set; }

    void Start()
    {
      var promise = new Promise<string>();

      _ = StartCoroutine(Provide(promise));
      _ = StartCoroutine(Consume(promise));
    }

    IEnumerator Provide(Promise<string> outPromise)
    {
      /**/ Assert.False(outPromise.IsCompleted);

      yield return new WaitForFrames(Random.Range(1,60));

      /**/ Assert.False(outPromise.IsCompleted);

      outPromise.CompleteWith("as promised!");

      /**/ Assert.True(outPromise.Succeeded);
    }

    IEnumerator Consume(Promise<string> inPromise)
    {
      /**/ Assert.False(inPromise.IsCompleted);

      inPromise.OnSucceeded += Debug.Log;

      /**/ LogAssert.Expect(LogType.Log, "as promised!");

      yield return inPromise;

      /**/ Assert.True(inPromise.Succeeded);

      /**/ LogAssert.Expect(LogType.Log, "as promised!");
      inPromise.OnSucceeded += Debug.Log;

      IsTestFinished = true;
    }
  }


  [UnityTest]
  public static IEnumerator CoroutinePromises()
  {
    var test = new MonoBehaviourTest<PromiseCoroutineTester>();
    return test;
  }

} // end class AsyncTests
