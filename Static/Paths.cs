/*! @file       Static/Paths.cs
 *  @author     levianperez\@gmail.com
 *  @author     levi\@leviperez.dev
 *  @date       2022-06-03
**/

using JetBrains.Annotations;

using Path = System.IO.Path;
using StringComparer = System.StringComparer;


namespace Ore
{
  /// <summary>
  /// Utilities for handling filesystem path strings.
  /// </summary>
  [PublicAPI]
  public static class Paths
  {
    public const int MaxLength = 260;

    public static readonly char[] DirectorySeparators =
    {
      DirectorySeparator,
      LameDirectorySeparator
    };

    public const char DirectorySeparator = '/';  // Path.AltDirectorySeparatorChar
    public const char LameDirectorySeparator = '\\'; // Path.DirectorySeparatorChar

    public static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
    public static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();

    public static readonly StringComparer Comparer = StringComparer.InvariantCulture;


    public static bool IsValidPath([CanBeNull] string path)
    {
      return !path.IsEmpty() && path.IndexOfAny(InvalidPathChars) < 0;
    }

    public static bool IsValidFileName([CanBeNull] string filename)
    {
      return ExtractBasePath(filename, out string name) &&
              name.IndexOfAny(InvalidFileNameChars) < 0;
    }


    public static bool AreEquivalent([NotNull] string path1, [NotNull] string path2)
    {
      if (path1 == path2)
        return true;

      return Path.GetFullPath(path1) == Path.GetFullPath(path2);
    }


    public static string DetectAssetPathAssumptions([CanBeNull] string path)
    {
      if (path.IsEmpty())
      {
        return path;
      }

      if (!ExtractExtension(path, out _ ))
      {
        path += ".asset";
      }

      if (path.StartsWith("Assets/") || path.StartsWith("Packages/"))
      {
        return path;
      }

      return "Assets/" + path;
    }


    public static bool ExtractExtension([CanBeNull] string filepath, [NotNull] out string extension, bool includeDot = true)
    {
      extension = string.Empty;

      if (filepath.IsEmpty())
        return false;

      int slash = -1, dot = -1;
      int i = filepath.Length;

      while (i --> 0)
      {
        char c = filepath[i];

        if (dot < 0 && c == '.')
        {
          dot = i;
        }
        else if (c == DirectorySeparator || c == LameDirectorySeparator)
        {
          slash = i;
          break;
        }
      }

      if (dot < 0 || dot < slash || dot == filepath.Length - 1)
        return false;

      if (!includeDot)
        ++ dot;

      extension = filepath.Substring(dot);

      return true;
    }


    public static bool ExtractBasePath([CanBeNull] string filepath, out string basepath)
    {
      basepath = filepath;
      if (filepath.IsEmpty())
        return false;

      int slash = 1 + filepath.LastIndexOfAny(DirectorySeparators);

      while (slash == filepath.Length)
      {
        filepath = filepath.Remove(slash - 1);
        slash = 1 + filepath.LastIndexOfAny(DirectorySeparators);
      }

      if (slash < 1)
        basepath = filepath;
      else
        basepath = filepath.Substring(slash);

      return basepath.Length > 0;
    }

    public static bool ExtractDirectoryPath([CanBeNull] string filepath, out string dirpath, bool trailing_slash = false)
    {
      dirpath = filepath;
      if (filepath.IsEmpty())
        return false;

      dirpath = filepath.TrimEnd(DirectorySeparators);

      int slash = dirpath.LastIndexOfAny(DirectorySeparators);
      if (slash > 0)
        dirpath = dirpath.Remove(slash);
      if (trailing_slash)
        dirpath += '/';

      return dirpath.Length > 0;
    }

    public static bool Decompose([CanBeNull] string filepath, out string dirpath, out string basepath, bool trailing_slash = true)
    {
      dirpath = basepath = filepath;
      if (filepath.IsEmpty())
        return false;

      dirpath = filepath.TrimEnd(DirectorySeparators);

      int slash = dirpath.LastIndexOfAny(DirectorySeparators);
      if (slash < 0)
        dirpath = ".";
      else
      {
        dirpath = dirpath.Remove(slash);
        basepath = dirpath.Substring(slash + 1);
      }

      if (trailing_slash)
        dirpath += '/';

      return basepath.Length > 0 && dirpath.Length > 0;
    }

  } // end static class Paths

}
