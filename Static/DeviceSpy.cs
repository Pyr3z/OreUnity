/*! @file       Static/DeviceSpy.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-01-20
**/

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

#if UNITY_IOS
using Device = UnityEngine.iOS.Device;
#endif

using DateTime   = System.DateTime;
using TimeSpan   = System.TimeSpan;
using Encoding   = System.Text.Encoding;
using RegionInfo = System.Globalization.RegionInfo;
using JsonObj    = System.Collections.Generic.Dictionary<string,object>;


namespace Ore
{
  [PublicAPI]
  public static class DeviceSpy
  {

  #region Public section

    // ReSharper disable ConvertToNullCoalescingCompoundAssignment
    // ReSharper disable ConvertIfStatementToNullCoalescingAssignment

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

    public static bool IsT1Graphics => Graphics.activeTier == GraphicsTier.Tier1;

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

    [NotNull]
    public static SerialVersion OSVersion => s_OSVersion ?? (s_OSVersion = CalcOSVersion());

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


    public static string ToJson()
    {
      // TODO this is cool code -- perhaps it can be more generalized and made into a utility?

      var json = new JsonObj();

      foreach (var property in typeof(DeviceSpy).GetProperties(TypeMembers.STATIC))
      {
        try
        {
          var value = property.GetValue(null);
          if (property.PropertyType.IsPrimitive)
          {
            json[property.Name] = value;
          }
          else if (value is TimeSpan span)
          {
            json[property.Name] = span.Ticks;
          }
          else
          {
            json[property.Name] = value?.ToString();
          }
        }
        catch (System.Exception e)
        {
          Orator.NFE(e);
          json[property.Name] = null;
        }
      }

      json["OSVersionStripped"] = OSVersion.ToString(stripExtras: true);

      #if !UNITY_EDITOR && UNITY_ANDROID
      json["TargetAPI"]       = AndroidBridge.TargetAPI;
      #endif

      json["RAMUsageMB"]      = CalcRAMUsageMB();
      json["RAMUsagePercent"] = CalcRAMUsagePercent();

      return JsonAuthority.Serialize(json);
    }


    #if UNITY_EDITOR

    [UnityEditor.MenuItem("Ore/Helpers/Write DeviceSpy to Json")]
    static void Menu_MakeJson()
    {
      string path = Filesystem.GetTempPath($"{nameof(DeviceSpy)}.json");
      string json = ToJson();

      if (Filesystem.TryWriteText(path, json))
      {
        Orator.Log(typeof(DeviceSpy), $"Wrote json to \"{path}\": {json}");
        UnityEditor.EditorUtility.RevealInFinder(path);
      }
      else
      {
        Filesystem.LogLastException();
        Orator.Warn(typeof(DeviceSpy), $"Couldn't write to \"{path}\", but here's the json anyway: {json}");
      }
    }

    #endif // UNITY_EDITOR


  #region Advanced API


    /// <summary>
    ///   Nested API to gift you some control over if/how/when the DeviceSpy
    ///   should utilize a proprietary geo-ip service, which takes an
    ///   internet-connected user's public IP address and gets an estimate as to
    ///   their device's current geo location.
    /// </summary>
    /// <remarks>
    ///   Current granularity: ISP's country only (2-char ISO 3166-a2 code).
    /// </remarks>
    [PublicAPI]
    public static class GeoIP
    {
      // TODO yeet PlayerPrefs

      /// <summary>
      ///   Gets the last cached value acquired by this system, or null if we
      ///   don't have one.
      /// </summary>
      /// <remarks>
      ///   Not guaranteed to be equivalent to the current value reported by
      ///   <see cref="DeviceSpy.CountryISOString"/>!
      /// </remarks>
      [CanBeNull]
      public static string CachedValue => PlayerPrefs.GetString(CACHED, null);

