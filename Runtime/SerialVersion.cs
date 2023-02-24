/*! @file       Objects/SerialVersion.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2021-11-08
 *
 *  A faster, more forgiving, and Unity-serializable reimplementation of vanilla
 *  C#'s System.Version.
**/

using JetBrains.Annotations;

using UnityEngine;

using System.Collections.Generic;


namespace Ore
{

  [System.Serializable, PublicAPI]
  public sealed class SerialVersion :
    System.IComparable<SerialVersion>,
    System.IEquatable<SerialVersion>,
    ISerializationCallbackReceiver
  {

  #region Static section

    public static readonly SerialVersion None = new SerialVersion(0);


    public static SerialVersion ExtractOSVersion(string from)
    {
      // https://mvi.github.io/UnitySystemInfoTable/

      const int MAX_EXPECTED_OSVER = 20; // shmeh, kinda arbitrary

      string[] vers = from.Split(Strings.WHITESPACES, System.StringSplitOptions.RemoveEmptyEntries);

      // walk back, looking for something useful

      int i = vers.Length;
      while (i --> 0)
      {
        string ver = vers[i];

        if (ver.Length > MAX_EXPECTED_OSVER)
          continue;

        if (ver.StartsWith("("))
        {
          #if UNITY_WINDOWS || UNITY_EDITOR_WIN
            return new SerialVersion(ver);
          #else
            continue;
          #endif
        }

        #if UNITY_ANDROID || UNITY_EDITOR // might be called in editor concerning Android~
          if (ver.StartsWith("API-"))
          {
            if (i == vers.Length - 2) // include extra version info (should be in parens)
              return new SerialVersion($"{ver} {vers[i +1]}");
            else
              return new SerialVersion(ver);
          }
        #endif

        if (Strings.ContainsOnly(ver, BASIC_CHARSET))
          return new SerialVersion(ver);
      }

      return new SerialVersion(1);
        // non-zero but shitty OS version number, too shitty to pass any checks
    }

  #endregion Static section


    public int Major => this[0];
    public int Minor => this[1];
    public int Patch => this[2];

    public bool IsValid => !m_String.IsEmpty() && !m_Vers.IsEmpty();
    public bool IsNone  => m_String.IsEmpty() || m_String == "0";
    public bool HasTag  => m_TagIndex > 0;

    public string Tag => HasTag ? m_String.Substring(m_TagIndex) : string.Empty;

    public int Length => m_Vers?.Length - HasTag.ToInt() ?? 0;
      // if there is a tag, the final array element is its hash,
      // which isn't treated as an official component of the version

    public int this[int i] => m_Vers.IsEmpty() || !i.IsIndexTo(m_Vers) ? 0 : m_Vers[i];


    internal const string SEPARATOR = ".";

    internal static readonly char[] BASIC_CHARSET = { '.', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'v' };

    internal static readonly char[] TAG_DELIMS = { ' ', '-', '+', '/', 'a', 'b', 'f' };

    internal static readonly char[] TRIM_CHARS = { 'v', 'V', '(', ')', '-', '+' };


    [SerializeField, Delayed]
    private string m_String = string.Empty;

    [System.NonSerialized]
    private int[] m_Vers = System.Array.Empty<int>();

    [System.NonSerialized]
    private int m_TagIndex = -1;


    public SerialVersion([CanBeNull] string ver)
    {
      Deserialize(ver);
    }

    public SerialVersion(params int[] versionParts)
    {
      if (versionParts.IsEmpty())
        return;

      m_String = string.Join(".", versionParts);
      m_Vers   = versionParts;
    }

    public SerialVersion([CanBeNull] System.Version runtimeVersion)
    {
      SetFromSystemVersion(runtimeVersion);
    }


    [NotNull]
    public override string ToString()
    {
      return m_String ?? string.Empty;
    }

    [NotNull]
    public string ToString(bool stripExtras)
    {
      if (m_Vers.IsEmpty())
        return m_String ?? string.Empty;

      return stripExtras ? string.Join(SEPARATOR, m_Vers) : (m_String ?? string.Empty);
    }

    [NotNull]
    public string ToSemverString(bool stripExtras = false)
    {
      // https://semver.org

      if (stripExtras || !HasTag)
      {
        return $"{this[0]}.{this[1]}.{this[2]}";
      }

      return $"{this[0]}.{this[1]}.{this[2]}{m_String.Substring(m_TagIndex-1)}";
    }

    public override int GetHashCode()
    {
      int hihash = 0;

      hihash |= (this[0] & 0xFF) << 24; // major, minor, and patch constitute an
      hihash |= (this[1] & 0x0F) << 20; // ORDERED top 4 nybbles of the hash
      hihash |= (this[2] & 0x0F) << 16;

      // lower 4 nybbles = an unordered hash
      int lohash = Hashing.DefaultHashPrime << 16; // init with high bits
      for (int i = 3; i < (m_Vers?.Length ?? 0); ++i)
      {
        lohash = (int)Hashing.MixHashes(lohash, this[i]);
      }

      return hihash | (lohash & 0x0000FFFF);
    }

    public int CompareTo([CanBeNull] SerialVersion other)
    {
      return other is null ? +1 : DeepCompareTo(other);
    }

    public int DeepCompareTo([NotNull] SerialVersion other)
    {
      for (int i = 0, ilen = Length.AtLeast(other.Length); i < ilen; ++i)
      {
        int lhs = this[i];
        int rhs = other[i];
        if (lhs < rhs)
          return -1;
        if (rhs < lhs)
          return +1;
      }

      return 0;
    }

    public override bool Equals([CanBeNull] object other)
    {
      if (other is null)
        return !IsValid;

      if (other is SerialVersion vstr)
        return DeepEquals(vstr);

      return other.ToString() == m_String;
    }
    public bool Equals([CanBeNull] SerialVersion other)
    {
      if (other is null)
        return !IsValid;

      return DeepEquals(other);
    }



    public static implicit operator string ([CanBeNull] SerialVersion vstr)
    {
      return vstr?.m_String ?? string.Empty;
    }

    public static implicit operator SerialVersion ([CanBeNull] string ver)
    {
      return new SerialVersion(ver);
    }

    public static bool operator < ([CanBeNull] SerialVersion lhs, [CanBeNull] SerialVersion rhs)
    {
      if (lhs is null)
        return !(rhs is null);
      return lhs.CompareTo(rhs) < 0;
    }

    public static bool operator > ([CanBeNull] SerialVersion lhs, [CanBeNull] SerialVersion rhs)
    {
      if (lhs is null)
        return false;
      return lhs.CompareTo(rhs) > 0;
    }

    public static bool operator == ([CanBeNull] SerialVersion lhs, [CanBeNull] SerialVersion rhs)
    {
      if (lhs is null)
        return rhs is null || !rhs.IsValid;
      return lhs.CompareTo(rhs) == 0;
    }

    public static bool operator != ([CanBeNull] SerialVersion lhs, [CanBeNull] SerialVersion rhs)
    {
      if (lhs is null)
        return !(rhs is null) && rhs.IsValid;
      return lhs.CompareTo(rhs) != 0;
    }


    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      Deserialize(m_String);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }


