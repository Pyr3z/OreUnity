﻿/** @file   Static/Paths.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-03
**/

using Path = System.IO.Path;
using StringComparer = System.StringComparer;

namespace Bore
{

  public static class Paths
  {
    public const int MaxLength = 260;

    public static readonly char[] DirectorySeparators =
    {
      DirectorySeparator,
      LameDirectorySeparator
    };

    public const char DirectorySeparator      = '/';  // Path.AltDirectorySeparatorChar
    public const char LameDirectorySeparator  = '\\'; // Path.DirectorySeparatorChar

    public static readonly char[] InvalidFileNameChars  = Path.GetInvalidFileNameChars();
    public static readonly char[] InvalidPathChars      = Path.GetInvalidPathChars();

    public static readonly StringComparer Comparer = StringComparer.InvariantCulture;


    public static bool IsValidPath(string path)
    {
      return !path.IsEmpty() && path.IndexOfAny(InvalidPathChars) < 0;
    }

    public static bool IsValidFileName(string filename)
    {
      return  ExtractBasePath(filename, out string name) &&
              name.IndexOfAny(InvalidFileNameChars) < 0;
    }


    public static bool ExtractBasePath(string filepath, out string basepath)
    {
      basepath = null;
      if (filepath.IsEmpty())
        return false;

      filepath = filepath.TrimEnd(DirectorySeparators);

      int slash = 1 + filepath.LastIndexOfAny(DirectorySeparators);
      if (slash < 1)
        basepath = filepath;
      else
        basepath = filepath.Substring(slash);

      return basepath.Length > 0;
    }

    public static bool ExtractDirectoryPath(string filepath, out string dirpath)
    {
      dirpath = null;
      if (filepath.IsEmpty())
        return false;

      filepath = filepath.TrimEnd(DirectorySeparators);

      int slash = 1 + filepath.LastIndexOfAny(DirectorySeparators);
      if (slash < 1)
        return false;

      dirpath = filepath.Remove(slash);
      return dirpath.Length > 0;
    }

    public static bool Decompose(string filepath, out string dirpath, out string basepath)
    {
      dirpath = basepath = null;
      if (filepath.IsEmpty())
        return false;

      filepath = filepath.TrimEnd(DirectorySeparators);

      int slash = 1 + filepath.LastIndexOfAny(DirectorySeparators);
      if (slash < 1)
      {
        dirpath   = "./";
        basepath  = filepath;
      }
      else
      {
        dirpath   = filepath.Remove(slash);
        basepath  = filepath.Substring(slash);
      }

      return basepath.Length > 0 && dirpath.Length > 0;
    }

  } // end static class Paths

}