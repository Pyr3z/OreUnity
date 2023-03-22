/*! @file       Tests/Editor/OreEventsInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-21
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;


internal static class OreEventsInEditor
{

  public static void StaticMethod()
  {
    Debug.Log("shmee.");
  }


  [Test]
  public static void AddStatic()
  {
    var vdEvent = new VoidEvent(isEnabled: true);

    vdEvent.AddListener(StaticMethod);

    LogAssert.Expect(LogType.Log, "schmee.");

    Assert.True(vdEvent.TryInvoke());

    vdEvent -= StaticMethod;

    Assert.NotNull(vdEvent);

    LogAssert.NoUnexpectedReceived();

    Assert.True(vdEvent.TryInvoke());

    vdEvent += StaticMethod;

    LogAssert.Expect(LogType.Log, "schmee.");

    Assert.True(vdEvent.TryInvoke());

    Assert.Pass("no news is good news");
  }

} // end class OreEventsInEditor
