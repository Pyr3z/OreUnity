/*! @file       Tests/Editor/OreEventsInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-03-21
**/

using Ore;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;


// ReSharper disable once CheckNamespace
internal static class OreEventsInEditor
{

  static int s_StaticMethodCount;

  static void StaticMethod()
  {
    ++ s_StaticMethodCount;
  }


  [Test]
  public static void AddStatic()
  {
    var vdEvent = new VoidEvent(isEnabled: true);

    int start = s_StaticMethodCount;

    vdEvent.AddListener(StaticMethod);

    Assert.True(vdEvent.TryInvoke());

    Assert.AreEqual(s_StaticMethodCount, start + 1);

    vdEvent -= StaticMethod;

    Assert.NotNull(vdEvent);

    Assert.True(vdEvent.TryInvoke());

    Assert.AreEqual(s_StaticMethodCount, start + 1);

    vdEvent += StaticMethod;

    Assert.True(vdEvent.TryInvoke());

    Assert.AreEqual(s_StaticMethodCount, start + 2);

    Assert.Pass("no news is good news");
  }

} // end class OreEventsInEditor
