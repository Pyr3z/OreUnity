/*! @file     Static/DeviceSpy.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-01-20
**/

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

using TimeSpan = System.TimeSpan;

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


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


    // ReSharper disable ConvertToNullCoalescingCompoundAssignment

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
          (s_Brand, s_Model) = CalcMakeModel();
        return s_Brand;
      }
    }

    public static string Model
    {
      get
      {
        if (s_Model is null)
          (s_Brand, s_Model) = CalcMakeModel();
        return s_Model;
      }
    }

    public static string Browser => s_Browser ?? (s_Browser = CalcBrowserName());

    public static string Carrier => s_Carrier ?? (s_Carrier = CalcCarrier());

    public static string LanguageISO6391 => s_LangISO6391 ?? (s_LangISO6391 = ToISO6391(Application.systemLanguage));

    public static TimeSpan TimezoneOffset => (TimeSpan)(s_TimezoneOffset ?? (s_TimezoneOffset = CalcTimezoneOffset()));

    public static float TimezoneOffsetHours => (float)TimezoneOffset.TotalHours;

    public static string TimezoneISOString => Strings.MakeISOTimezone(TimezoneOffset);

    public static float DiagonalInches => (float)(s_DiagonalInches ?? (s_DiagonalInches = CalcScreenDiagonalInches()));

    public static float AspectRatio => (float)(s_AspectRatio ?? (s_AspectRatio = CalcAspectRatio()));

    public static bool IsTablet => (bool)(s_IsTablet ?? (s_IsTablet = CalcIsTablet()));

    public static bool IsBlueStacks => (bool)(s_IsBlueStacks ?? (s_IsBlueStacks = CalcIsBlueStacks()));

    public static bool Is64Bit => ABI == ABIArch.ARM64 || s_ABIArch == ABIArch.x86_64;

    public static ABIArch ABI => (ABIArch)(s_ABIArch ?? (s_ABIArch = CalcABIArch()));


    public static int CalcRAMUsageMiB()
    {
      return (int)(CalcRAMUsageBytes() / BYTES_PER_MIB);
    }

    public static float CalcRAMUsagePercent()
    {
      return (float)CalcRAMUsageBytes() / BYTES_PER_MB / SystemInfo.systemMemorySize.AtLeast(1);
    }


    public static string ToJSON(bool prettyPrint)
    {
      var json = new JObject();

      foreach (var property in typeof(DeviceSpy).GetProperties(TypeMembers.STATIC))
      {
        try
        {
          var value = property.GetValue(null);
          if (value is TimeSpan span)
          {
            json[property.Name] = new JValue(span.Ticks);
          }
          else
          {
            json[property.Name] = new JValue(value?.ToString());
          }
        }
        catch (System.Exception e)
        {
          Orator.NFE(e);
          json[property.Name] = JValue.CreateNull();
        }
      }

      return json.ToString(prettyPrint ? Formatting.Indented : Formatting.None);
    }

  #if UNITY_EDITOR

    [UnityEditor.MenuItem("Ore/Log/DeviceSpy (JSON)")]
    private static void Menu_LogJSON()
    {
      Orator.Log(ToJSON(prettyPrint: true));
    }

  #endif // UNITY_EDITOR


