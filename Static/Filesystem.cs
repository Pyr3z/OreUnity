/*! @file       Static/Filesystem.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-03
**/

using JetBrains.Annotations;
using UnityEngine;

using System.IO;

using Encoding              = System.Text.Encoding;
using Exception             = System.Exception;
using UnauthorizedException = System.UnauthorizedAccessException;
using ArgumentException     = System.ArgumentException;
using DateTime              = System.DateTime;


namespace Ore
{

  [PublicAPI]
  public static class Filesystem
  {

    public static Encoding DefaultEncoding
    {
      [NotNull]
      get => s_DefaultEncoding;
      set => s_DefaultEncoding = value ?? Encoding.UTF8;
    }


  #region FUNDAMENTAL FILE I/O

    public static bool TryWriteObject([NotNull] string filepath, [CanBeNull] object obj, Encoding encoding = null)
    {
      #if DEBUG // default value for "pretty print JSON" relies on debug build status
      return TryWriteObject(filepath, obj, pretty: true, encoding);
      #else
      return TryWriteObject(filepath, obj, pretty: false, encoding);
      #endif
    }

    public static bool TryWriteObject([NotNull] string filepath, [CanBeNull] object obj, bool pretty, Encoding encoding = null)
    {
      try
      {
        MakePathTo(filepath);

        string json;
        if (!(obj is null))
        {
          json = JsonUtility.ToJson(obj, pretty);

          if (json.IsEmpty() || json[0] != '{')
          {
            LastException = new UnanticipatedException("JsonUtility.ToJson returned a bad JSON string.");
            return false;
          }
        }
        else
        {
          json = "{}";
        }

        File.WriteAllBytes(filepath, json.ToBytes(encoding ?? s_DefaultEncoding));

        s_LastWrittenPath = filepath;
        LastException = null;
        return true;
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      return false;
    }

    public static bool TryWriteText([NotNull] string filepath, [CanBeNull] string text, Encoding encoding = null)
    {
      return TryWriteBinary(filepath, text.ToBytes(encoding ?? s_DefaultEncoding));
    }

    public static bool TryWriteBinary([NotNull] string filepath, [NotNull] byte[] data)
    {
      try
      {
        MakePathTo(filepath);

        File.WriteAllBytes(filepath, data);

        s_LastWrittenPath = filepath;
        LastException = null;
        return true;
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      return false;
    }

    public static bool TryUpdateObject([NotNull] string filepath, [NotNull] object obj, Encoding encoding = null)
    {
      if (!TryReadText(filepath, out string json, encoding))
      {
        return false;
      }

      try
      {
        JsonUtility.FromJsonOverwrite(json, obj);
      }
      catch (Exception e)
      {
        LastException = new UnanticipatedException(e);
      }

      return false;
    }

    public static bool TryReadObject<T>([NotNull] string filepath, [CanBeNull] out T obj, Encoding encoding = null)
    {
      if (!TryReadText(filepath, out string json, encoding))
      {
        obj = default;
        return false;
      }

      try
      {
        obj = JsonUtility.FromJson<T>(json);
        return obj != null;
      }
      catch (Exception e)
      {
        LastException = new UnanticipatedException(e);
      }

      obj = default;
      return false;
    }

    public static bool TryReadText([NotNull] string filepath, [NotNull] out string text, Encoding encoding = null)
    {
      if (TryReadBinary(filepath, out byte[] data))
      {
        try
        {
          text = Strings.FromBytes(data, encoding);
          return true;
        }
        catch (ArgumentException ex)
        {
          CurrentException = ex;
        }
      }

      text = string.Empty;
      return false;
    }

    public static bool TryReadLines([NotNull] string filepath, [NotNull] out string[] lines, char newline = '\n', Encoding encoding = null)
    {
      if (TryReadText(filepath, out string text, encoding))
      {
        // maybe this is slow?
        lines = text.Split(newline);
        return lines.Length > 0;
      }

      lines = System.Array.Empty<string>();
      return false;
    }

    public static bool TryReadBinary([NotNull] string filepath, [NotNull] out byte[] data)
    {
      if (!Paths.IsValidPath(filepath))
      {
        LastException = new ArgumentException("filepath");
        data = System.Array.Empty<byte>();
        return false;
      }

      try
      {
        data = File.ReadAllBytes(filepath);
        LastException = null;
        return true;
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      data = System.Array.Empty<byte>();
      return false;
    }


    public static bool TryMakePathTo([NotNull] string filepath)
    {
      try
      {
        MakePathTo(filepath);
        LastException = null;
        return true;
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      return false;
    }

    public static void MakePathTo([NotNull] string filepath) // throws
    {
      if (Paths.IsValidPath(filepath) && Paths.ExtractDirectoryPath(filepath, out string dirpath))
      {
        if (!Directory.Exists(dirpath) && !Directory.CreateDirectory(dirpath).Exists)
          throw new IOException($"Could not create directory \"{dirpath}\".");
        // else fallthrough
      }
      else
      {
        throw new ArgumentException($"Invalid path string \"{filepath}\".", "filepath");
      }
    }

    public static bool PathExists([CanBeNull] string path)
    {
      return File.Exists(path) || Directory.Exists(path);
    }

    public static bool TryDeletePath([CanBeNull] string path)
    {
      try
      {
        #if UNITY_EDITOR

        return !PathExists(path) || UnityEditor.FileUtil.DeleteFileOrDirectory(path);

        #else // if !UNITY_EDITOR

        if (File.Exists(path))
        {
          File.Delete(path);
        }
        else if (Directory.Exists(path))
        {
          Directory.Delete(path, recursive: true);
        }

        return true;

        #endif // UNITY_EDITOR
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      return false;
    }

    public static bool TryTouch([NotNull] string filepath)
    {
      try
      {
        MakePathTo(filepath);

        if (!File.Exists(filepath))
        {
          File.Create(filepath).Close();
        }
        else
        {
          File.SetLastWriteTimeUtc(filepath, DateTime.UtcNow);
        }

        LastException = null;
        return true;
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      return false;
    }

  #endregion FUNDAMENTAL FILE I/O


  #region INFO & DEBUGGING

    public static string LastWrittenPath => s_LastWrittenPath ?? string.Empty;

    public static IOResult GetLastIOResult()
    {
      if (s_ExceptionRingIdx == 0)
        return IOResult.None;

      return InterpretException(LastException);
    }

    public static IOResult InterpretException([CanBeNull] Exception ex)
    {
    TOP:
      switch (ex)
      {
        case null:
          return IOResult.Success;

        case ArgumentException _:
        case PathTooLongException _:
          return IOResult.PathNotValid;

        case FileNotFoundException _:
        case DirectoryNotFoundException _:
        case DriveNotFoundException _:
          return IOResult.PathNotFound;

        case IOException iox:
        {
          string msg = iox.Message.ToLowerInvariant();

          if (msg.StartsWith("disk full"))
            return IOResult.DiskFull;

          if (msg.StartsWith("sharing violation") || msg.StartsWith("win32 io returned 997."))
            return IOResult.FileAlreadyInUse;

          if (msg.StartsWith("invalid handle") || msg.Contains(" permi"))
            return IOResult.NotPermitted;

          return IOResult.UnknownFailure;
        }

        case UnauthorizedException _:
          return IOResult.NotPermitted;

        case UnanticipatedException unant:
        {
          if (!((ex = unant.InnerException) is null))
            goto TOP; // fuck recursion.
          return IOResult.UnknownFailure;
        }

        default:
          return IOResult.UnknownFailure;
      }
    }

    public static bool TryGetLastException(out Exception ex, bool consume = false)
    {
      // funky for-loop is necessary to preserve actual order of buffer reads
      for (int i = 0; i < EXCEPTION_RING_SZ; ++i)
      {
        int idx = (s_ExceptionRingIdx - i) % EXCEPTION_RING_SZ;
        if (idx < 0)
          break;

        if (s_ExceptionRingBuf[idx] is null)
          continue;

        ex = s_ExceptionRingBuf[idx];

        if (consume)
          s_ExceptionRingBuf[idx] = null;

        return true;
      }

      ex = null;
      return false; ;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogLastException()
    {
      if (TryGetLastException(out var ex, consume: true))
        Orator.NFE(ex);
    }

  #endregion INFO & DEBUGGING


  #region PRIVATE

    private static Encoding s_DefaultEncoding;

    private static string s_LastWrittenPath;

    private const int EXCEPTION_RING_SZ = 4;
    private static readonly Exception[] s_ExceptionRingBuf = new Exception[EXCEPTION_RING_SZ];
    private static int s_ExceptionRingIdx = 0;

    private static Exception LastException
    {
      get => s_ExceptionRingBuf[s_ExceptionRingIdx % EXCEPTION_RING_SZ];
      set => s_ExceptionRingBuf[++s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

    private static Exception CurrentException
    {
      set => s_ExceptionRingBuf[s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

  #endregion PRIVATE

  } // end static class Filesystem

}