    // ReSharper disable once CognitiveComplexity
    internal int SplitParts(/*out*/ List<(string str, int idx)> parts)
    {
      parts.Clear();

      if (m_String.IsEmpty())
      {
        return 0;
      }

      int start = 0;
      while (m_String[start] < '0' || m_String[start] > '9')
      {
        if (++start == m_String.Length)
        {
          parts.Add((m_String, -1 * start));
          return 1;
        }
      }

      if (start > 0)
      {
        parts.Add((m_String.Remove(start), -1));
      }

      int end = m_String.IndexOfAny(TAG_DELIMS, start);
      if (end < 0)
        end = m_String.Length;

      var splits = m_String.Substring(start, end - start).Split(SEPARATOR[0]);
      for (int i = 0, ilen = splits.Length; i < ilen; ++i)
      {
        if (Parsing.TryParseInt32(splits[i], out _ ))
        {
          parts.Add((splits[i], i));
        }
        else
        {
          parts.Add((splits[i], -1 * i));
        }
      }

      if (end < m_String.Length)
      {
        parts.Add((m_String.Substring(end), -1 * m_String.Length));
      }

      return parts.Count;
    }

    internal void SetFromSystemVersion(System.Version sysVer)
    {
      m_TagIndex = -1;

      if (sysVer is null || sysVer.GetHashCode() == 0)
      {
        m_String = string.Empty;
        m_Vers = System.Array.Empty<int>();
        return;
      }

      var parts = new List<int>
      {
        sysVer.Major,
        sysVer.Minor
      };

      if (sysVer.Build >= 0)
      {
        parts.Add(sysVer.Build);
      }

      if (sysVer.MajorRevision >= 0)
      {
        parts.Add(sysVer.MajorRevision);

        if (sysVer.MinorRevision >= 0)
        {
          parts.Add(sysVer.MinorRevision);
        }
      }
      else if (sysVer.Revision >= 0)
      {
        parts.Add(sysVer.Revision);
      }

      m_String = string.Join(SEPARATOR, parts);
      m_Vers   = parts.ToArray();
    }

    // ReSharper disable once CognitiveComplexity
    internal void Deserialize(string str)
    {
      if (str.IsEmpty() || (str = str.Trim(TRIM_CHARS)).IsEmpty())
      {
        m_String   = string.Empty;
        m_Vers     = System.Array.Empty<int>();
        m_TagIndex = -1;
        return;
      }

      int start = 0;
      while (str[start] < '0' || str[start] > '9')
      {
        if (++start == str.Length) // no numbers in this string
        {
          m_String   = str;
          m_Vers     = new []{ 1 };
          m_TagIndex = -1;
          return;
        }
      }

      m_String = str;

      int end = str.IndexOfAny(TAG_DELIMS, start);
        // note: at this point the first char can't be a tag delim
      if (end < 0 || end == str.Length-1)
      {
        m_TagIndex = -1;
        end = str.Length;
      }
      else
      {
        m_TagIndex = end + 1;
      }

      var splits = str.Substring(start, end - start).Split(SEPARATOR[0]);

      int len = splits.Length;
      m_Vers = new int[len + (m_TagIndex > 0).ToInt()];

      int i = 0;
      while (i < len)
      {
        if (!Parsing.TryParseInt32(splits[i], out m_Vers[i]))
          m_Vers[i] = -1;
        ++ i;
      }

      if (i == m_Vers.Length-1)
      {
        m_Vers[i] = str.Substring(m_TagIndex).GetHashCode();
      }
    }


    private bool DeepEquals([NotNull] SerialVersion other)
    {
      for (int i = 0, ilen = Length.AtLeast(other.Length); i < ilen; ++i)
      {
        if (this[i] != other[i])
          return false;
      }

      return true;
    }

  } // end class SerialVersion

}
