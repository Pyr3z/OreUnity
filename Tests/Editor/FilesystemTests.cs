/*! @file       Tests/Editor/FilesystemTests.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-16
**/

using NUnit.Framework;
using UnityEngine;
using UnityEditor;

using Ore;


public static class FilesystemTests
{
  [Test]
  public static void DirtyTest()
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
        Orator.Error($"TEST ERROR: text file input and output differ (\"{path}\").\n\t IN:{text}\n\tOUT:{read}");
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
        Orator.Error($"TEST: binary file byte array lengths differ. (\"{path}\")\n\t INLEN: {ilen}\n\tOUTLEN: {olen}");
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
        Orator.Error($"TEST: binary input and output differ. (\"{path}\")\n\tNUM DIFFS: {diffs}");
    }
  }

} // end static class FilesystemTests
