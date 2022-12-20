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

      public static AndroidJavaClass ResolveInfoFlags
        => s_ResolveInfoFlags ?? (s_ResolveInfoFlags = new AndroidJavaClass("android.content.pm.PackageManager$ResolveInfoFlags"));

      public static AndroidJavaClass Locale
        => s_Locale ?? (s_Locale = new AndroidJavaClass("java.util.Locale"));

    } // end nested static class Classes


    public static AndroidJavaObject Activity
      => s_Activity ?? (s_Activity = Classes.UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"));

    public static AndroidJavaObject PackageManager
      => s_PackageManager ?? (s_PackageManager = Activity.Call<AndroidJavaObject>("getPackageManager"));

    public static AndroidJavaObject SystemLocale
      => s_SystemLocal ?? (s_SystemLocal = Classes.Locale.CallStatic<AndroidJavaObject>("getDefault"));



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
    private static AndroidJavaClass s_Locale;

    private static AndroidJavaObject s_Activity;
    private static AndroidJavaObject s_PackageManager;
    private static AndroidJavaObject s_SystemLocal;

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
    }

  } // end static class AndroidBridge
}

#endif // UNITY_ANDROID