#region Private section

    private const long BYTES_PER_MIB = 1048576L; // = pow(2,20)
    private const long BYTES_PER_MB  = 1000000L;

    private static VersionID  s_OSVersion       = null;
    private static string     s_Brand           = null;
    private static string     s_Model           = null;
    private static string     s_Browser         = null;
    private static string     s_Carrier         = null;
    private static string     s_LangISO6391     = null;
    private static TimeSpan?  s_TimezoneOffset  = null;
    private static float?     s_DiagonalInches  = null;
    private static float?     s_AspectRatio     = null;
    private static bool?      s_IsTablet        = null;
    private static bool?      s_IsBlueStacks    = null;
    private static ABIArch?   s_ABIArch         = null;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long CalcRAMUsageBytes()
    {
      #if UNITY_2020_1_OR_NEWER
      return UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
      #else
      return System.GC.GetTotalMemory(forceFullCollection: false);
      #endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan CalcTimezoneOffset()
    {
      // TODO there might be a better (100x faster) Java API to call for Android ~
      return System.TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now);
    }


    private static (string make, string model) CalcMakeModel()
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

    private static string CalcBrowserName()
    {
      #if UNITY_ANDROID
        return CalcAndroidBrowser();
      #elif UNITY_IOS
        return string.Empty;
      #elif UNITY_WEBGL // TODO: doable, but requires javascript plugin
        return string.Empty;
      #else
        return string.Empty;
      #endif
    }

    private static string CalcCarrier()
    {
      #if UNITY_ANDROID
        return CalcAndroidCarrier();
      #elif UNITY_IOS
        return string.Empty;
      #elif UNITY_WEBGL
        return string.Empty;
      #else
        return string.Empty;
      #endif
    }

    private static string ToISO6391(SystemLanguage lang) // TODO refactor to new extension class
    {
      switch (lang)
      {
        case SystemLanguage.Afrikaans:
          return "AF";
        case SystemLanguage.Arabic:
          return "AR";
        case SystemLanguage.Basque:
          return "EU";
        case SystemLanguage.Belarusian:
          return "BY";
        case SystemLanguage.Bulgarian:
          return "BG";
        case SystemLanguage.Catalan:
          return "CA";
        case SystemLanguage.Chinese:
          return "ZH";
        case SystemLanguage.Czech:
          return "CS";
        case SystemLanguage.Danish:
          return "DA";
        case SystemLanguage.Dutch:
          return "NL";
        case SystemLanguage.English:
          return "EN";
        case SystemLanguage.Estonian:
          return "ET";
        case SystemLanguage.Faroese:
          return "FO";
        case SystemLanguage.Finnish:
          #if UNITY_ANDROID
            if (CalcAndroidISO6392() == "FIL")
              return "TL"; // tagalog
          #endif
          return "FI";
        case SystemLanguage.French:
          return "FR";
        case SystemLanguage.German:
          return "DE";
        case SystemLanguage.Greek:
          return "EL";
        case SystemLanguage.Hebrew:
          return "HE";
        case SystemLanguage.Hungarian:
          return "HU";
        case SystemLanguage.Icelandic:
          return "IS";
        case SystemLanguage.Indonesian:
          return "ID";
        case SystemLanguage.Italian:
          return "IT";
        case SystemLanguage.Japanese:
          return "JA";
        case SystemLanguage.Korean:
          return "KO";
        case SystemLanguage.Latvian:
          return "LV";
        case SystemLanguage.Lithuanian:
          return "LT";
        case SystemLanguage.Norwegian:
          return "NB";
        case SystemLanguage.Polish:
          return "PL";
        case SystemLanguage.Portuguese:
          return "PT"; // TODO differentiate between PT-BR?
        case SystemLanguage.Romanian:
          return "RO";
        case SystemLanguage.Russian:
          return "RU";
        case SystemLanguage.SerboCroatian:
          return "SH";
        case SystemLanguage.Slovak:
          return "SK";
        case SystemLanguage.Slovenian:
          return "SL";
        case SystemLanguage.Spanish:
          return "ES";
        case SystemLanguage.Swedish:
          return "SV";
        case SystemLanguage.Thai:
          return "TH";
        case SystemLanguage.Turkish:
          return "TR";
        case SystemLanguage.Ukrainian:
          return "UK";
        case SystemLanguage.Vietnamese:
          return "VI";
        case SystemLanguage.ChineseSimplified:
          return "ZH-CN";
        case SystemLanguage.ChineseTraditional:
          return "ZH-TW";

        case SystemLanguage.Unknown:
        default:
          // caller can decide what a good default value is if empty.
          #if UNITY_ANDROID
            return CalcAndroidISO6391();
          #else // TODO other platforms
            return string.Empty;
          #endif
      }
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
        return ABIArch.ARM64;
      else if (type.StartsWith("ARMv7"))
        return ABIArch.ARM32;

      // Chrome OS (should be a rare case)
      if (System.Environment.Is64BitOperatingSystem)
        return ABIArch.x86_64;
      else
        return ABIArch.x86;
    }

    // TODO: CalcIsChromeOS() - https://docs.unity3d.com/ScriptReference/Android.AndroidDevice-hardwareType.html


