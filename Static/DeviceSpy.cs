/*! @file     Static/DeviceSpy.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-01-20
**/

using JetBrains.Annotations;

using UnityEngine;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

#if UNITY_IOS
using Device = UnityEngine.iOS.Device;
#endif

using TimeSpan = System.TimeSpan;

using RegionInfo = System.Globalization.RegionInfo;

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

  #region Public section

    // ReSharper disable ConvertToNullCoalescingCompoundAssignment

    #if UNITY_IOS
    public static string IDFV => s_IDFV ?? (s_IDFV = Device.vendorIdentifier);
    public static string IDFA => s_IDFA = Device.advertisingIdentifier; // TODO
    #elif UNITY_ANDROID
    public static string IDFV => s_IDFV ?? (s_IDFV = CalcAndroidIDFV());
    public static string IDFA => s_IDFA ?? (s_IDFA = CalcAndroidIDFA());
    #else
    public static string IDFV => SystemInfo.deviceUniqueIdentifier;
    public static string IDFA => SystemInfo.deviceUniqueIdentifier;
    #endif

    public static bool IsTrackingLimited
    {
      get
      {
        #if UNITY_ANDROID
          if (s_IDFA is null)
            s_IDFA = CalcAndroidIDFA(); // TODO somehow reset cached value so it can update?
          return s_IsAdTrackingLimited;
        #elif UNITY_IOS
          return s_IsAdTrackingLimited = Device.advertisingTrackingEnabled;
        #else
          return s_IsAdTrackingLimited = false;
        #endif
      }
    }

    public static SerialVersion OSVersion
    {
      get
      {
        // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
        if (s_OSVersion is null)
          s_OSVersion = SerialVersion.ExtractOSVersion(SystemInfo.operatingSystem);
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

    public static string TimezoneISOString => Strings.MakeISOTimezone(TimezoneOffset);

    public static string LanguageISOString => s_LangISO6391 ?? (s_LangISO6391 = CalcISO6391());

    public static string CountryISOString => s_CountryISO3166a2 ?? (s_CountryISO3166a2 = CalcISO3166a2());

    public static TimeSpan TimezoneOffset => (TimeSpan)(s_TimezoneOffset ?? (s_TimezoneOffset = CalcTimezoneOffset()));

    public static float DiagonalInches => (float)(s_DiagonalInches ?? (s_DiagonalInches = CalcScreenDiagonalInches()));

    public static float AspectRatio => (float)(s_AspectRatio ?? (s_AspectRatio = CalcAspectRatio()));

    public static bool IsTablet => (bool)(s_IsTablet ?? (s_IsTablet = CalcIsTablet()));

    public static bool IsBlueStacks => (bool)(s_IsBlueStacks ?? (s_IsBlueStacks = CalcIsBlueStacks()));

    public static bool Is64Bit => ABI == ABIArch.ARM64 || s_ABIArch == ABIArch.x86_64;

    public static int ScreenRefreshHz => (int)(s_ScreenRefreshHz ?? (s_ScreenRefreshHz = Screen.currentResolution.refreshRate.AtLeast(30)));

    public static ABIArch ABI => (ABIArch)(s_ABIArch ?? (s_ABIArch = CalcABIArch()));

    // TODO fetch LowRAMThreshold from platform
    // e.g. https://developer.android.com/reference/android/app/ActivityManager.MemoryInfo#threshold
    public static int LowRAMThreshold => (int)(SystemInfo.systemMemorySize * 0.9f).AtLeast(256);


    public static int CalcRAMUsageMB()
    {
      return (int)(CalcRAMUsageBytes() / BYTES_PER_MB);
    }

    public static int CalcRAMUsageMiB()
    {
      // it's important to know there's a difference between MB and MiB
      return (int)(CalcRAMUsageBytes() / BYTES_PER_MIB);
    }

    public static float CalcRAMUsagePercent()
    {
      // SystemInfo gives sizes in MB
      return ((float)CalcRAMUsageBytes() / BYTES_PER_MB / SystemInfo.systemMemorySize.AtLeast(1)).Clamp01();
    }


    public static string ToJSON(bool prettyPrint = EditorBridge.IS_DEBUG)
    {
      #if !NEWTONSOFT_JSON
        return "\"Newtonsoft.Json not available.\"";
      #else
        // TODO this is cool code -- perhaps it can be more generalized and made into a utility?
        var json = new JObject();

        foreach (var property in typeof(DeviceSpy).GetProperties(TypeMembers.STATIC))
        {
          try
          {
            var value = property.GetValue(null);
            if (property.PropertyType.IsPrimitive)
            {
              json[property.Name] = new JValue(value);
            }
            else if (value is TimeSpan span)
            {
              json[property.Name] = new JValue(span);
            }
            else
            {
              json[property.Name] = value?.ToString();
            }
          }
          catch (System.Exception e)
          {
            Orator.NFE(e);
            json[property.Name] = JValue.CreateNull();
          }
        }

        json["RAMUsageMB"]      = CalcRAMUsageMB();
        json["RAMUsagePercent"] = CalcRAMUsagePercent();

        return json.ToString(prettyPrint ? Formatting.Indented : Formatting.None);
      #endif // NEWTONSOFT_JSON
    }

    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Ore/Log/DeviceSpy (JSON)")]
    private static void Menu_LogJSON()
    {
      Orator.Log($"\"{nameof(DeviceSpy)}\": {ToJSON(prettyPrint: true)}");
    }
    #endif // UNITY_EDITOR

  #endregion Public section


  #region Private section

    private static string        s_IDFV;
    private static string        s_IDFA;
    private static bool          s_IsAdTrackingLimited;
    private static SerialVersion s_OSVersion;
    private static string        s_Brand;
    private static string        s_Model;
    private static string        s_Browser;
    private static string        s_Carrier;
    private static string        s_LangISO6391;
    private static string        s_CountryISO3166a2;
    private static TimeSpan?     s_TimezoneOffset;
    private static float?        s_DiagonalInches;
    private static float?        s_AspectRatio;
    private static bool?         s_IsTablet;
    private static bool?         s_IsBlueStacks;
    private static int?          s_ScreenRefreshHz;
    private static ABIArch?      s_ABIArch;

    private const long BYTES_PER_MIB = 1048576L; // = pow(2,20)
    private const long BYTES_PER_MB  = 1000000L;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long CalcRAMUsageBytes()
    {
      // NOTE: This is where Ore's internal definition for reported RAM resides

      #if UNITY_2020_1_OR_NEWER
        return UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
      #else
        return System.GC.GetTotalMemory(forceFullCollection: false);
      #endif
    }


    private static (string make, string model) CalcMakeModel()
    {
      #if UNITY_IOS
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

    private static string CalcISO6391()
    {
      string iso6391 = Strings.MakeISO6391(Application.systemLanguage);

      if (iso6391.IsEmpty())
      {
        #if UNITY_ANDROID
          return CalcAndroidISO6391();
        #else // TODO other platforms
          return "";
        #endif
      }

      // handle Tagalog ("TL") collison with 2-letter ISO code for Finnish
      if (iso6391 == "FI")
      {
        #if UNITY_ANDROID
          if (CalcAndroidISO6392() == "FIL") // FIL for Filipino, sometimes aggregated as Tagalog
            return "TL";
        #else // TODO other platforms
        #endif
      }

      return iso6391;
    }

    private static string CalcISO3166a2() // 2-letter region code
    {
      // TODO this is probably inaccurate or else slow to call on devices
      return RegionInfo.CurrentRegion.TwoLetterISORegionName;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan CalcTimezoneOffset()
    {
      // TODO there might be a better (100x faster) Java API to call for Android ~
      return System.TimeZoneInfo.Local.GetUtcOffset(System.DateTime.Now);
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

    private static string CalcAndroidIDFV()
    {
      var resolver = AndroidBridge.Activity.Call<AndroidJavaObject>("getContentResolver");
      var secure = new AndroidJavaClass("android.provider.Settings$Secure");

      string id = secure.CallStatic<string>("getString", resolver, "android_id");

      return id.IsEmpty() ? SystemInfo.deviceUniqueIdentifier : id;
    }

    private static string CalcAndroidIDFA()
    {
      var adidClient = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
      var adidInfo = adidClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidBridge.Activity);

      s_IsAdTrackingLimited = adidInfo.Call<bool>("isLimitAdTrackingEnabled");
      if (s_IsAdTrackingLimited == false)
        return SystemInfo.unsupportedIdentifier;

      string id = adidInfo.Call<string>("getId");

      return id.IsEmpty() ? SystemInfo.unsupportedIdentifier : id;
    }

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
