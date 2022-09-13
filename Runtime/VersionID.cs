/*! @file       Objects/VersionID.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2021-11-08
**/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;


namespace Ore
{

  [System.Serializable, PublicAPI]
  public sealed class VersionID :
    System.IComparable<VersionID>,
    System.IEquatable<VersionID>,
    ISerializationCallbackReceiver
  {
    public static bool FuzzyValidate(string str)
    {
      const float PCT_THRESHOLD = 0.5f;

      if (str.IsEmpty())
        return false;

      int extra = str.IndexOfAny(TAG_DELIMS);
      if (extra > 0)
        str = str.Remove(extra);

      float digs = Strings.CountDigits(str);

      return digs / str.Length > PCT_THRESHOLD;
    }

    public static string ExtractOSVersion(string from)
    {
      string[] vers = from.Split(Strings.WHITESPACES, System.StringSplitOptions.RemoveEmptyEntries);

      //#if UNITY_ANDROID
      foreach (var ver in vers)
      {
        if (ver.StartsWith("API-"))
          return ver.Substring(4);
      }
      //#endif

      // walk back, looking for something useful
      int i = vers.Length;
      while (i --> 0)
      {
        string ver = vers[i];
        if (FuzzyValidate(ver))
          return ver;
      }

      return "1"; // non-zero but shitty OS version number, too shitty to pass any checks
    }



    public bool IsValid => !m_Vers.IsEmpty() && (m_OrderedHash & HASH_MASK_RESERVED) == 0x00;

    public bool IsDeep => m_Vers is {} && m_Vers.Length > 3;

    public int Major => this[0];
    public int Minor => this[1];
    public int Patch => this[2];
    public int TagNanoHash => m_OrderedHash & (int)HASH_MASK_EXTRA;

    public int Length => m_Vers?.Length ?? 0;
    public int this[int i]
    {
      get
      {
        int len = Length;
        if (i == len)
          return TagNanoHash;
        if (len < i + 1)
          return 0;
        return m_Vers[i];
      }
    }


    internal const char SEPARATOR = '.';
    internal static readonly char[] TAG_DELIMS = { '-', '+', '/', 'f' };
    internal static readonly char[] TRIM_CHARS = { 'v', 'V', '(', ')', '-', '+' };

    private const int NYBBLE = 4; // bits in half a byte

    // internal: these masks document succinctly the byte layout of VersionID ordered hashes.
    internal const uint HASH_MASK_RESERVED = 0xF0000000;
    internal const uint HASH_MASK_MAJOR    = 0x0FF00000;
    internal const uint HASH_MASK_MINOR    = 0x000FF000;
    internal const uint HASH_MASK_PATCH    = 0x00000FF0;
    internal const uint HASH_MASK_EXTRA    = 0x0000000F;


    [SerializeField, Delayed]
    private string m_String;


    [System.NonSerialized]
    private int[] m_Vers = System.Array.Empty<int>();

    [System.NonSerialized]
    private int m_OrderedHash;


    public VersionID([CanBeNull] string ver)
    {
      Deserialize(ver);
    }


    [NotNull]
    public override string ToString()
    {
      return m_String ?? string.Empty;
    }

    public override int GetHashCode()
    {
      return m_OrderedHash;
    }

    public int CompareTo([CanBeNull] VersionID other)
    {
      if (other is null)
        return 1;

      if (IsDeep || other.IsDeep)
        return DeepCompareTo(other);

      if (other.m_OrderedHash < m_OrderedHash)
        return +1;
      if (m_OrderedHash < other.m_OrderedHash)
        return -1;

      return 0;
    }

    public int DeepCompareTo([NotNull] VersionID other)
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

      if (other is VersionID vstr)
        return Equals(vstr);

      return other.ToString() == m_String;
    }
    public bool Equals([CanBeNull] VersionID other)
    {
      if (other is null)
        return !IsValid;

      if (IsDeep || other.IsDeep)
        return DeepEquals(other);

      return other.m_OrderedHash == m_OrderedHash;
    }