#region Native Platform Bindings

  #if UNITY_ANDROID

    private static string CalcAndroidBrowser()
    {
      #if UNITY_EDITOR
      if (Application.isEditor)
        return string.Empty;
      #endif

      const string DUMMY_URL          = "https://example.com";
      const long   MATCH_DEFAULT_ONLY = 0x00010000; // https://developer.android.com/reference/android/content/pm/PackageManager#MATCH_DEFAULT_ONLY

      AndroidJavaObject uri    = null,
                        intent = null,
                        flags  = null,
                        resolv = null;
      try
      {
        uri    = AndroidBridge.MakeUri(DUMMY_URL);
        intent = AndroidBridge.MakeIntent("android.intent.action.VIEW", uri);
        flags  = AndroidBridge.Classes.ResolveInfoFlags.CallStatic<AndroidJavaObject>("of", MATCH_DEFAULT_ONLY);
        resolv = AndroidBridge.PackageManager.Call<AndroidJavaObject>("resolveActivity", intent, flags);
        return resolv.Call<AndroidJavaObject>("loadLabel", AndroidBridge.PackageManager).ToString();
      }
      catch (AndroidJavaException aje)
      {
        Orator.NFE(aje);
      }
      finally
      {
        uri?.Dispose();
        intent?.Dispose();
        flags?.Dispose();
        resolv?.Dispose();
      }

      return string.Empty; // no deferred calcing
    }

    private static string CalcAndroidISO6391() // 2-letter retval
    {
      #if UNITY_EDITOR
      if (Application.isEditor)
        return string.Empty;
      #endif

      try
      {
        return AndroidBridge.SystemLocale.Call<string>("getLanguage").ToUpperInvariant();
      }
      catch (AndroidJavaException aje)
      {
        Orator.NFE(aje);
      }

      return string.Empty;
    }

    private static string CalcAndroidISO6392() // 3-letter retval
    {
      #if UNITY_EDITOR
      if (Application.isEditor)
        return string.Empty;
      #endif

      try
      {
        return AndroidBridge.SystemLocale.Call<string>("getISO3Language").ToUpperInvariant();
      }
      catch (AndroidJavaException aje)
      {
        Orator.NFE(aje);
      }

      return string.Empty;
    }

    private static string CalcAndroidCarrier()
    {
      #if UNITY_EDITOR
      if (Application.isEditor)
        return string.Empty;
      #endif

      const string TELEPHONY_SERVICE = "phone"; // https://developer.android.com/reference/android/content/Context#TELEPHONY_SERVICE

      AndroidJavaObject teleMan = null;
      try
      {
        teleMan = AndroidBridge.Activity.Call<AndroidJavaObject>("getSystemService", TELEPHONY_SERVICE);
        if (teleMan != null)
        {
          return teleMan.Call<string>("getNetworkOperatorName") ?? string.Empty;
        }
      }
      catch (AndroidJavaException aje)
      {
        Orator.NFE(aje);
      }
      finally
      {
        teleMan?.Dispose();
      }

      return string.Empty;
    }

  #elif UNITY_IOS

  #elif UNITY_WEBGL

  #endif

#endregion (Native Platform Bindings)

#endregion Private section

  } // end class DeviceSpy

}
