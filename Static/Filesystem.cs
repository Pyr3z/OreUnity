/*! @file       Static/Filesystem.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-03
**/

using JetBrains.Annotations;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

using System.IO;

using Exception             = System.Exception;
using UnauthorizedException = System.UnauthorizedAccessException;
using ArgumentException     = System.ArgumentException;
using SecurityException     = System.Security.SecurityException;

using DateTime = System.DateTime;
using Encoding = System.Text.Encoding;


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

    public static bool TryWriteObject([NotNull] string filepath, [CanBeNull] object obj,
                                      bool     pretty   = EditorBridge.IS_DEBUG,
                                      Encoding encoding = null)
    {
      try
      {
        MakePathTo(filepath);

        string json;
        if (obj is null)
        {
          json = "{}";
        }
        else if (obj is JToken jtok)
        {
          json = jtok.ToString(pretty ? Formatting.Indented : Formatting.None);
        }
        else
        {
          json = JsonUtility.ToJson(obj, pretty);

          if (json.IsEmpty() || json[0] != '{')
          {
            LastException = new UnanticipatedException("JsonUtility.ToJson returned a bad JSON string.");
            return false;
          }
        }

        File.WriteAllBytes(filepath, json.ToBytes(encoding ?? s_DefaultEncoding));

        s_LastModifiedPath = filepath;
        LastException = null;
        return true;
      }
      catch (JsonException jex)
      {
        LastException = jex;
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

    public static bool TryWriteJson([NotNull] string filepath, [NotNull] JContainer token, bool pretty,
                                    Encoding encoding = null, JsonSerializer serializer = null)
    {
      if (pretty != (JsonAuthority.SerializerSettings.Formatting == Formatting.Indented))
      {
        if (serializer is null)
        {
          serializer = JsonSerializer.CreateDefault();
        }

        serializer.Formatting = pretty ? Formatting.Indented : Formatting.None;
      }

      return TryWriteJson(filepath, token, encoding, serializer);
    }

    public static bool TryWriteJson([NotNull] string filepath, [CanBeNull] object obj,
                                    Encoding encoding = null, JsonSerializer serializer = null)
    {
      if (obj is null)
      {
        obj = JValue.CreateNull();
      }

      StreamWriter   stream = null;
      JsonTextWriter writer = null;
      try
      {
        MakePathTo(filepath);
        stream = new StreamWriter(filepath, append: false, encoding ?? s_DefaultEncoding);
        writer = new JsonTextWriter(stream);

        if (serializer is null)
        {
          serializer = JsonSerializer.CreateDefault();
        }

        serializer.Serialize(writer, obj);

        s_LastModifiedPath = filepath;
        LastException = null;
        return true;
      }
      catch (JsonException jex)
      {
        LastException = jex;
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
      finally
      {
        stream?.Close();
        writer?.Close();
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

        s_LastModifiedPath = filepath;
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
      catch (SecurityException sec)
      {
        LastException = sec;
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
        CurrentException = new UnanticipatedException(e);
      }

      return false;
    }

    public static bool TryUpdateJson([NotNull] string filepath, [NotNull] JContainer objectOrArray,
                                     Encoding encoding = null, JsonSerializer serializer = null)
    {
      if (!TryReadJson(filepath, out JToken token, encoding, serializer))
      {
        return false;
      }

      if (token.Type != objectOrArray.Type)
      {
        CurrentException = new ArgumentException(
          $"Cannot update object representation from JSON: Container type mismatch.\nprevious={objectOrArray.Type}, read={token.Type}",
          nameof(objectOrArray)
        );
        return false;
      }

      try
      {
        objectOrArray.Merge(token, JsonAuthority.MergeReplace);
        return true;
      }
      catch (JsonException jex)
      {
        CurrentException = jex;
      }
      catch (Exception e)
      {
        CurrentException = new UnanticipatedException(e);
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

    public static bool TryReadJson([NotNull] string filepath, [NotNull] out JToken token,
                                   Encoding encoding = null, JsonSerializer serializer = null)
    {
      token = JValue.CreateUndefined();

      if (!Paths.IsValidPath(filepath))
      {
        LastException = new ArgumentException($"filepath: \"{filepath}\"");
        return false;
      }

      StreamReader   stream = null;
      JsonTextReader reader = null;
      try
      {
        stream = new StreamReader(filepath, encoding ?? s_DefaultEncoding);
        reader = new JsonTextReader(stream);

        if (serializer is null)
        {
          serializer = JsonSerializer.CreateDefault();
        }
        
        var maybeNull = serializer.Deserialize<JToken>(reader);
        
        if (maybeNull != null)
        {
          token = maybeNull;
        }

        LastException = null;
        s_LastReadPath = filepath;
      }
      catch (JsonException jex)
      {
        LastException = jex;
      }
      catch (IOException iox)
      {
        LastException = iox;
      }
      catch (UnauthorizedException auth)
      {
        LastException = auth;
      }
      catch (SecurityException sec)
      {
        LastException = sec;
      }
      catch (Exception e)
      {
        LastException = new UnanticipatedException(e);
      }
      finally
      {
        stream?.Close();
        reader?.Close();
      }

      return token.Type != JTokenType.Undefined;
    }

    public static bool TryReadText([NotNull] string filepath, [NotNull] out string text, Encoding encoding = null)
    {
      if (TryReadBinary(filepath, out byte[] data))
      {
        try
        {
          text = Strings.FromBytes(data, encoding ?? s_DefaultEncoding);
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
        LastException = new ArgumentException($"filepath: \"{filepath}\"");
        data = System.Array.Empty<byte>();
        return false;
      }

      try
      {
        data = File.ReadAllBytes(filepath);
        LastException = null;
        s_LastReadPath = filepath;
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
      catch (SecurityException sec)
      {
        LastException = sec;
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
      catch (SecurityException sec)
      {
        LastException = sec;
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
        throw new ArgumentException($"Invalid path string \"{filepath}\".", nameof(filepath));
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
          if (!PathExists(path))
          {
          }
          else if (UnityEditor.FileUtil.DeleteFileOrDirectory(path))
          {
            s_LastModifiedPath = path;
          }
        #else
          if (File.Exists(path))
          {
            File.Delete(path);
            s_LastModifiedPath = path;
          }
          else if (Directory.Exists(path))
          {
            Directory.Delete(path, recursive: true);
            s_LastModifiedPath = path;
          }
        #endif

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
      catch (SecurityException sec)
      {
        LastException = sec;
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
        s_LastModifiedPath = filepath;
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
      catch (SecurityException sec)
      {
        LastException = sec;
      }
      catch (Exception ex)
      {
        LastException = new UnanticipatedException(ex);
      }

      return false;
    }

  #endregion FUNDAMENTAL FILE I/O


  #region INFO & DEBUGGING

    public static string LastReadPath => s_LastReadPath ?? string.Empty;

    public static string LastModifiedPath => s_LastModifiedPath ?? string.Empty;

    [System.Obsolete("LastWrittenPath is deprecated. Use LastModifiedPath instead.")]
    public static string LastWrittenPath => LastModifiedPath;


    public static bool TryGetLastModified(out FileInfo file)
    {
      file = new FileInfo(s_LastModifiedPath ?? string.Empty);
      return file.Exists;
    }

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
        case SecurityException _:
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

    private static Encoding s_DefaultEncoding = Encoding.UTF8;

    private static string s_LastModifiedPath;
    private static string s_LastReadPath;

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
