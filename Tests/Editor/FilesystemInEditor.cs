/*! @file       Tests/Editor/FilesystemInEditor.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-08-16
**/

using NUnit.Framework;

using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

using Ore;

#if NEWTONSOFT_JSON
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif


internal static class FilesystemInEditor
{
  const string TMPDIR = "./Temp/" + nameof(FilesystemInEditor) + "/";

  static readonly (string path, string text)[] TEXTS =
  {
    ("progress", "topScore:314\ntopKills:13\ncurrentSkin:dragonMonkey\n"),
    ("flight",   "{\"FlightVersion\":6820,\"LastUpdatedHash\":\"257af26594d7f4d6a3ff89f789622f23\"}"),
    ("stats",    "dayOfPlaying:3\ngamesToday:2\ntotalGames:15"),
  };

  static readonly (string path, byte[] data)[] BYTES =
  {
    ("UnityLogo.png",        null), // init in Setup()
    ($"{TEXTS[1].path}.bin", TEXTS[1].text.ToBase64().ToBytes()),
  };

  [OneTimeSetUp]
  public static void Setup()
  {
    Debug.Log($"{nameof(FilesystemInEditor)}.{nameof(Setup)}()");

    var icon = EditorGUIUtility.IconContent("UnityLogo").image as Texture2D;
    Assert.NotNull(icon, "icon");

    var tmp = RenderTexture.GetTemporary(icon.width, icon.height);

    Graphics.Blit(icon, tmp);

    var pop = RenderTexture.active;
    RenderTexture.active = tmp;

    icon = new Texture2D(icon.width, icon.height);
    icon.ReadPixels(new Rect(0f, 0f, tmp.width, tmp.height), 0, 0);
    icon.Apply();

    BYTES[0].data = icon.EncodeToPNG();

    RenderTexture.active = pop;
    RenderTexture.ReleaseTemporary(tmp);

    Debug.Log($"{nameof(FilesystemInEditor)}.{nameof(Setup)}() done.");
  }


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
      // skip if bad setup
      if (data is null)
      {
        Debug.Log($"Skipping test item \"{path}\" - bad setup.");
        continue;
      }

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


  #if NEWTONSOFT_JSON

  [Test]
  public static void TryJson()
  {
    var intList = new int[] { 1, 2, 3 };
    var dict = new Dictionary<string,object>
    {
      ["fef"] = true,
      ["bub"] = "bub",
      ["123"] = intList
    };

    string path = TMPDIR + nameof(TryJson) + ".json";

    if (!Filesystem.TryWriteJson(path, dict, pretty: true))
    {
      Assert.True(Filesystem.TryGetLastException(out var ex));
      throw ex;
    }

    if (!Filesystem.TryReadJson(path, out JToken readToken))
    {
      Assert.True(Filesystem.TryGetLastException(out var ex));
      throw ex;
    }

    Assert.AreEqual(dict["fef"], readToken.Value<bool>("fef"),   "fef == fef");
    Assert.AreEqual(dict["bub"], readToken.Value<string>("bub"), "bub == bub");
    var readList = readToken["123"] as JArray;
    Assert.NotNull(readList, "readList");
    Assert.AreEqual(intList[0], readList.Value<int>(0), "123 == 123");
    Assert.AreEqual(intList[1], readList.Value<int>(1), "123 == 123");
    Assert.AreEqual(intList[2], readList.Value<int>(2), "123 == 123");
  }

  #endif // NEWTONSOFT_JSON

} // end static class FilesystemInEditor
