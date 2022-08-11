/*! @file   Static/Filesystem.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2022-06-03
**/

using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // TODO will remove when tests are moved out
#endif

using Encoding = System.Text.Encoding;


namespace Ore
{

  public static class Filesystem
  {

    #region FUNDAMENTAL FILE I/O

    public static bool TryWriteObject(string filepath, object obj)
    {
      #if DEBUG // default value for "pretty print JSON" relies on debug build status
      return TryWriteObject(filepath, obj, pretty: true);
      #else
      return TryWriteObject(filepath, obj, pretty: false);
      #endif
    }

    public static bool TryWriteObject(string filepath, object obj, bool pretty)
    {
      if (obj == null)
      {
        LastException = new System.ArgumentNullException("obj");
        return false;
      }

      try
      {
        MakePathTo(filepath);

        string json = JsonUtility.ToJson(obj, pretty);

        if (json.IsEmpty() || json[0] != '{')
        {
          LastException = new UnanticipatedException("JsonUtility.ToJson returned a bad JSON string.");
          return false;
        }

        File.WriteAllBytes(filepath, json.ToBytes(Encoding.Unicode));

        LastException = null;
        return true;
      }
      catch (System.Exception ex)
      {
        if (ex is IOException)
        {
          LastException = ex;
        }
        else
        {
          LastException = new UnanticipatedException(ex);
        }

        return false;
      }
    }

    public static bool TryWriteText(string filepath, string text, Encoding encoding = null)
    {
      return TryWriteBinary(filepath, text.ToBytes(encoding));
    }

    public static bool TryReadText(string filepath, out string text, Encoding encoding = null)
    {
      if (TryReadBinary(filepath, out byte[] data))
      {
        text = Strings.FromBytes(data, encoding);
        return true;
      }

      text = null;
      return false;
    }

    public static bool TryWriteBinary(string filepath, byte[] data)
    {
      try
      {
        MakePathTo(filepath);

        File.WriteAllBytes(filepath, data);

        LastException = null;
        return true;
      }
      catch (System.Exception ex)
      {
        if (ex is IOException)
        {
          LastException = ex;
        }
        else
        {
          LastException = new UnanticipatedException(ex);
        }

        return false;
      }
    }

    public static bool TryReadBinary(string filepath, out byte[] data)
    {
      try
      {
        data = File.ReadAllBytes(filepath);
        LastException = null;
        return true;
      }
      catch (System.Exception ex)
      {
        if (ex is IOException)
        {
          LastException = ex;
        }
        else
        {
          LastException = new UnanticipatedException(ex);
        }

        data = null;
        return false;
      }
    }

    public static bool TryMakePathTo(string filepath)
    {
      try
      {
        MakePathTo(filepath);
        LastException = null;
        return true;
      }
      catch (System.Exception ex)
      {
        if (ex is IOException)
        {
          LastException = ex;
        }
        else
        {
          LastException = new UnanticipatedException(ex);
        }

        return false;
      }
    }

    public static void MakePathTo(string filepath)
    {
      if (Paths.IsValidPath(filepath) && Paths.ExtractDirectoryPath(filepath, out string dirpath))
      {
        if (!Directory.Exists(dirpath) && !Directory.CreateDirectory(dirpath).Exists)
          throw new IOException($"Could not create directory \"{dirpath}\".");
        // else fallthrough
      }
      else
      {
        throw new System.ArgumentException($"Invalid path string \"{filepath}\".", "filepath");
      }
    }

    public static bool PathExists(string path)
    {
      return File.Exists(path) || Directory.Exists(path);
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

        case FileNotFoundException _:
        case DirectoryNotFoundException _:
        case DriveNotFoundException _:
          return IOResult.PathNotFound;

        case IOException iox:
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
      if (TryGetLastException(out var ex))
        Debug.LogException(ex);
    }

    #endregion INFO & DEBUGGING


    #region PRIVATE

    private const int EXCEPTION_RING_SZ = 4;
    private static System.Exception[] s_ExceptionRingBuf = new System.Exception[EXCEPTION_RING_SZ];
    private static int s_ExceptionRingIdx = 0;

    private static System.Exception LastException
    {
      get => s_ExceptionRingBuf[s_ExceptionRingIdx % EXCEPTION_RING_SZ];
      set => s_ExceptionRingBuf[++s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

    #endregion PRIVATE

  } // end static class Filesystem

}


#if UNITY_EDITOR

// TODO move outta here, use proper test framework
namespace Ore.Tests
{
  internal static class TestFilesystem
  {

    [MenuItem("Ore/Tests/Filesystem")]
    private static void Run()
    {
      Debug.Log("--- TEST BEGIN");

      // set up test data

      (string path, string text)[] texts =
      {
        ("progress",  "topScore:314\ntopKills:13\ncurrentSkin:dragonMonkey\n"),
        ("flight",    "{\"FlightVersion\":6820,\"LastUpdatedHash\":\"257af26594d7f4d6a3ff89f789622f23\"}"),
        ("stats",     "dayOfPlaying:3\ngamesToday:2\ntotalGames:15"),
      };

      (string path, byte[] data)[] datas =
      {
        ("koobox.png",            AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/bore~.png").EncodeToPNG()),
        ($"{texts[1].path}.bin",  texts[1].text.ToBase64().ToBytes()),
      };

      string pre = "./Temp/TestFilesystem";

      // run tests

      // text tests:
      foreach (var (path, text) in texts)
      {
        string fullpath = $"{pre}/{path}";

        if (!Filesystem.TryWriteText(fullpath, text) ||
            !Filesystem.TryReadText(fullpath, out string read))
        {
          Filesystem.LogLastException();
          continue;
        }

        if (!string.Equals(text, read, System.StringComparison.Ordinal))
          Debug.LogError($"TEST ERROR: text file input and output differ (\"{path}\").\n\t IN:{text}\n\tOUT:{read}");
      }

      // binary tests:
      foreach (var (path, data) in datas)
      {
        string fullpath = $"{pre}/{path}";

        if (!Filesystem.TryWriteBinary(fullpath, data) ||
            !Filesystem.TryReadBinary(fullpath, out byte[] read))
        {
          Filesystem.LogLastException();
          continue;
        }

        int ilen = data.Length, olen = read.Length;
        if (ilen != olen)
        {
          Debug.LogError($"TEST: binary file byte array lengths differ. (\"{path}\")\n\t INLEN: {ilen}\n\tOUTLEN: {olen}");
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
          Debug.LogError($"TEST: binary input and output differ. (\"{path}\")\n\tNUM DIFFS: {diffs}");
      }

      Debug.Log("--- TEST END");
    }

  } // end internal static class TestFilesystem

}

#endif // UNITY_EDITOR
