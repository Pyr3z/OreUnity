/** @file   Static/Filesystem.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-03
**/

using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Encoding = System.Text.Encoding;


namespace Bore
{

  public static class Filesystem
  {
    #region FUNDAMENTAL FILE I/O
    public static bool TryWriteText(string path, string text, Encoding encoding = null)
    {
      return false;
    }

    public static bool TryReadText(string path, out string text, Encoding encoding = null)
    {
      try
      {
        text = File.ReadAllText(path, encoding ?? Strings.DefaultEncoding);

        LastException = null;
        return true;
      }
      catch (System.Exception ex)
      {
        switch (ex)
        {
          case IOException ioex:
            // anticipated case
            break;
          default:
            // unanticipated case
            ex = new UnhandledException(ex);
            break;
        }

        LastException = ex;
        text = string.Empty;
        return false;
      }
    }


    public static bool TryWriteBinary(string path, byte[] data)
    {
      return false;
    }

    public static bool TryReadBinary(string path, out byte[] data)
    {
      data = null;
      return false;
    }
    #endregion FUNDAMENTAL FILE I/O




    #region INFO & DEBUGGING

    public enum IOResult
    {
      None = -1,
      Success,
      PathNotFound,
      PathNotValid,
      NotPermitted,
      DiskFull,
      UnknownFailure
    }

    public static IOResult GetLastIOResult()
    {
      if (s_ExceptionRingIdx == 0)
        return IOResult.None;

      switch (LastException)
      {
        case null:
          return IOResult.Success;

        case FileNotFoundException _ :
        case DirectoryNotFoundException _ :
        case DriveNotFoundException _ :
          return IOResult.PathNotFound;

        case IOException iox :
        {
          string msg = iox.Message.ToLowerInvariant();

          if (msg.Contains("disk full"))
            return IOResult.DiskFull;
          if (msg.Contains("permission"))
            return IOResult.NotPermitted;
          if (msg.Contains("invalid"))
            return IOResult.PathNotValid;

          return IOResult.UnknownFailure;
        }

        default:
          return IOResult.UnknownFailure;
      }
    }

    public static bool TryGetLastException(out System.Exception ex)
    {
      // funky for-loop is necessary to preserve actual order of buffer reads
      for (int i = 0; i < EXCEPTION_RING_SZ; ++i)
      {
        int idx = (s_ExceptionRingIdx - i) % EXCEPTION_RING_SZ;
        if (idx < 0)
          break;

        if (s_ExceptionRingBuf[idx] != null)
        {
          ex = s_ExceptionRingBuf[idx];
          return true;
        }
      }

      ex = null;
      return false; ;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogLastException()
    {
      if (TryGetLastException(out System.Exception ex))
      {
        Debug.LogException(ex);
      }
    }

    #endregion INFO & DEBUGGING


    #region PRIVATE

    private const int EXCEPTION_RING_SZ = 4;
    private static System.Exception[] s_ExceptionRingBuf = new System.Exception[EXCEPTION_RING_SZ];
    private static int                s_ExceptionRingIdx = 0;

    private static System.Exception LastException
    {
      get => s_ExceptionRingBuf[  s_ExceptionRingIdx % EXCEPTION_RING_SZ];
      set => s_ExceptionRingBuf[++s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

    #endregion PRIVATE

  } // end static class Filesystem

}


#if UNITY_EDITOR

// TODO move outta here, use proper test framework
namespace Bore.Tests
{

  [InitializeOnLoad]
  internal static class TestFilesystem
  {
    static TestFilesystem()
    {
      EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
      // set up test data

      (string path, string text)[] texts =
      {
        ("progress",  "topScore:314\ntopKills:13\ncurrentSkin:dragonMonkey\n"),
        ("flight",    "{\"FlightVersion\":6820,\"LastUpdatedHash\":\"257af26594d7f4d6a3ff89f789622f23\"}"),
        ("stats",     "dayOfPlaying:3\ngamesToday:2\ntotalGames:15"),
      };

      (string path, byte[] data)[] datas =
      {
        ("koobox.png",            UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/koobox-icon.png").EncodeToPNG()),
        ($"{texts[1].path}.bin",  texts[1].text.ToBase64().ToBytes()),
      };


      // run tests

      // text tests:
      foreach (var (path, text) in texts)
      {
        if (!Filesystem.TryWriteText(path, text) ||
            !Filesystem.TryReadText(path, out string read))
        {
          Filesystem.LogLastException();
          continue;
        }

        if (!string.Equals(text, read, System.StringComparison.Ordinal))
        {
          Debug.LogError($"TEST ERROR: text file input and output differ.\n\t IN:{text}\n\tOUT:{read}");
        }
      }

      // binary tests:
      foreach (var (path, data) in datas)
      {
        if (!Filesystem.TryWriteBinary(path, data) ||
            !Filesystem.TryReadBinary(path, out byte[] read))
        {
          Filesystem.LogLastException();
          continue;
        }

        int ilen = texts.Length, rlen = read.Length;
        if (ilen != rlen)
        {
          Debug.LogError($"TEST: binary file byte array lengths differ.\n\t INLEN: {ilen}\n\tOUTLEN: {rlen}");
          continue;
        }

        int diffs = 0;
        for (int i = 0; i < ilen; ++i)
        {
          byte lhs = data[i];
          byte rhs = read[i];

          if (lhs != rhs)
            ++diffs;
        }

        if (diffs > 0)
        {
          Debug.LogError($"TEST: binary input and output differ.\n\tNUM DIFFS: {diffs}");
        }
      }

      EditorApplication.delayCall -= Run;
    }

  } // end internal static class TestFilesystem

}

#endif // UNITY_EDITOR
