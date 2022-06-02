/*  @file       Objects/VersionID.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2021-11-08
 */

using UnityEngine;


namespace Bore
{

  [System.Serializable]
  public sealed class VersionID :
    System.IComparable<VersionID>,
    System.IEquatable<VersionID>
//#if UNITY_EDITOR
//  , ISerializationCallbackReceiver
//#endif
  {
    public static bool FuzzyValidate(string str)
    {
      const float PCT_THRESHOLD = 0.5f;

      if (str.IsEmpty())
        return false;

      int extra = str.IndexOfAny(TAG_DELIMS);
      if (extra > 0)
        str = str.Remove(extra);

      float digs = Parsing.CountDigits(str);

      return (digs / str.Length) > PCT_THRESHOLD;
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
        {
          return ver;
        }
      }

      return "1"; // non-zero but shitty OS version number, too shitty to pass any checks
    }



    public bool IsValid         => m_OrderedHash >= 0;
    public int  ComponentCount  => m_Vers.Length;

    public int Major => m_Vers[0];
    public int Minor => m_Vers[1 % m_Vers.Length]; // lazy safety
    public int Patch => m_Vers[2 % m_Vers.Length]; // lazy safety

    public int this[int i] => m_Vers[i % m_Vers.Length];


    private const char SEPARATOR = '.';
    private static readonly char[] TAG_DELIMS = { '-', '+', '/' };
    private static readonly char[] TRIM_CHARS = { 'v', 'V', '(', ')', '-', '+' };

    private const int NYBBLE = 4; // bits in half a byte

    private const uint HASH_MASK_MAJOR = 0x0FF00000;
    private const uint HASH_MASK_MINOR = 0x000FF000;
    private const uint HASH_MASK_PATCH = 0x00000FF0;
    private const uint HASH_MASK_EXTRA = 0x0000000F;

    [SerializeField]
    private string  m_String;

    [SerializeField]
    private int     m_OrderedHash;

    [SerializeField]
    private int[]   m_Vers = new int[] { 0 };



    public VersionID(string ver)
    {
      if (string.IsNullOrEmpty(ver))
        return;

      ver = ver.Trim(TRIM_CHARS);

      int start = 0;
      while (ver[start] < '0' || ver[start] > '9')
      {
        if (++start == ver.Length)
          return;
      }

      int end = ver.IndexOfAny(TAG_DELIMS, start);
      if (end < 0)
        end = ver.Length;

      var splits = ver.Substring(start, end - start).Split(SEPARATOR);
      int len = splits.Length;

      m_Vers = new int[len];

      for (int i = 0; i < len; ++i)
      {
        if (!int.TryParse(splits[i], out m_Vers[i]))
        {
          m_Vers[i] = splits[i].Length * -1;
        }
      }

      m_String = ver;

      m_OrderedHash = CalcOrderedHash(end);
    }

    private int CalcOrderedHash(int end)
    {
      int bitpos = 8 * sizeof(uint) - NYBBLE; // reserve top nybble
      int len    = m_Vers.Length;
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
        if (nano == 0) // niche but can't be too careful with nanohashes
          nano = 0xFF;
        hash |= nano;
      }

      return hash;
    }


    public override string ToString()
    {
      return m_String;
    }

    public override int GetHashCode()
    {
      return m_OrderedHash;
    }

    public int CompareTo(VersionID other)
    {
      Debug.Assert(other != null, "VersionID other != null");

      if (m_OrderedHash < other.m_OrderedHash)
        return -1;
      if (other.m_OrderedHash < m_OrderedHash)
        return  1;
      else
        return  0;
    }

    public override bool Equals(object other)
    {
      if (other == null)
        return false;

      if (other is VersionID vstr)
        return Equals(vstr);

      return other.ToString() == m_String;
    }
    public bool Equals(VersionID other)
    {
      return other is object && other.m_OrderedHash == m_OrderedHash; // check tag?
    }


    public static implicit operator string (VersionID vstr)
    {
      return vstr.m_String;
    }

    public static implicit operator VersionID (string ver)
    {
      return new VersionID(ver);
    }

    public static bool operator < (VersionID lhs, VersionID rhs)
    {
      return lhs is object && rhs is object && lhs.m_OrderedHash < rhs.m_OrderedHash;
    }

    public static bool operator > (VersionID lhs, VersionID rhs)
    {
      return lhs is object && rhs is object && rhs.m_OrderedHash < lhs.m_OrderedHash;
    }

    public static bool operator == (VersionID lhs, VersionID rhs)
    {
      return lhs?.Equals(rhs) ?? (rhs is null);
    }

    public static bool operator != (VersionID lhs, VersionID rhs)
    {
      return !( lhs?.Equals(rhs) ?? (rhs is null) );
    }

  } // end class VersionID

}