    public bool DeepEquals([NotNull] VersionID other)
    {
      for (int i = 0, ilen = Length.AtLeast(other.Length); i < ilen; ++i)
      {
        if (this[i] != other[i])
          return false;
      }

      return true;
    }


    public static implicit operator string ([CanBeNull] VersionID vstr)
    {
      return vstr?.m_String ?? string.Empty;
    }

    public static implicit operator VersionID ([CanBeNull] string ver)
    {
      return new VersionID(ver);
    }

    public static bool operator < ([CanBeNull] VersionID lhs, [CanBeNull] VersionID rhs)
    {
      if (lhs is null)
        return rhs is {};
      return lhs.CompareTo(rhs) < 0;
    }

    public static bool operator > ([CanBeNull] VersionID lhs, [CanBeNull] VersionID rhs)
    {
      if (lhs is null)
        return false;
      return lhs.CompareTo(rhs) > 0;
    }

    public static bool operator == ([CanBeNull] VersionID lhs, [CanBeNull] VersionID rhs)
    {
      if (lhs is null)
        return rhs is null || !rhs.IsValid;
      return lhs.CompareTo(rhs) == 0;
    }

    public static bool operator != ([CanBeNull] VersionID lhs, [CanBeNull] VersionID rhs)
    {
      if (lhs is null)
        return rhs is { IsValid: true };
      return lhs.CompareTo(rhs) != 0;
    }


    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
      Deserialize(m_String);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
    }


    internal int SplitParts(/*out*/ List<(string str, int idx)> parts)
    {
      parts.Clear();

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

      var splits = m_String.Substring(start, end - start).Split(SEPARATOR);
      for (int i = 0, ilen = splits.Length; i < ilen; ++i)
      {
        if (Parsing.TryParseInt32(splits[i], out int val))
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

    internal void Deserialize(string str)
    {
      if (str.IsEmpty() || (str = str.Trim(TRIM_CHARS)).IsEmpty())
      {
        m_String = string.Empty;
        m_Vers = System.Array.Empty<int>();
        m_OrderedHash = 0;
        return;
      }

      int start = 0;
      while (str[start] < '0' || str[start] > '9')
      {
        if (++start == str.Length) // no numbers in this string
        {
          m_String = str;
          m_Vers = new []{ 1 };
          m_OrderedHash = CalcOrderedHash(-1); // calcs a nanohash
          return;
        }
      }

      m_String = str;

      int end = str.IndexOfAny(TAG_DELIMS, start);
      if (end < 0)
        end = str.Length;

      var splits = str.Substring(start, end - start).Split(SEPARATOR);
      int len = splits.Length;

      m_Vers = new int[len];

      for (int i = 0; i < len; ++i)
      {
        if (!Parsing.TryParseInt32(splits[i], out m_Vers[i]))
          m_Vers[i] = -1;
      }

      m_OrderedHash = CalcOrderedHash(end);
    }

    private int CalcOrderedHash(int end)
    {
      int bitpos = 8 * sizeof(uint) - NYBBLE; // reserve top nybble
      int len = m_Vers.Length;
      int hash;

      // use bottom 2 nybbles of major version as MSB
      bitpos -= 2 * NYBBLE; // 20
      hash = (m_Vers[0] & 0xFF) << bitpos;

      // 2 nybbles for minor version
      if (len > 1)
      {
        bitpos -= 2 * NYBBLE; // 12
        hash |= (m_Vers[1] & 0xFF) << bitpos;
      }

      // 2 nybbles for patch version
      if (len > 2)
      {
        bitpos -= 2 * NYBBLE; // 4
        hash |= (m_Vers[2] & 0xFF) << bitpos;
      }

      // one nybble for a nanohash of any tags
      // (mainly only matters: 0 vs non-0)
      if (end + 1 < m_String.Length)
      {
        int nano = m_String.Substring(end + 1).GetHashCode() & 0xFF;
        hash |= nano ^ 0xFF;
      }

      if (len > 3)
      {
        hash |= (hash + len) & 0xFF;
      }

      return hash;
    }

  } // end class VersionID

}
