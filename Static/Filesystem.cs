/*! @file       Static/Filesystem.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-03
**/

using System.IO;
using JetBrains.Annotations;
using UnityEngine;

using Encoding = System.Text.Encoding;

using Exception = System.Exception;
using UnauthorizedException = System.UnauthorizedAccessException;
using ArgumentNullException = System.ArgumentNullException;
using ArgumentException = System.ArgumentException;


namespace Ore
{

  public static class Filesystem
  {

    #region FUNDAMENTAL FILE I/O

    [PublicAPI]
    public static bool TryWriteObject(string filepath, [NotNull] object obj)
    {
      #if DEBUG // default value for "pretty print JSON" relies on debug build status
      return TryWriteObject(filepath, obj, pretty: true);
      #else
      return TryWriteObject(filepath, obj, pretty: false);
      #endif
    }

    [PublicAPI]
    public static bool TryWriteObject(string filepath, [NotNull] object obj, bool pretty)
    {
      if (obj is null)
      {
        LastException = new ArgumentNullException("obj");
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

      lines = System.Array.Empty<string>();
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

    [PublicAPI]
    public static bool TryReadBinary([CanBeNull] string filepath, out byte[] data)
    {
      if (!Paths.IsValidPath(filepath))
      {
        
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

    [PublicAPI]
    public static bool TryMakePathTo([CanBeNull] string filepath)
    {
      if (filepath.IsEmpty())
      {
        LastException = new ArgumentNullException("filepath");
        return false;
      }

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

    [PublicAPI]
    public static void MakePathTo([NotNull] string filepath)
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

    [PublicAPI]
    public static bool PathExists([CanBeNull] string path)
    {
      return File.Exists(path) || Directory.Exists(path);
    }

    [PublicAPI]
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

    #endregion FUNDAMENTAL FILE I/O


    #region INFO & DEBUGGING

    [PublicAPI]
    public static IOResult GetLastIOResult()
    {
      if (s_ExceptionRingIdx == 0)
        return IOResult.None;

      return InterpretException(LastException);
    }

    [PublicAPI]
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
          if ((ex = unant.InnerException) is {})
            goto TOP; // fuck recursion.
          return IOResult.UnknownFailure;
        }

        default:
          return IOResult.UnknownFailure;
      }
    }

    [PublicAPI]
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

    [PublicAPI]
    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogLastException()
    {
      if (TryGetLastException(out var ex, consume: true))
        Orator.NFE(ex);
    }

    #endregion INFO & DEBUGGING


    #region PRIVATE

    private const int EXCEPTION_RING_SZ = 4;
    private static readonly Exception[] s_ExceptionRingBuf = new Exception[EXCEPTION_RING_SZ];
    private static int s_ExceptionRingIdx = 0;

    private static Exception LastException
    {
      get => s_ExceptionRingBuf[s_ExceptionRingIdx % EXCEPTION_RING_SZ];
      set => s_ExceptionRingBuf[++s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

    #endregion PRIVATE

  } // end static class Filesystem

}