      /// <summary>
      ///   Get the timepoint (in UTC) when we last cached a geolocation value
      ///   received from our remote service.
      /// </summary>
      /// <returns>
      ///   <c>default(DateTime)</c> if we've never successfully cached a
      ///   geolocation provided by the remote service.
      /// </returns>
      /// <seealso cref="CachedValue"/>
      public static DateTime LastFetchedAt
      {
        get
        {
          if (s_LastFetchedAt is null)
          {
            s_LastFetchedAt = DateTimes.GetPlayerPref(FETCHED_AT);
          }

          return (DateTime)s_LastFetchedAt;
        }
      }

      /// <summary>
      ///   Get the <see cref="TimeInterval"/> since we last cached a
      ///   geolocation value received from our remote service, <b>rounded to
      ///   the nearest whole second.</b>
      /// </summary>
      /// <returns>
      ///   The equivalent of <see cref="DateTime.UtcNow"/> as a TimeInterval
      ///   if we've never successfully cached a geolocation.
      /// </returns>
      public static TimeInterval TimeSinceLastFetched => new TimeInterval(DateTime.UtcNow - LastFetchedAt)
                                                            .RoundToInterval(TimeInterval.Second);


      /// <summary>
      ///   <b>You should probably prefer this over <see cref="Fetch"/>.</b> <br/>
      ///   Attempts to asynchronously ping our remote geoip service.
      /// </summary>
      /// <param name="staleAfter">
      ///   The amount of time that must have passed since the last successful
      ///   <see cref="Fetch"/> before this call considers our cached
      ///   geolocation to be "stale" and attempt to refresh it with a new Fetch.
      /// </param>
      /// <inheritdoc cref="Fetch"/>
      [NotNull]
      public static Promise<string> FetchIfStale(TimeInterval staleAfter, int timeout = DEFAULT_TIMEOUT)
      {
        if (TimeSinceLastFetched < staleAfter && PlayerPrefs.HasKey(CACHED))
        {
          return Promise<string>.SucceedOnArrival(CountryISOString);
        }

        return Fetch(timeout);
      }

      /// <summary>
      ///   <b>You should probably prefer <see cref="FetchIfStale"/> over this
      ///   method</b>, unless of course you know the difference between the two
      ///   and have judged accordingly.
      /// </summary>
      /// <param name="timeout">
      ///   The timeout interval (in whole seconds) allotted to the request. If
      ///   it is exceeded, the request will halt and catch fire.
      /// </param>
      /// <returns>
      ///   A static <see cref="Promise{T}"/> (of T string) ready for you to
      ///   await and/or subscribe delegates to. <br/>
      ///   If Fetch has already been called and is still awaiting a server
      ///   response, the same promise object that was originally returned will
      ///   be returned again.
      /// </returns>
      [NotNull]
      public static Promise<string> Fetch(int timeout = DEFAULT_TIMEOUT)
      {
        // getCountryWithIP does not care about inputs, they're just used for hashing
        // TODO Should change this up in the future for 3rd party - Darren
        const string PARAMS    = "appName=&ipAddress=&timestamp=&hash=74be16979710d4c4e7c6647856088456"; // TODO encrypt (& scrub?)
        const string API       = "https://api.boreservers.com/borePlatform2/getCountryWithIP.php"; // TODO encrypt (& scrub?)
        const string MIME      = "application/x-www-form-urlencoded";
        const string ERRORMARK = "\"error\"";

        if (s_WWW != null)
        {
          return s_Promise;
        }

        var req = new UnityWebRequest(API)
        {
          method          = UnityWebRequest.kHttpVerbPOST,
          downloadHandler = new DownloadHandlerBuffer(),
          uploadHandler   = new UploadHandlerRaw(PARAMS.ToBytes(Encoding.UTF8))
        };

        req.SetRequestHeader("Content-Type", MIME);

        req.timeout = timeout;

        s_Promise.Reset();

        var promise = req.PromiseText(ERRORMARK, s_Promise);
                      // Note: extension calls req.Dispose() for us

        if (promise.IsCompleted && !promise.Succeeded)
        {
          #if DEBUG
          Orator.Log(typeof(GeoIP), "Fetch failed rather quickly - you sure about your internet connection?");
          #endif
          return promise;
        }

        s_WWW = req;

        promise.OnCompleted += () =>
        {
          s_WWW = null;
        };

        promise.OnSucceeded += response =>
        {
          // TODO implement non Json.NET solution? see #43

          #if NEWTONSOFT_JSON

            var jobj = Newtonsoft.Json.Linq.JObject.Parse(response, NewtonsoftAuthority.LoadStrict);

            string geoCode = jobj["result"]?["countryCode"]?.ToString();

            // ReSharper disable once PossibleNullReferenceException
            if (geoCode.IsEmpty() || !geoCode.Length.IsBetween(2, 6))
            {
              promise.Forget()
                     .FailWith(new UnanticipatedException($"{nameof(GeoIP)}.{nameof(Fetch)} -> \"{geoCode}\""));
              return;
            }

            LittleBirdie.CountryISOString = geoCode;
            // (LittleBirdie is used to propogate changes to listeners)

            PlayerPrefs.SetString(CACHED, geoCode);
            // meh... I *GUESS* since this is such rare code, triggering a save wouldn't kill us...
            PlayerPrefs.Save();
            // (but TODO just in case)

          #elif DEBUG

            Orator.Warn($"Newtonsoft JSON is not available; {nameof(GeoIP)}.{nameof(Fetch)} will pass up the raw server response.\n" +
                         response);

          #endif // NEWTONSOFT_JSON

          var now = DateTime.UtcNow;
          now.SetPlayerPref(FETCHED_AT);
          s_LastFetchedAt = now;
        };

        return promise;
      }

