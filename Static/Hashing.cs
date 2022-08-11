/*! @file   Static/Hashing.cs
 *  @author levi\@leviperez.dev
 *  @date   2020-06-06
**/


namespace Ore
{
  /// <summary>
  /// Utility functions for making/combining hash codes.
  /// </summary>
  public static class Hashing
  {

    public static int SafeGetHashCode(object obj)
    {
      return obj?.GetHashCode() ?? 0;
    }


    public static uint MixHashes(uint h0, uint h1)
    {
      return h0 + (h0 << 5 | h0 >> 27) ^ h1;
    }
    public static uint MixHashes(int h0, int h1) => MixHashes((uint)h0, (uint)h1);

    public static uint MixHashes(uint h0, uint h1, uint h2)
    {
      h0 = h0 + (h0 << 5 | h0 >> 27) ^ h1;
      return h0 + (h0 << 5 | h0 >> 27) ^ h2;
    }
    public static uint MixHashes(int h0, int h1, int h2) => MixHashes((uint)h0, (uint)h1, (uint)h2);

    public static uint MixHashes(uint h0, uint h1, uint h2, uint h3)
    {
      h0 = h0 + (h0 << 5 | h0 >> 27) ^ h1;
      h0 = h0 + (h0 << 5 | h0 >> 27) ^ h2;
      return h0 + (h0 << 5 | h0 >> 27) ^ h3;
    }
    public static uint MixHashes(int h0, int h1, int h2, int h3) => MixHashes((uint)h0, (uint)h1, (uint)h2, (uint)h3);


    public static int MakeHash(object a, object b)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b));
    }

    public static int MakeHash(object a, object b, object c)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b),
                            (uint)SafeGetHashCode(c));
    }

    public static int MakeHash(object a, object b, object c, object d)
    {
      return (int)MixHashes((uint)SafeGetHashCode(a),
                            (uint)SafeGetHashCode(b),
                            (uint)SafeGetHashCode(c),
                            (uint)SafeGetHashCode(d));
    }

  } // end static class Hashing

}
