/*! @file     Static/DeviceSpy.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-01-20
**/

using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{
  [PublicAPI]
  public static class DeviceSpy
  {
    public enum ABIArch
    {
      ARMv7   = 0,
      ARM64   = 1,
      x86     = 2, // x86* = ChromeOS
      x86_64  = 3,

      ARM     = ARMv7,
      ARM32   = ARMv7,
      ARMv8   = ARM64,
    }


    public static VersionID OSVersion
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (s_OSVersion is null)
          s_OSVersion = VersionID.ExtractOSVersion(SystemInfo.operatingSystem);
        return s_OSVersion;
      }
    }

    public static string Brand
    {
      get
      {
        if (s_Brand is null)
          (s_Brand, s_Model) = GetMakeModel();
        return s_Brand;
      }
    }

    public static string Model
    {
      get
      {
        if (s_Model is null)
          (s_Brand, s_Model) = GetMakeModel();
        return s_Model;
      }
    }

    public static string TimezoneUTCString
    {
      get
      {
        if (s_TimezoneUTCStr is null)
          (s_TimezoneOffset, s_TimezoneUTCStr) = GetTimezoneUTCOffset();
        return s_TimezoneUTCStr;
      }
    }

    public static float TimezoneOffset
    {
      get
      {
        if (s_TimezoneOffset is null)
          (s_TimezoneOffset, s_TimezoneUTCStr) = GetTimezoneUTCOffset();
        return (float)s_TimezoneOffset;
      }
    }

    public static float DiagonalInches
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (s_DiagonalInches is null)
          s_DiagonalInches = CalcScreenDiagonalInches();
        return (float)s_DiagonalInches;
      }
    }

    public static float AspectRatio
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (s_AspectRatio is null)
          s_AspectRatio = CalcAspectRatio();
        return (float)s_AspectRatio;
      }
    }

    public static bool IsTablet
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (s_IsTablet is null)
          s_IsTablet = CalcIsTablet();
        return (bool)s_IsTablet;
      }
    }

    public static bool IsBlueStacks
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (s_IsBlueStacks is null)
          s_IsBlueStacks = CalcIsBlueStacks();
        return (bool)s_IsBlueStacks;
      }
    }

    public static bool Is64Bit => ABI == ABIArch.ARM64 || s_ABIArch == ABIArch.x86_64;
    public static ABIArch ABI
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
        if (s_ABIArch is null)
          s_ABIArch = CalcABIArch();
        return (ABIArch)s_ABIArch;
      }
    }


    private static VersionID  s_OSVersion       = null;
    private static string     s_Brand           = null;
    private static string     s_Model           = null;
    private static string     s_TimezoneUTCStr  = null;
    private static float?     s_TimezoneOffset  = null;
    private static float?     s_DiagonalInches  = null;
    private static float?     s_AspectRatio     = null;
    private static bool?      s_IsTablet        = null;
    private static bool?      s_IsBlueStacks    = null;
    private static ABIArch?   s_ABIArch         = null;


    private static (string make, string model) GetMakeModel()
    {
#if UNITY_EDITOR

      return ("UNITY_EDITOR", SystemInfo.deviceModel);

#elif UNITY_IOS

      return ("Apple", SystemInfo.deviceModel);

#else

      string makemodel = SystemInfo.deviceModel;

      int split = makemodel.IndexOfAny(new []{ ' ', '-' });
      if (split < 0)
        return (makemodel, makemodel);

      return (makemodel.Remove(split), makemodel.Substring(split + 1));

#endif
    }

    private static (float off, string str) GetTimezoneUTCOffset()
    {
      var offset = System.TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now);
      return ((float)offset.TotalHours, $"{(offset.Hours <= 0 ? "" : "+")}{offset.Hours:00}{offset.Minutes:00}");
    }

    private static float CalcScreenDiagonalInches()
    {
      float w = Screen.width  / Screen.dpi;
      float h = Screen.height / Screen.dpi;
      return Mathf.Sqrt(w * w + h * h);
    }

    private static float CalcAspectRatio()
    {
      if (Screen.height < Screen.width)
        return (float)Screen.width / Screen.height;
      else
        return (float)Screen.height / Screen.width;
    }

    private static bool CalcIsTablet()
    {
#if AD_MEDIATION_MAX

      return MaxSdkUtils.IsTablet();

#else

      return Model.Contains("iPad") || CalcIsTabletByScreenSize();

#endif
    }

    private static bool CalcIsTabletByScreenSize()
    {
      const float MIN_DIAGONAL_INCHES = 6.5f;
      const float MAX_ASPECT_RATIO = 2.0f;
      return DiagonalInches > MIN_DIAGONAL_INCHES && AspectRatio < MAX_ASPECT_RATIO;
    }

    private static bool CalcIsBlueStacks()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
      foreach (string dir in new string[]{  "/sdcard/windows/BstSharedFolder",
                                            "/mnt/windows/BstSharedFolder" })
      {
        if (System.IO.Directory.Exists(dir))
          return true;
      }
#endif

      return false;
    }

    private static ABIArch CalcABIArch()
    {
#if !UNITY_EDITOR && UNITY_IOS // TODO iOS needs to be tested
      if (System.Environment.Is64BitOperatingSystem)
        return ABIArch.ARM64;
      else
        return ABIArch.ARM;
#endif // UNITY_IOS

      string type = SystemInfo.processorType;

      // Android and Android-like devices are pretty standard here
      if (type.StartsWith("ARM64"))
      {
        return ABIArch.ARM64;
      }
      else if (type.StartsWith("ARMv7"))
      {
        return ABIArch.ARM32;
      }

      // Chrome OS (should be a rare case)
      if (System.Environment.Is64BitOperatingSystem)
        return ABIArch.x86_64;
      else
        return ABIArch.x86;
    }

  } // end class DeviceSpy

}
