/*! @file       Static/AndroidBridge.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-16
**/

#if UNITY_ANDROID

using UnityEngine;

using JetBrains.Annotations;


namespace Ore
{
  [PublicAPI]
  public static class AndroidBridge
  {
    [PublicAPI]
    public static class Classes
    {
      // ReSharper disable ConvertToNullCoalescingCompoundAssignment
      public static AndroidJavaClass UnityPlayer
        => s_UnityPlayer ?? (s_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"));

      public static AndroidJavaClass Uri
        => s_Uri ?? (s_Uri = new AndroidJavaClass("android.net.Uri"));

      public static AndroidJavaClass Intent
        => s_Intent ?? (s_Intent = new AndroidJavaClass("android.content.Intent"));

      public static AndroidJavaClass ResolveInfo
        => s_ResolveInfo ?? (s_ResolveInfo = new AndroidJavaClass("android.content.pm.ResolveInfo"));

      public static AndroidJavaClass ResolveInfoFlags // API 33 only
        => s_ResolveInfoFlags ?? (s_ResolveInfoFlags = new AndroidJavaClass("android.content.pm.PackageManager$ResolveInfoFlags"));

      public static AndroidJavaClass ApplicationInfoFlags // API 33 only
        => s_ApplicationInfoFlags ?? (s_ApplicationInfoFlags = new AndroidJavaClass("android.content.pm.PackageManager$ApplicationInfoFlags"));

      public static AndroidJavaClass Locale
        => s_Locale ?? (s_Locale = new AndroidJavaClass("java.util.Locale"));

    } // end nested static class Classes


    public static AndroidJavaObject Activity
      => s_Activity ?? (s_Activity = Classes.UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"));

    public static AndroidJavaObject PackageManager
      => s_PackageManager ?? (s_PackageManager = Activity.Call<AndroidJavaObject>("getPackageManager"));

    public static AndroidJavaObject ApplicationInfo
      => s_ApplicationInfo ?? (s_ApplicationInfo = PackageManager.Call<AndroidJavaObject>("getApplicationInfo", Application.identifier, 0));
      // how ironic. In order to know our target API, I need to know our target API...
      // ... the above might throw a warning if targeting API 33+ ... TODO

    public static AndroidJavaObject SystemLocale
      => s_SystemLocal ?? (s_SystemLocal = Classes.Locale.CallStatic<AndroidJavaObject>("getDefault"));


    public static int TargetAPI
    {
      get
      {
        if (s_TargetAPI is null)
        {
          try
          {
            s_TargetAPI = ApplicationInfo.Get<int>("targetSdkVersion");
          }
          catch (System.Exception e)
          {
            Orator.NFE(e);
            s_TargetAPI = 0;
          }

          if (s_TargetAPI == 0)
            s_TargetAPI = MIN_TARGET_API;
        }

        return (int)s_TargetAPI;
      }
    }


    public static void RunOnUIThread(AndroidJavaRunnable runnable)
    {
      Activity.Call("runOnUiThread", runnable);
    }


    public static AndroidJavaObject MakeUri([NotNull] string uri)
    {
      return Classes.Uri.CallStatic<AndroidJavaObject>("parse", uri);
    }

    public static AndroidJavaObject MakeIntent([NotNull] string intent, params object[] args)
    {
      if (args.IsEmpty())
      {
        return new AndroidJavaObject("android.content.Intent", intent);
      }

      var catArgs = new object[args.Length+1];
      catArgs[0] = intent;
      args.CopyTo(catArgs, 1);
      return new AndroidJavaObject("android.content.Intent", catArgs);
    }


    private static AndroidJavaClass s_UnityPlayer;
    private static AndroidJavaClass s_Uri;
    private static AndroidJavaClass s_Intent;
    private static AndroidJavaClass s_ResolveInfo;
    private static AndroidJavaClass s_ResolveInfoFlags;
    private static AndroidJavaClass s_ApplicationInfoFlags;
    private static AndroidJavaClass s_Locale;

    private static AndroidJavaObject s_Activity;
    private static AndroidJavaObject s_PackageManager;
    private static AndroidJavaObject s_ApplicationInfo;
    private static AndroidJavaObject s_SystemLocal;

    private static int? s_TargetAPI;
    private const int MIN_TARGET_API = 31;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void OnAppLaunch()
    {
      Application.lowMemory += Dispose;
    }

    internal static void Dispose()
    {
      if (s_UnityPlayer != null)
      {
        s_UnityPlayer.Dispose();
        s_UnityPlayer = null;
      }

      if (s_Uri != null)
      {
        s_Uri.Dispose();
        s_Uri = null;
      }

      if (s_Intent != null)
      {
        s_Intent.Dispose();
        s_Intent = null;
      }

      if (s_ResolveInfo != null)
      {
        s_ResolveInfo.Dispose();
        s_ResolveInfo = null;
      }

      if (s_ResolveInfoFlags != null)
      {
        s_ResolveInfoFlags.Dispose();
        s_ResolveInfoFlags = null;
      }

      if (s_ApplicationInfoFlags != null)
      {
        s_ApplicationInfoFlags.Dispose();
        s_ApplicationInfoFlags = null;
      }

      if (s_Locale != null)
      {
        s_Locale.Dispose();
        s_Locale = null;
      }


      if (s_Activity != null)
      {
        s_Activity.Dispose();
        s_Activity = null;
      }

      if (s_PackageManager != null)
      {
        s_PackageManager.Dispose();
        s_PackageManager = null;
      }

      if (s_ApplicationInfo != null)
      {
        s_ApplicationInfo.Dispose();
        s_ApplicationInfo = null;
      }

      if (s_SystemLocal != null)
      {
        s_SystemLocal.Dispose();
        s_SystemLocal = null;
      }
    }

  } // end static class AndroidBridge
}

#endif // UNITY_ANDROID