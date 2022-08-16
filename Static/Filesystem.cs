/*! @file   Static/Filesystem.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2022-06-03
**/

using System.IO;

using UnityEngine;

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
    
    public static bool TryReadLines(string filepath, out string[] lines, char newline = '\n', Encoding encoding = null)
    {
      if (TryReadText(filepath, out string text, encoding))
      {
        // maybe this is slow?
        lines = text.Split(newline);
        return lines.Length > 0;
      }
      
      lines = new string[0];
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

    public static bool IsValidPath(string path)
    {
      return Paths.IsValidPath(path);
    }
    
    public static bool PathExists(string path)
    {
      return File.Exists(path) || Directory.Exists(path);
    }
    
    public static bool TryDeletePath(string path)
    {
      try
      {
        #if UNITY_EDITOR
        if (!PathExists(path))
          return true;
        
        return UnityEditor.FileUtil.DeleteFileOrDirectory(path);
        #else // if !UNITY_EDITOR
        
        if (File.Exists(path))
        {
          File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
          Directory.Delete(path, true);
        }
        
        return true;
        
        #endif // UNITY_EDITOR
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
