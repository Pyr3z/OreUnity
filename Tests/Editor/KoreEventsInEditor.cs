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
    var @event = new VoidEvent(isEnabled: true);

    @event.AddListener(StaticMethod);

    LogAssert.Expect(LogType.Log, "schmee.");

    Assert.True(@event.TryInvoke());

    Assert.Pass("no news is good news");
  }

} // end class OreEventsInEditor