      /// <summary>
      ///   Resets everything cached and handled by <see cref="GeoIP"/>, making
      ///   its state as if the current user just had their first launch again. <br/>
      ///   In the off-chance that there is an ongoing <see cref="Fetch"/>
      ///   request still running, it will be aborted, the associated promise
      ///   will be expedited, and in DEBUG, a warning will get logged to let
      ///   you know that this unicorn edge case has occurred.
      /// </summary>
      /// <remarks>
      ///   Intended for debugging.
      /// </remarks>
      public static void Reset()
      {
        if (s_WWW != null)
        {
          s_WWW.Abort();
          s_WWW = null;

          if (!s_Promise.IsCompleted)
          {
            #if DEBUG
            Orator.Warn(typeof(GeoIP), "Called GeoIP.Reset() while a request is still pending; " +
                                            "undefined behaviour may result.");
            #endif

            s_Promise.Forget();
          }

          // just to double check that listeners aren't getting shafted:
          s_Promise.AwaitBlocking();

          s_Promise.Reset();
        }

        s_CountryISO3166a2 = null;
        s_LastFetchedAt    = default(DateTime);

        PlayerPrefs.DeleteKey(CACHED);
        PlayerPrefs.DeleteKey(FETCHED_AT);
        PlayerPrefs.Save();
      }



      static          UnityWebRequest s_WWW;
      static readonly Promise<string> s_Promise = new Promise<string>();
      static          DateTime?       s_LastFetchedAt;

      const int    DEFAULT_TIMEOUT = 30;
      const string FETCHED_AT      = "DeviceSpy.GeoIP.FetchedAt";
      const string CACHED          = "DeviceSpy.GeoIP.Cached";

    } // end nested static class GeoIP


    /// <summary>
    ///   Special API which allows you to globally override DeviceSpy's perception
    ///   of the current device.
    /// </summary>
    /// <remarks>
    ///   By design, nothing within this class is defensively validated (aside from data type, of course). <br/>
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
        set
        {
          if (!s_ABI.Equals(value))
          {
            s_ABI = value;
            OnCheepCheep?.Invoke(nameof(ABI), value);
          }
        }
      }

