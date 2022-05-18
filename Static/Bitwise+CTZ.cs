/** @file   Runtime/StaticTypes/Bitwise+CTZ.cs
    @author levianperez\@gmail.com
    @author levi\@leviperez.dev
    @date   2020-06-06

    @brief
      Implements [CTZ](https://en.wikipedia.org/wiki/Find_first_set)
      using precomputed "De Bruijn" lookups I generated in C.

    @remark
      If you supply the preprocessor define "BITWISE_CTZ_NOJUMP", this
      implementation will swap to using a shittier algorithm! :D
**/

namespace Bore
{
  public static partial class Bitwise
  {
    public static int CTZ(ulong bits)
    {
      /* Valid return range: [0,63] */
      /* Returns bits.GetBitWidth() (64 in this case) when passed 0. */
    #if BITWISE_CTZ_NOJUMP // (NOJUMP sucks don't use it)

      int ctz  = 0;
      int last = 0;

      ctz += ((bits & 0xFFFFFFFF).Not() << 5); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0000FFFF).Not() << 4); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x000000FF).Not() << 3); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0000000F).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x00000003).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x00000001).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
    #else // A lookup with a pre-computed DeBruijn sequence is fastest:
      if (bits == 0)
        return 64;
      
      return s_CTZLookup64[ ( LSB(bits) * c_DeBruijnKey64 ) >> 58 ];
    #endif
    }

    public static int CTZ(uint bits)
    {
      /* Valid return range: [0,31] */
      /* Returns bits.GetBitWidth() (32 in this case) when passed 0. */
    #if BITWISE_CTZ_NOJUMP

      int ctz = 0;
      int last = 0;

      ctz += ((bits & 0xFFFF).Not() << 4); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x00FF).Not() << 3); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x000F).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0003).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0001).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
    #else
      if (bits == 0)
        return 32;

      return s_CTZLookup32[ ( LSB(bits) * c_DeBruijnKey32 ) >> 27 ];
    #endif
    }

    public static int CTZ(ushort bits)
    {
      /* Valid return range: [0,15] */
      /* Returns bits.GetBitWidth() (16 in this case) when passed 0. */
    #if BITWISE_CTZ_NOJUMP

      int ctz = 0;
      int last = 0;

      ctz += ((bits & 0xFF).Not() << 3); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x0F).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x03).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x01).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
    #else
      if (bits == 0)
        return 16;

      return s_CTZLookup16[ (ushort)( LSB(bits) * c_DeBruijnKey16 ) >> 12 ];
    #endif
    }

    public static int CTZ(byte bits)
    {
      /* Valid return range: [0,7] */
      /* Returns bits.GetBitWidth() (8 in this case) when passed 0. */
    #if BITWISE_CTZ_NOJUMP

      int ctz = 0;
      int last = 0;

      ctz += ((bits & 0xF).Not() << 2); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x3).Not() << 1); bits >>= (ctz - last); last = ctz;
      ctz += ((bits & 0x1).Not()     );

      return ctz + ((bits >> (ctz - last)) & 1).Not();
    #else
      if (bits == 0)
        return 8;

      return s_CTZLookup8[ (byte)( LSB(bits) * c_DeBruijnKey8 ) >> 5 ];
    #endif
    }


    // The signed variants simply cast and call the unsigned versions:
    public static int CTZ(long bits)
    {
      return CTZ((ulong)bits);
    }

    public static int CTZ(int bits)
    {
      return CTZ((uint)bits);
    }

    public static int CTZ(short bits)
    {
      return CTZ((ushort)bits);
    }

    public static int CTZ(sbyte bits)
    {
      return CTZ((byte)bits);
    }


    #region Private data section
    /* The following lookups were custom generated with    */
    /* a DeBruijn sequence tool I wrote in C (circa 2020): */
    private static readonly int[] s_CTZLookup8  = { 0, 1, 2, 4, 7, 3, 6, 5 };

    private static readonly int[] s_CTZLookup16 = { 0,  1, 2, 5,  3,  9, 6,  11,
                                                    15, 4, 8, 10, 14, 7, 13, 12 };

    private static readonly int[] s_CTZLookup32 = { 0,  1,  2,  6,  3,  11, 7,  16, 4,  14, 12,
                                                    21, 8,  23, 17, 26, 31, 5,  10, 15, 13, 20,
                                                    22, 25, 30, 9,  19, 24, 29, 18, 28, 27 };

    private static readonly int[] s_CTZLookup64 = {
      0,  1,  2,  7,  3,  13, 8,  19, 4,  25, 14, 28, 9,  34, 20, 40, 5,  17, 26, 38, 15, 46,
      29, 48, 10, 31, 35, 54, 21, 50, 41, 57, 63, 6,  12, 18, 24, 27, 33, 39, 16, 37, 45, 47,
      30, 53, 49, 56, 62, 11, 23, 32, 36, 44, 52, 55, 61, 22, 43, 51, 60, 42, 59, 58
    };

    /* ... and the corresponding DeBruijn keys: */
    private const byte    c_DeBruijnKey8  = 0x17;               /* B(2,3) */
    private const ushort  c_DeBruijnKey16 = 0x9AF;              /* B(2,4) */
    private const uint    c_DeBruijnKey32 = 0x4653ADF;          /* B(2,5) */
    private const ulong   c_DeBruijnKey64 = 0x218A392CD3D5DBF;  /* B(2,6) */
    #endregion Private data section

  } // end static partial class Bitwise (CTZ)

}

