/*! @file       Static/Filesystem.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-06-03
**/

using JetBrains.Annotations;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

using UnityEngine;

using System.IO;
using System.Collections.Generic;

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

    public static bool TryWriteJson([NotNull] string filepath, [CanBeNull] object data, bool pretty, Encoding encoding = null)
    {
      throw new System.NotImplementedException("Levi had to leave early");
    }

    public static bool TryWriteJson([NotNull] string filepath, [CanBeNull] object data, Encoding encoding = null)
    {
      throw new System.NotImplementedException("Levi had to leave early");
    }


    public static bool TryReadJson<T>([NotNull] string filepath, out T obj, Encoding encoding = null)
      where T : new()
    {
      if (!File.Exists(filepath))
      {
        LastException = new FileNotFoundException("File does not exist!", filepath);
        obj = default;
        return false;
      }

      try
      {
        var stream = new StreamReader(filepath, encoding ?? s_DefaultEncoding);

        s_LastReadPath = filepath;

        if (JsonAuthority.TryDeserializeStream(stream, out obj))
        {
          LastException = null;
          return true;
        }

        LastException = new UnanticipatedException("Some other squelched exception case occurred.");
        return false;
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

      obj = default;
      return false;
    }

    public static bool TryReadJsonObject([NotNull] string filepath, out IDictionary<string,object> obj, Encoding encoding = null)
    {
      if (!File.Exists(filepath))
      {                
        LastException = new FileNotFoundException("File does not exist!", filepath);
        obj = null;
        return false;
      }

      try
      {
        var stream = new StreamReader(filepath, encoding ?? s_DefaultEncoding);

        s_LastReadPath = filepath;

        var boxed = JsonAuthority.DeserializeStream(stream);

        if (boxed is IDictionary<string,object> casted)
        {
          LastException = null;
          obj = casted;
          return true;
        }

        LastException = new UnanticipatedException("Some other squelched exception case occurred.");
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

      obj = null;
      return false;
    }

    public static bool TryReadJsonArray([NotNull] string filepath, out IList<object> arr, Encoding encoding = null)
    {
      if (!File.Exists(filepath))
      {                
        LastException = new FileNotFoundException("File does not exist!", filepath);
        arr = null;
        return false;
      }

      try
      {
        var stream = new StreamReader(filepath, encoding ?? s_DefaultEncoding);

        s_LastReadPath = filepath;

        var boxed = JsonAuthority.DeserializeStream(stream);

        if (boxed is IList<object> casted)
        {
          LastException = null;
          arr = casted;
          return true;
        }

        LastException = new UnanticipatedException("Some other squelched exception case occurred.");
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

      arr = null;
      return false;
    }


    #if NEWTONSOFT_JSON

    /// <param name="filepath">
    ///   The valid path to the file to be written.
    ///   The directories leading up to it do not need to exist.
    /// </param>
    /// <param name="data">
    ///   An object representing your data. This is very softly-typed.
    ///   Newtonsoft.Json.JContainers are valid, and so are generic IList or
    ///   IDictionary objects.
    /// </param>
    /// <param name="encoding">
    ///   Optionally override <see cref="DefaultEncoding"/> with another encoder
    ///   (this call only).
    /// </param>
    /// <param name="serializer">
    ///   Optionally override the default serializer (defined by <see cref="JsonAuthority"/>)
    ///   (this call only).
    /// </param>
    /// <returns>
    ///   TRUE iff the data was written to a file at the given path successfully.
    /// </returns>
    public static bool TryWriteJson([NotNull] string filepath, [CanBeNull] object data,
                                    JsonSerializer serializer, Encoding encoding = null)
    {
      try
      {
        MakePathTo(filepath);

        var stream = new StreamWriter(filepath, append: false, encoding ?? s_DefaultEncoding);
        NewtonsoftAuthority.SerializeTo(stream, data, serializer);

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

    public static bool TryReadJson<T>([NotNull] string filepath, out T token,
                                      JsonSerializer serializer, Encoding encoding = null)
      where T : class
    {
      if (!File.Exists(filepath))
      {
        LastException = new FileNotFoundException("File does not exist!", filepath);
        token = null;
        return false;
      }

      try
      {
        var stream = new StreamReader(filepath, encoding ?? s_DefaultEncoding);

        s_LastReadPath = filepath;

        if (NewtonsoftAuthority.TryDeserializeStream(stream, out token, serializer) && token != null)
        {
          LastException = null;
          return true;
        }

        LastException = new UnanticipatedException("Some other squelched exception case occurred.");
        return false;
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

      token = null;
      return false;
    }

    #endif // NEWTONSOFT_JSON


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
        #if NEWTONSOFT_JSON
        else if (obj is JToken jtok)
        {
          json = jtok.ToString(pretty ? Formatting.Indented : Formatting.None);
        }
        #endif
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
      #if NEWTONSOFT_JSON
      catch (JsonException jex)
      {
        LastException = jex;
      }
      #endif
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
        CurrentException = new UnanticipatedException(e);
      }

      return false;
    }

    public static bool TryReadObject<T>([NotNull] string filepath, out T obj, Encoding encoding = null)
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


    public static bool TryWriteText([NotNull] string filepath, [CanBeNull] string text, Encoding encoding = null)
    {
      return TryWriteBinary(filepath, text.ToBytes(encoding ?? s_DefaultEncoding));
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


  #region FILESYSTEM QUERIES

    public static string GetTempPath(string forFilename = null)
    {
      if (!forFilename.IsEmpty() && !Paths.IsValidFileName(forFilename))
      {
        const string FALLBACK = "check_ur_warnings.tmp";
        Orator.Warn(typeof(Filesystem), 
                     $"Provided filename \"{forFilename}\" contains invalid chars. Using \"{FALLBACK}\" until you fix this.");
        forFilename = FALLBACK;
      }

      return $"{Application.temporaryCachePath}/{forFilename}";
    }

    public static IEnumerable<FileInfo> GetFiles([NotNull] string path)
    {
      var dir = new DirectoryInfo(path);

      if (!dir.Exists)
      {
        var fileInfo = new FileInfo(path);
        if (fileInfo.Exists)
          return new FileInfo[] { fileInfo };

        return System.Array.Empty<FileInfo>();
      }

      return dir.EnumerateFiles();
    }

  #endregion FILESYSTEM QUERIES


  #region OPERATION INFO & DEBUGGING

    public static string LastReadPath => s_LastReadPath ?? string.Empty;

    public static string LastModifiedPath => s_LastModifiedPath ?? string.Empty;

    [System.Obsolete("LastWrittenPath is deprecated. Use LastModifiedPath instead.")]
    public static string LastWrittenPath => LastModifiedPath;


    public static bool TryGetLastModified([NotNull] out FileInfo file)
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

        case FauxException faux:
        {
          if (!((ex = faux.InnerException) is null))
            goto TOP; // not recursion.
          return IOResult.UnknownFailure;
        }

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

    public static bool TryGetLastException(out Exception ex, bool consume = false, bool skipNulls = true)
    {
      // funky for-loop is necessary to preserve actual order of buffer reads
      for (int i = 0, count = s_ExceptionRingIdx.AtMost(EXCEPTION_RING_SZ); i < count; ++i)
      {
        int idx = (s_ExceptionRingIdx - i) % EXCEPTION_RING_SZ;

        ex = s_ExceptionRingBuf[idx];

        if (ex is null)
        {
          if (skipNulls)
            continue;
          return false;
        }

        if (consume)
        {
          s_ExceptionRingBuf[idx] = ex.Silenced();
        }

        return true;
      }

      ex = null;
      return false; ;
    }

    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogLastException(bool consume = true, bool skipNulls = false)
    {
      if (TryGetLastException(out var ex, consume, skipNulls))
        Orator.NFE(ex);
    }

  #endregion OPERATION INFO & DEBUGGING


  #region PRIVATE

    static Encoding s_DefaultEncoding = Encoding.UTF8;

    static string s_LastModifiedPath;
    static string s_LastReadPath;

    const int EXCEPTION_RING_SZ = 4;
    static readonly Exception[] s_ExceptionRingBuf = new Exception[EXCEPTION_RING_SZ];
    static int s_ExceptionRingIdx = 0;

    static Exception LastException
    {
      get => s_ExceptionRingBuf[s_ExceptionRingIdx % EXCEPTION_RING_SZ];
      set => s_ExceptionRingBuf[++s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

    static Exception CurrentException
    {
      set => s_ExceptionRingBuf[s_ExceptionRingIdx % EXCEPTION_RING_SZ] = value;
    }

  #endregion PRIVATE

  } // end static class Filesystem

}