      public static float AspectRatio
      {
        get => DeviceSpy.AspectRatio;
        set
        {
          if (!s_AspectRatio.Equals(value))
          {
            s_AspectRatio = value;
            OnCheepCheep?.Invoke(nameof(AspectRatio), value);
          }
        }
      }

      public static string Brand
      {
        get => DeviceSpy.Brand;
        set
        {
          if (s_Brand != value)
          {
            s_Brand = value;
            OnCheepCheep?.Invoke(nameof(Brand), value);
          }
        }
      }

      public static string Browser
      {
        get => DeviceSpy.Browser;
        set
        {
          if (s_Browser != value)
          {
            s_Browser = value;
            OnCheepCheep?.Invoke(nameof(Browser), value);
          }
        }
      }

      public static string Carrier
      {
        get => DeviceSpy.Carrier;
        set
        {
          if (s_Carrier != value)
          {
            s_Carrier = value;
            OnCheepCheep?.Invoke(nameof(Carrier), value);
          }
        }
      }

      public static string CountryISOString
      {
        get => DeviceSpy.CountryISOString;
        set
        {
          if (s_CountryISO3166a2 != value)
          {
            s_CountryISO3166a2 = value;
            OnCheepCheep?.Invoke(nameof(CountryISOString), value);
          }
        }
      }

      public static string IDFA
      {
        get => DeviceSpy.IDFA;
        set
        {
          if (s_IDFA != value)
          {
            s_IDFA = value;
            OnCheepCheep?.Invoke(nameof(IDFA), value);
          }
        }
      }

      public static string IDFV
      {
        get => DeviceSpy.IDFV;
        set
        {
          if (s_IDFV != value)
          {
            s_IDFV = value;
            OnCheepCheep?.Invoke(nameof(IDFV), value);
          }
        }
      }

      public static bool IsBlueStacks
      {
        get => DeviceSpy.IsBlueStacks;
        set
        {
          if (!s_IsBlueStacks.Equals(value))
          {
            s_IsBlueStacks = value;
            OnCheepCheep?.Invoke(nameof(IsBlueStacks), value);
          }
        }
      }

      public static bool IsTablet
      {
        get => DeviceSpy.IsTablet;
        set
        {
          if (!s_IsTablet.Equals(value))
          {
            s_IsTablet = value;
            OnCheepCheep?.Invoke(nameof(IsTablet), value);
          }
        }
      }

      public static bool IsTrackingLimited
      {
        get => DeviceSpy.IsTrackingLimited;
        set
        {
          if (!s_IsAdTrackingLimited.Equals(value))
          {
            s_IsAdTrackingLimited = value;
            OnCheepCheep?.Invoke(nameof(IsTrackingLimited), value);
          }
        }
      }

      public static string LanguageISOString
      {
        get => DeviceSpy.LanguageISOString;
        set
        {
          if (s_LangISO6391 != value)
          {
            s_LangISO6391 = value;
            OnCheepCheep?.Invoke(nameof(LanguageISOString), value);
          }
        }
      }

      public static int LowRAMThreshold
      {
        get => DeviceSpy.LowRAMThreshold;
        set
        {
          if (!s_LowRAMThresh.Equals(value))
          {
            s_LowRAMThresh = value;
            OnCheepCheep?.Invoke(nameof(LowRAMThreshold), value);
          }
        }
      }

      public static string Model
      {
        get => DeviceSpy.Model;
        set
        {
          if (s_Model != value)
          {
            s_Model = value;
            OnCheepCheep?.Invoke(nameof(Model), value);
          }
        }
      }

      public static SerialVersion OSVersion
      {
        get => DeviceSpy.OSVersion;
        set
        {
          if (s_OSVersion != value)
          {
            s_OSVersion = value;
            OnCheepCheep?.Invoke(nameof(OSVersion), value);
          }
        }
      }

