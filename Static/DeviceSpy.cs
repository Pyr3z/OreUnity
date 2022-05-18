/** @file     Runtime/StaticTypes/DeviceSpy.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-01-20
**/

using UnityEngine;


namespace Bore
{

  public static class DeviceSpy
  {
    public enum ABIArch
    {
      ARM     = 0,
      ARM64   = 1,
      x86     = 2,
      x86_64  = 3,
    }



    public static VersionID OSVersion
    {
      get
      {
        if (s_OSVersion == null)
          s_OSVersion = VersionID.ExtractOSVersion(SystemInfo.operatingSystem);
        return s_OSVersion;
      }
    }

    public static string Brand
    {
      get
      {
        if (s_Brand == null)
          (s_Brand, s_Model) = GetMakeModel();
        return s_Brand;
      }
    }

    public static string Model
    {
      get
      {
        if (s_Model == null)
          (s_Brand, s_Model) = GetMakeModel();
        return s_Model;
      }
    }

    public static string TimezoneUTCString
    {
      get
      {
        if (s_TimezoneUTCStr == null)
          (s_TimezoneOffset, s_TimezoneUTCStr) = GetTimezoneUTCOffset();
        return s_TimezoneUTCStr;
      }
    }

    public static float TimezoneOffset
    {
      get
      {
        if (s_TimezoneOffset == null)
          (s_TimezoneOffset, s_TimezoneUTCStr) = GetTimezoneUTCOffset();
        return (float)s_TimezoneOffset;
      }
    }

    public static float DiagonalInches
    {
      get
      {
        if (s_DiagonalInches == null)
          s_DiagonalInches = CalcScreenDiagonalInches();
        return (float)s_DiagonalInches;
      }
    }

    public static float AspectRatio
    {
      get
      {
        if (s_AspectRatio == null)
          s_AspectRatio = CalcAspectRatio();
        return (float)s_AspectRatio;
      }
    }

    public static bool IsTablet
    {
      get
      {
        if (s_IsTablet == null)
          s_IsTablet = CalcIsTablet();
        return (bool)s_IsTablet;
      }
    }

    public static bool IsBlueStacks
    {
      get
      {
        if (s_IsBlueStacks == null)
          s_IsBlueStacks = CalcIsBlueStacks();
        return (bool)s_IsBlueStacks;
      }
    }

    public static bool Is64Bit => ABI == ABIArch.ARM64 || s_ABIArch == ABIArch.x86_64;
    public static ABIArch ABI
    {
      get
      {
        if (s_ABIArch == null)
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

      int split = makemodel.IndexOfAny(new char[]{ ' ', '-' });
      if (split < 0)
        return (makemodel, makemodel);

      return (makemodel.Remove(split), makemodel.Substring(split + 1));

    #endif
    }

    private static (float off, string str) GetTimezoneUTCOffset()
    {
      var offset = System.TimeZone.CurrentTimeZone.GetUtcOffset(System.DateTime.Now);
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
      {
        return Screen.width / Screen.height;
      }
      else
      {
        return Screen.height / Screen.width;
      }
    }

    private static bool CalcIsTablet()
    {
    #if AD_MEDIATION_MAX

      return MaxSdkUtils.IsTablet();

    #elif UNITY_EDITOR

      return false;

    #else

      return Model.Contains("iPad") || CalcIsTabletByScreenSize();

    #endif
    }

    private static bool CalcIsTabletByScreenSize()
    {
      const float MIN_DIAGONAL_INCHES = 6.5f;
      const float MAX_ASPECT_RATIO    = 2.0f;
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
      string  type    = SystemInfo.processorType;
      var     strcmp  = System.StringComparison.OrdinalIgnoreCase;

      // Android and Android-like devices are pretty standard here
      if (type.StartsWith("ARM", strcmp))
      {
        if (type.Substring(3, 2) == "64")
          return ABIArch.ARM64;
        else
          return ABIArch.ARM;
      }
      else if (Application.platform == RuntimePlatform.IPhonePlayer)
      {
        if (System.Environment.Is64BitOperatingSystem)
          return ABIArch.ARM64;
        else
          return ABIArch.ARM;
      }
      else
      {
        if (System.Environment.Is64BitOperatingSystem)
          return ABIArch.x86_64;
        else
          return ABIArch.x86;
      }
    }

  } // end class DeviceSpy

}
