/*! @file       Static/DeviceSpy.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-20
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

  #region Public section

    // ReSharper disable ConvertToNullCoalescingCompoundAssignment

    public static ABI ABI => (ABI)(s_ABI ?? (s_ABI = CalcABIArch()));

    public static float AspectRatio => (float)(s_AspectRatio ?? (s_AspectRatio = CalcAspectRatio()));

    public static string Brand
    {
      get
      {
        if (s_Brand is null)
          (s_Brand, s_Model) = CalcMakeModel();
        return s_Brand;
      }
    }

    public static string Browser => s_Browser ?? (s_Browser = CalcBrowserName());

    public static string Carrier => s_Carrier ?? (s_Carrier = CalcCarrier());

    public static string CountryISOString => s_CountryISO3166a2 ?? (s_CountryISO3166a2 = CalcISO3166a2());

    public static float DiagonalInches => (float)(s_DiagonalInches ?? (s_DiagonalInches = CalcScreenDiagonalInches()));

    public static string IDFA => s_IDFA ?? (s_IDFA = CalcIDFA());

    public static string IDFV => s_IDFV ?? (s_IDFV = CalcIDFV());

    public static bool Is64Bit => ABI == ABI.ARM64 || s_ABI == ABI.x86_64;

    public static bool IsBlueStacks => (bool)(s_IsBlueStacks ?? (s_IsBlueStacks = CalcIsBlueStacks()));

    public static bool IsTablet => (bool)(s_IsTablet ?? (s_IsTablet = CalcIsTablet()));

    public static bool IsTrackingLimited
    {
      get
      {
        #if UNITY_EDITOR
          return false;
        #elif UNITY_ANDROID
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

    public static string LanguageISOString => s_LangISO6391 ?? (s_LangISO6391 = CalcISO6391());

    public static int LowRAMThreshold => (int)(s_LowRAMThresh ?? (s_LowRAMThresh = CalcLowRAMThreshold()));

    public static string Model
    {
      get
      {
        if (s_Model is null)
          (s_Brand, s_Model) = CalcMakeModel();
        return s_Model;
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

    public static int ScreenRefreshHz => (int)(s_ScreenRefreshHz ?? (s_ScreenRefreshHz = Screen.currentResolution.refreshRate.AtLeast(30)));

    public static string TimezoneISOString => Strings.MakeISOTimezone(TimezoneOffset);

    public static TimeSpan TimezoneOffset => (TimeSpan)(s_TimezoneOffset ?? (s_TimezoneOffset = CalcTimezoneOffset()));

    public static string UDID => s_UDID ?? (s_UDID = CalcVendorUDID());


    public static long CalcRAMUsageBytes()
    {
      // NOTE: This is where Ore's internal definition for reported RAM resides

      #if UNITY_2020_1_OR_NEWER
        return UnityEngine.Profiling.Profiler.GetTotalReservedMemoryLong();
      #else
        return System.GC.GetTotalMemory(forceFullCollection: false);
      #endif
    }

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
    private static void Menu_LogJSON() => Orator.Log($"\"{nameof(DeviceSpy)}\": {ToJSON(prettyPrint: true)}");

    #endif // UNITY_EDITOR


  #region Advanced API

    /// <summary>
    ///   Special API which allows you to globally override DeviceSpy's perception
    ///   of the current device.
    /// </summary>
    /// <remarks>
    ///   By design, nothing within this class is defensively validated. <br/>
    ///   <i>Little birdies can lie.</i> <br/>
    ///   <b>USE WITH CAUTION!</b>
    /// </remarks>
    [PublicAPI]
    public static class LittleBirdie
    {
      // ReSharper disable MemberHidesStaticFromOuterClass

      public static ABI ABI
      {
        get => DeviceSpy.ABI;
        set => s_ABI = value;
      }

      public static string Brand
      {
        get => DeviceSpy.Brand;
        set => s_Brand = value;
      }

      public static string Browser
      {
        get => DeviceSpy.Browser;
        set => s_Browser = value;
      }

      public static string Carrier
      {
        get => DeviceSpy.Carrier;
        set => s_Carrier = value;
      }

      public static string CountryISOString
      {
        get => DeviceSpy.CountryISOString;
        set => s_CountryISO3166a2 = value;
      }

      public static string IDFA
      {
        get => DeviceSpy.IDFA;
        set => s_IDFA = value;
      }

      public static string IDFV
      {
        get => DeviceSpy.IDFV;
        set => s_IDFV = value;
      }

      public static bool IsBlueStacks
      {
        get => DeviceSpy.IsBlueStacks;
        set => s_IsBlueStacks = value;
      }

      public static bool IsTablet
      {
        get => DeviceSpy.IsTablet;
        set => s_IsTablet = value;
      }

      public static bool IsTrackingLimited
      {
        get => DeviceSpy.IsTrackingLimited;
        set => s_IsAdTrackingLimited = value;
      }

      public static string LanguageISOString
      {
        get => DeviceSpy.LanguageISOString;
        set => s_LangISO6391 = value;
      }

      public static int LowRAMThreshold
      {
        get => DeviceSpy.LowRAMThreshold;
      }

      public static string UDID
      {
        get => DeviceSpy.UDID;
        set => s_UDID = value;
      }

      // ReSharper restore MemberHidesStaticFromOuterClass
    } // end nested class LittleBirdie

  #endregion Advanced API

  #endregion Public section



  #region Private section

    private static ABI?          s_ABI;
    private static float?        s_AspectRatio;
    private static string        s_Brand;
    private static string        s_Browser;
    private static string        s_Carrier;
    private static string        s_CountryISO3166a2;
    private static float?        s_DiagonalInches;
    private static string        s_IDFA;
    private static string        s_IDFV;
    private static bool          s_IsAdTrackingLimited;
    private static bool?         s_IsBlueStacks;
    private static bool?         s_IsTablet;
    private static string        s_LangISO6391;
    private static int?          s_LowRAMThresh;
    private static string        s_Model;
    private static SerialVersion s_OSVersion;
    private static int?          s_ScreenRefreshHz;
    private static TimeSpan?     s_TimezoneOffset;
    private static string        s_UDID;

    private const long BYTES_PER_MIB = 1048576L; // = pow(2,20)
    private const long BYTES_PER_MB  = 1000000L;

    private const string PREFKEY_UDID = "VENDOR_UDID";


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

    private static string CalcVendorUDID()
    {
      string udid = PlayerPrefs.GetString(PREFKEY_UDID); // don't tell Irontown
      if (!udid.IsEmpty())
        return udid;

      udid = SystemInfo.deviceUniqueIdentifier;

      if (udid.Equals(SystemInfo.unsupportedIdentifier))
      {
        udid = Strings.MakeGUID();
      }

      PlayerPrefs.SetString(PREFKEY_UDID, udid);
      PlayerPrefs.Save(); // meh, should let someone else save?

      return udid;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string CalcISO3166a2() // 2-letter region code
    {
      // TODO this is probably inaccurate or else slow to call on devices
      return RegionInfo.CurrentRegion.TwoLetterISORegionName;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static TimeSpan CalcTimezoneOffset()
    {
      // TODO there might be a better (100x faster) Java API to call for Android ~
      return System.TimeZoneInfo.Local.BaseUtcOffset;
    }

    private static string CalcIDFA()
    {
      #if UNITY_ANDROID
        return CalcAndroidIDFA();
      #elif UNITY_IOS
        return Device.advertisingIdentifier;
      #else
        return UDID;
      #endif
    }

    private static string CalcIDFV()
    {
      #if UNITY_ANDROID
        return CalcAndroidIDFV();
      #elif UNITY_IOS
        return Device.vendorIdentifier;
      #else
        return UDID;
      #endif
    }

    private static int CalcLowRAMThreshold()
    {
      // TODO fetch LowRAMThreshold from platform
      // e.g. Android: https://developer.android.com/reference/android/app/ActivityManager.MemoryInfo#threshold
      return (int)(SystemInfo.systemMemorySize * 0.1f).AtLeast(64);
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

    private static ABI CalcABIArch()
    {
      #if !UNITY_EDITOR && UNITY_IOS // TODO iOS needs to be tested
        if (System.Environment.Is64BitOperatingSystem)
          return ABI.ARM64;
        else
          return ABI.ARM;
      #endif // UNITY_IOS

      string type = SystemInfo.processorType;

      // Android and Android-like devices are pretty standard here
      if (type.StartsWith("ARM64"))
        return ABI.ARM64;
      else if (type.StartsWith("ARMv7"))
        return ABI.ARM32;

      // Chrome OS (should be a rare case)
      if (System.Environment.Is64BitOperatingSystem)
        return ABI.x86_64;
      else
        return ABI.x86;
    }

    // TODO: CalcIsChromeOS() - https://docs.unity3d.com/ScriptReference/Android.AndroidDevice-hardwareType.html


  #region Native Platform Bindings

    #if UNITY_ANDROID

    private static string CalcAndroidIDFV()
    {
      #if UNITY_EDITOR
        if (Application.isEditor) return UDID;
      #endif

      var resolver = AndroidBridge.Activity.Call<AndroidJavaObject>("getContentResolver");
      var secure = new AndroidJavaClass("android.provider.Settings$Secure");

      string id = secure.CallStatic<string>("getString", resolver, "android_id");

      return id ?? string.Empty;
    }

    private static string CalcAndroidIDFA()
    {
      #if UNITY_EDITOR
        if (Application.isEditor) return string.Empty;
      #endif

      var adidClient = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
      var adidInfo = adidClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidBridge.Activity);

      s_IsAdTrackingLimited = adidInfo.Call<bool>("isLimitAdTrackingEnabled");
      if (s_IsAdTrackingLimited == false)
        return SystemInfo.unsupportedIdentifier;

      string id = adidInfo.Call<string>("getId");

      return id ?? string.Empty;
    }

    private static string CalcAndroidBrowser()
    {
      #if UNITY_EDITOR
        if (Application.isEditor) return string.Empty;
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
        return $"{resolv.Call<AndroidJavaObject>("loadLabel", AndroidBridge.PackageManager)}";
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
        if (Application.isEditor) return string.Empty;
      #endif

      try
      {
        return AndroidBridge.SystemLocale.Call<string>("getLanguage")?.ToUpperInvariant() ?? string.Empty;
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
        if (Application.isEditor) return string.Empty;
      #endif

      try
      {
        return AndroidBridge.SystemLocale.Call<string>("getISO3Language")?.ToUpperInvariant() ?? string.Empty;
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
        if (Application.isEditor) return string.Empty;
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

    // TODO

    #elif UNITY_WEBGL

    // TODO

    #endif

  #endregion (Native Platform Bindings)

  #endregion Private section

  } // end class DeviceSpy

}