      public static int ScreenRefreshHz
      {
        get => DeviceSpy.ScreenRefreshHz;
        set
        {
          if (!s_ScreenRefreshHz.Equals(value))
          {
            s_ScreenRefreshHz = value;
            OnCheepCheep?.Invoke(nameof(ScreenRefreshHz), value);
          }
        }
      }

      public static TimeSpan TimezoneOffset
      {
        get => DeviceSpy.TimezoneOffset;
        set
        {
          if (!s_TimezoneOffset.Equals(value))
          {
            s_TimezoneOffset = value;
            OnCheepCheep?.Invoke(nameof(TimezoneOffset), value);
          }
        }
      }

      public static string UDID
      {
        get => DeviceSpy.UDID;
        set
        {
          if (s_UDID != value)
          {
            s_UDID = value;
            OnCheepCheep?.Invoke(nameof(UDID), value);
          }
        }
      }

      // ReSharper restore MemberHidesStaticFromOuterClass

      public delegate void PropertyAction([NotNull] string propertyName, [CanBeNull] object value);
      public static event PropertyAction OnCheepCheep;

    } // end nested class LittleBirdie

  #endregion Advanced API

  #endregion Public section



  #region Private section

    static ABI?          s_ABI;
    static float?        s_AspectRatio;
    static string        s_Brand;
    static string        s_Browser;
    static string        s_Carrier;
    static string        s_CountryISO3166a2;
    static float?        s_DiagonalInches;
    static string        s_IDFA;
    static string        s_IDFV;
    static bool          s_IsAdTrackingLimited;
    static bool?         s_IsBlueStacks;
    static bool?         s_IsTablet;
    static string        s_LangISO6391;
    static int?          s_LowRAMThresh;
    static string        s_Model;
    static SerialVersion s_OSVersion;
    static int?          s_ScreenRefreshHz;
    static TimeSpan?     s_TimezoneOffset;
    static string        s_UDID;

    const long BYTES_PER_MIB = 1048576L; // = pow(2,20)
    const long BYTES_PER_MB  = 1000000L;


    static (string make, string model) CalcMakeModel()
    {
      #if UNITY_IOS
        return ("Apple", SystemInfo.deviceModel);
      #else
        string makemodel = SystemInfo.deviceModel;

        // a la: https://docs.unity3d.com/ScriptReference/SystemInfo-deviceModel.html
        if (makemodel == SystemInfo.unsupportedIdentifier)
          return (string.Empty, string.Empty);

        int split = makemodel.IndexOfAny(new []{ ' ', '-' });
        if (split < 0)
          return (makemodel, makemodel);

        return (makemodel.Remove(split), makemodel.Substring(split + 1));
      #endif
    }

    [NotNull]
    static SerialVersion CalcOSVersion()
    {
      switch (SystemInfo.operatingSystemFamily)
      {
        case OperatingSystemFamily.MacOSX:
          return new SerialVersion(SystemInfo.operatingSystem);

        default:
          return SerialVersion.ExtractOSVersion(SystemInfo.operatingSystem);
      }
    }

    static string CalcVendorUDID()
    {
      const string PREFKEY_UDID = "VENDOR_UDID";

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

    static string CalcBrowserName()
    {
      #if UNITY_ANDROID
        return CalcAndroidBrowser().Trim();
      #elif UNITY_IOS
        return string.Empty; // TODO
      #elif UNITY_WEBGL
        return SystemInfo.deviceModel;
      #else
        return string.Empty;
      #endif
    }

    static string CalcCarrier()
    {
      #if UNITY_ANDROID
        return CalcAndroidCarrier().Trim();
      #elif UNITY_IOS
        return string.Empty; // TODO
      #elif UNITY_WEBGL
        return string.Empty;
      #else
        return string.Empty;
      #endif
    }

    static string CalcISO6391()
    {
      string iso6391 = Strings.MakeISO6391(Application.systemLanguage);

      if (iso6391.IsEmpty())
      {
        #if UNITY_ANDROID
          iso6391 = CalcAndroidISO6391();
        #else // TODO other platforms
          iso6391 = string.Empty;
        #endif
      }

      // handle Tagalog ("TL") collison with 2-letter ISO code for Finnish
      if (iso6391 == "FI")
      {
        #if UNITY_ANDROID
          if (CalcAndroidISO6392() == "FIL") // FIL for Filipino, sometimes aggregated as Tagalog
            return "TL";
        #else // TODO other platforms
          if (CountryISOString == "PH") // reasonable assumption
            return "TL";
        #endif
      }

      return iso6391;
    }

    static string CalcISO3166a2() // 2-letter region code
    {
      return GeoIP.CachedValue ?? RegionInfo.CurrentRegion.TwoLetterISORegionName;
        // RegionInfo.CurrentRegion is a temporary fallback,
        // and does not give accurate geo on most devices.
    }

    static TimeSpan CalcTimezoneOffset()
    {
      // TODO there might be a better (100x faster) Java API to call for Android ~
      return System.TimeZoneInfo.Local.BaseUtcOffset;
    }

    static string CalcIDFA()
    {
      #if UNITY_ANDROID
        return CalcAndroidIDFA();
      #elif UNITY_IOS
        return Device.advertisingIdentifier;
      #else
        return string.Empty; // TODO (?)
      #endif
    }

    static string CalcIDFV()
    {
      #if UNITY_ANDROID
        return CalcAndroidIDFV();
      #elif UNITY_IOS
        return CalcAppleIDFV();
      #else
        return UDID;
      #endif
    }

    static int CalcLowRAMThreshold()
    {
      // TODO fetch LowRAMThreshold from platform
      // e.g. Android: https://developer.android.com/reference/android/app/ActivityManager.MemoryInfo#threshold
      return (int)(SystemInfo.systemMemorySize * 0.1f).AtLeast(64);
    }

    static float CalcScreenDiagonalInches()
    {
      float w = Screen.width  / Screen.dpi;
      float h = Screen.height / Screen.dpi;
      return Mathf.Sqrt(w * w + h * h);
    }

    static float CalcAspectRatio()
    {
      if (Screen.height < Screen.width)
        return (float)Screen.width / Screen.height;
      else
        return (float)Screen.height / Screen.width;
    }

    static bool CalcIsTablet()
    {
      #if AD_MEDIATION_MAX
        return MaxSdkUtils.IsTablet();
      #else
        return Model.Contains("iPad") || CalcIsTabletByScreenSize();
      #endif
    }

    static bool CalcIsTabletByScreenSize()
    {
      const float MIN_DIAGONAL_INCHES = 6.5f;
      const float MAX_ASPECT_RATIO = 2.0f;
      return DiagonalInches > MIN_DIAGONAL_INCHES && AspectRatio < MAX_ASPECT_RATIO;
    }

    static bool CalcIsBlueStacks()
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

    static ABI CalcABIArch()
    {
      #if !UNITY_EDITOR && UNITY_IOS // TODO iOS needs to be tested
        if (System.Environment.Is64BitOperatingSystem)
          return ABI.ARM64;
        else
          return ABI.ARM;

      #else

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

      #endif // !UNITY_IOS
    }

    // TODO: CalcIsChromeOS() - https://docs.unity3d.com/ScriptReference/Android.AndroidDevice-hardwareType.html


  #region Native Platform Bindings

    #if UNITY_ANDROID

    static string CalcAndroidIDFV()
    {
      #if UNITY_EDITOR
        if (Application.isEditor) return UDID;
      #endif

      AndroidJavaObject resolv = null;
      AndroidJavaClass  secure = null;

      try
      {
        resolv = AndroidBridge.Activity.Call<AndroidJavaObject>("getContentResolver");
        secure = new AndroidJavaClass("android.provider.Settings$Secure");

        string id = secure.CallStatic<string>("getString", resolv, "android_id");
        return id ?? string.Empty;
      }
      catch (AndroidJavaException aje)
      {
        Orator.NFE(aje);
      }
      finally
      {
        resolv?.Dispose();
        secure?.Dispose();
      }

      return string.Empty;
    }

    static string CalcAndroidIDFA()
    {
      #if UNITY_EDITOR
        if (Application.isEditor) return string.Empty;
      #endif

      AndroidJavaClass adidClient = null;
      AndroidJavaObject adidInfo = null;
      try
      {
        adidClient = new AndroidJavaClass("com.google.android.gms.ads.identifier.AdvertisingIdClient");
        adidInfo = adidClient.CallStatic<AndroidJavaObject>("getAdvertisingIdInfo", AndroidBridge.Activity);

        s_IsAdTrackingLimited = adidInfo.Call<bool>("isLimitAdTrackingEnabled");
        if (s_IsAdTrackingLimited)
          return string.Empty;

        string id = adidInfo.Call<string>("getId");

        return id ?? string.Empty;
      }
      catch (AndroidJavaException aje)
      {
        Orator.NFE(aje);
      }
      finally
      {
        adidClient?.Dispose();
        adidInfo?.Dispose();
      }

      return string.Empty;
    }

    static string CalcAndroidBrowser()
    {
      #if UNITY_EDITOR
        if (Application.isEditor) return string.Empty;
      #endif

      const string DUMMY_URL          = "https://example.com";
      const long   MATCH_DEFAULT_ONLY = 0x00010000; // https://developer.android.com/reference/android/content/pm/PackageManager#MATCH_DEFAULT_ONLY

      bool api33 = OSVersion.Major >= 33;

      AndroidJavaObject uri    = null,
                        intent = null,
                        flags  = null,
                        resolv = null;
      try
      {
        uri    = AndroidBridge.MakeUri(DUMMY_URL);
        intent = AndroidBridge.MakeIntent("android.intent.action.VIEW", uri);

        // https://developer.android.com/reference/android/content/pm/PackageManager#resolveActivity(android.content.Intent,%20int)
        if (api33)
        {
          flags  = AndroidBridge.Classes.ResolveInfoFlags.CallStatic<AndroidJavaObject>("of", MATCH_DEFAULT_ONLY);
          resolv = AndroidBridge.PackageManager.Call<AndroidJavaObject>("resolveActivity", intent, flags);
        }
        else
        {
          resolv = AndroidBridge.PackageManager.Call<AndroidJavaObject>("resolveActivity", intent, (int)MATCH_DEFAULT_ONLY);
        }

        if (resolv is null)
          return string.Empty;

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

    static string CalcAndroidISO6391() // 2-letter retval
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

    static string CalcAndroidISO6392() // 3-letter retval
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

    static string CalcAndroidCarrier()
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

    static string CalcAppleIDFV()
    {
      // TODO is PlayerPrefs really necessary here?
      // I know on iOS native we use the user's keychain to persist IDFV across
      // fresh installs... I don't think PlayerPrefs is as robust.

      const string PREFKEY_IDFV = "VENDOR_IDFV";

      string idfv = PlayerPrefs.GetString(PREFKEY_IDFV);

      if (idfv.IsEmpty() || idfv == SystemInfo.unsupportedIdentifier)
      {
        idfv = Device.vendorIdentifier;

        if (idfv.IsEmpty())
          return UDID;

        PlayerPrefs.SetString(PREFKEY_IDFV, idfv);
      }

      return idfv;
    }

    // TODO

    #elif UNITY_WEBGL

    // TODO

    #endif

  #endregion (Native Platform Bindings)

  #endregion Private section

  } // end class DeviceSpy

}
