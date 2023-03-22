/*! @file       Tests/Editor/DeviceSpyInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-02-13
**/

using NUnit.Framework;

using UnityEngine;

using Ore;

using DateTime = System.DateTime;
using TimeSpan = System.TimeSpan;


// ReSharper disable once CheckNamespace
internal static class DeviceSpyInEditor
{

  [Test]
  public static void TimezoneOffset()
  {
    const long EPSILON = TimeSpan.TicksPerMillisecond;

    var (utcNow, localNow) = (DateTime.UtcNow, DateTime.Now);

    Assert.AreEqual(localNow.Ticks, (utcNow + DeviceSpy.TimezoneOffset).Ticks, EPSILON, "local == utc + timezone");
  }

  [Test]
  public static void TimezoneOffsetString()
  {
    string isoStr = DeviceSpy.TimezoneISOString;

    Debug.Log($"{nameof(DeviceSpy)}.{nameof(DeviceSpy.TimezoneISOString)}: {isoStr}");

    Assert.True(Parsing.TryParseTimezoneOffset(isoStr, out TimeSpan parsed), "TryParseTimezoneOffset(isoStr, out parsed)");

    var expected = System.TimeZoneInfo.Local.BaseUtcOffset;

    Assert.AreEqual(expected, parsed, $"{expected:g} == {parsed:g} == \"{isoStr}\"");
  }

  [Test]
  public static void Is64Bit()
  {
    // lol probably never gonna test the null of this

    #if UNITY_EDITOR_64
      Assert.True(DeviceSpy.Is64Bit, "DeviceSpy.Is64Bit");
    #elif UNITY_EDITOR
      Assert.False(DeviceSpy.Is64Bit, "DeviceSpy.Is64Bit");
    #endif
  }

} // end class DeviceSpyInEditor
