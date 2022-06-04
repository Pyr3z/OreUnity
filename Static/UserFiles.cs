/** @file   Static/UserFiles.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2022-06-03
**/

using UnityEngine;


namespace Bore
{

  public static class UserFiles
  {
    private static System.Exception s_LastException;

    public static bool TryWriteText(string path, string text)
    {
      return false;
    }

    public static bool TryWriteBinary(string path, byte[] data)
    {
      return false;
    }


    public static bool TryReadText(string path, out string text)
    {
      text = string.Empty;
      return false;
    }

    public static bool TryReadBinary(string path, out byte[] data)
    {
      data = null;
      return false;
    }


    [System.Diagnostics.Conditional("DEBUG")]
    public static void LogLastError()
    {
      if (s_LastException != null)
        Debug.LogException(s_LastException);
    }

  } // end static class UserFiles

}


// TODO move outta here, use proper test framework
namespace Bore.Tests
{

#if UNITY_EDITOR
  [UnityEditor.InitializeOnLoad]
  internal static class Tests
  {
    static Tests()
    {
      UnityEditor.EditorApplication.delayCall += Run;
    }

    private static void Run()
    {
      // set up test data

      (string path, string text)[] texts =
      {
        ("player.save", "topScore:314\ntopKills:13\ncurrentSkin:dragonMonkey\n"),
        ("flight",      "{\"FlightVersion\":6820,\"LastUpdatedHash\":\"257af26594d7f4d6a3ff89f789622f23\"}"),
        ("stats",       "dayOfPlaying:3\ngamesToday:2\ntotalGames:15"),
      };

      (string path, byte[] data)[] datas =
      {
        ("koobox.png", UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Textures/koobox-icon.png").EncodeToPNG()),
        ($"{texts[1].path}.bin", texts[1].text.ToBase64().ToBytes()),
      };


      // run tests

      // text tests:
      foreach (var (path, text) in texts)
      {
        if (!UserFiles.TryWriteText(path, text) ||
            !UserFiles.TryReadText(path, out string read))
        {
          UserFiles.LogLastError();
          continue;
        }

        if (!string.Equals(text, read, System.StringComparison.Ordinal))
        {
          Debug.LogError($"TEST ERROR: text file input and output differ.\n\t IN:{text}\n\tOUT:{read}");
        }
      }

      // binary tests:
      foreach (var (path, data) in datas)
      {
        if (!UserFiles.TryWriteBinary(path, data) ||
            !UserFiles.TryReadBinary(path, out byte[] read))
        {
          UserFiles.LogLastError();
          continue;
        }

        int ilen = texts.Length, rlen = read.Length;
        if (ilen != rlen)
        {
          Debug.LogError($"TEST: binary file byte array lengths differ.\n\t INLEN: {ilen}\n\tOUTLEN: {rlen}");
          continue;
        }

        int diffs = 0;
        for (int i = 0; i < ilen && i < rlen; ++i)
        {
          byte lhs = data[i];
          byte rhs = read[i];

          if (lhs != rhs)
            ++diffs;
        }

        if (diffs > 0)
        {
          Debug.LogError($"TEST: binary input and output differ.\n\t INLEN: {ilen}\n\tOUTLEN: {rlen}\n\t DIFFS: {diffs}");
        }
      }

      UnityEditor.EditorApplication.delayCall -= Run;
    }

  }
#endif // UNITY_EDITOR

}
