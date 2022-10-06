/*! @file       Tests/Editor/FilesystemCorrectness.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-16
**/

using NUnit.Framework;
using UnityEngine;
using UnityEditor;

using Ore;


internal static class FilesystemCorrectness
{
  const string TMPDIR = "./Temp/TestFilesystem/";

  static readonly (string path, string text)[] TEXTS =
  {
    ("progress", "topScore:314\ntopKills:13\ncurrentSkin:dragonMonkey\n"),
    ("flight",   "{\"FlightVersion\":6820,\"LastUpdatedHash\":\"257af26594d7f4d6a3ff89f789622f23\"}"),
    ("stats",    "dayOfPlaying:3\ngamesToday:2\ntotalGames:15"),
  };

  static readonly (string path, byte[] data)[] BYTES =
  {                                                                   // TODO this ties the test to KooBox
    ("koobox.png",           AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/bore~.png").EncodeToPNG()),
    ($"{TEXTS[1].path}.bin", TEXTS[1].text.ToBase64().ToBytes()),
  };


  [Test]
  public static void TextFiles()
  {
    Assert.DoesNotThrow(DoTextFiles, "Filesystem API is not intended to throw exceptions.");
  }

  private static void DoTextFiles()
  {
    foreach (var (path, text) in TEXTS)
    {
      string fullpath = TMPDIR + path;

      if (!Filesystem.TryWriteText(fullpath, text) ||
          !Filesystem.TryReadText(fullpath, out string read))
      {
        Assert.True(Filesystem.TryGetLastException(out var ex));
        throw ex;
      }

      Assert.AreEqual(text, read, $"text file contents mismatch. path=\"{path}\"");
    }
  }

  [Test]
  public static void BinaryFiles()
  {
    Assert.DoesNotThrow(DoBinaryFiles, "Filesystem API is not intended to throw exceptions.");
  }

  private static void DoBinaryFiles()
  {
    foreach (var (path, data) in BYTES)
    {
      string fullpath = TMPDIR + path;

      if (!Filesystem.TryWriteBinary(fullpath, data) ||
          !Filesystem.TryReadBinary(fullpath, out byte[] read))
      {
        Assert.True(Filesystem.TryGetLastException(out var ex));
        throw ex;
      }

      int ilen = data.Length, olen = read.Length;

      Assert.AreEqual(ilen, olen, $"binary file data lengths differ. path=\"{path}\"");

      int diffs = 0;
      for (int i = 0; i < ilen; ++i)
      {
        byte lhs = data[i];
        byte rhs = read[i];

        if (lhs != rhs)
          ++diffs;
      }

      Assert.Zero(diffs, $"binary input and output differ. path=\"{path}\"");
    }
  }

} // end static class FilesystemCorrectness
