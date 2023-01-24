/*! @file   Static/Bitwise+CTZ.cs
 *  @author levianperez\@gmail.com
 *  @author levi\@leviperez.dev
 *  @date   2020-06-06
 *  @date   2023-01-24 (removed alt implementation; added XML docs)
 *
 *  @brief
 *    Implements Count Trailing Zeroes, a slight variation of the
 *    [Find First Set](https://en.wikipedia.org/wiki/Find_first_set) bitwise
 *    instruction\. This is useful for reversing bitmasks and being able to loop
 *    through their non-zero bit indices.
 *
 *  @remarks
 *    Implementation uses a precomputed "De Bruijn" lookups Levi generated in C.
 *    Source: https://github.com/Pyr3z/hurry-c-lookupgen
 *
 *  @copyright
 *    This code was released into the Public Domain by Levi Perez in June 2020.
 *    The declaration was under the following terms, provided for convenience:
 *
 *      This is free and unencumbered software released into the public domain.
 *
 *      Anyone is free to copy, modify, publish, use, compile, sell, or
 *      distribute this software, either in source code form or as a compiled
 *      binary, for any purpose, commercial or non-commercial, and by any
 *      means.
 *
 *      In jurisdictions that recognize copyright laws, the author or authors
 *      of this software dedicate any and all copyright interest in the
 *      software to the public domain. We make this dedication for the benefit
 *      of the public at large and to the detriment of our heirs and
 *      successors. We intend this dedication to be an overt act of
 *      relinquishment in perpetuity of all present and future rights to this
 *      software under copyright law.
 *      
 *      THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 *      EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 *      MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 *      IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 *      OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 *      ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 *      OTHER DEALINGS IN THE SOFTWARE.
 *      
 *      For more information, please refer to <http://unlicense.org/>
**/

using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
using MethodImplOptions   = System.Runtime.CompilerServices.MethodImplOptions;


namespace Ore
{
  public static partial class Bitwise
  {
    /// <summary>
    ///   CTZ = Count Trailing Zeroes
    /// </summary>
    ///
    /// <returns>
    ///   The number of trailing zeroes in the binary representation,
    ///   i.e. <c>CTZ(0b0100)</c> returns 2. Specifically:
    ///     <li>if <c>bits</c> is non-zero, the 0-based index of the first set bit <c>[0,63]</c>.</li>
    ///     <li>if <c>bits</c> is zero, returns its bit width <c>[64]</c>.</li>
    /// </returns>
    /// 
    /// <remarks>
    ///   See also: <a href="https://en.wikipedia.org/wiki/Find_first_set">Find First Set (FFS)</a>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(ulong bits)
    {
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

    /// <summary>
    ///   CTZ = Count Trailing Zeroes
    /// </summary>
    ///
    /// <returns>
    ///   The number of trailing zeroes in the binary representation,
    ///   i.e. <c>CTZ(0b0100)</c> returns 2. Specifically:
    ///     <li>if <c>bits</c> is non-zero, the 0-based index of the first set bit <c>[0,31]</c>.</li>
    ///     <li>if <c>bits</c> is zero, returns its bit width <c>[32]</c>.</li>
    /// </returns>
    /// 
    /// <remarks>
    ///   See also: <a href="https://en.wikipedia.org/wiki/Find_first_set">Find First Set (FFS)</a>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(uint bits)
    {
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

    /// <summary>
    ///   CTZ = Count Trailing Zeroes
    /// </summary>
    ///
    /// <returns>
    ///   The number of trailing zeroes in the binary representation,
    ///   i.e. <c>CTZ(0b0100)</c> returns 2. Specifically:
    ///     <li>if <c>bits</c> is non-zero, the 0-based index of the first set bit <c>[0,15]</c>.</li>
    ///     <li>if <c>bits</c> is zero, returns its bit width <c>[16]</c>.</li>
    /// </returns>
    /// 
    /// <remarks>
    ///   See also: <a href="https://en.wikipedia.org/wiki/Find_first_set">Find First Set (FFS)</a>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(ushort bits)
    {
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

    /// <summary>
    ///   CTZ = Count Trailing Zeroes
    /// </summary>
    ///
    /// <returns>
    ///   The number of trailing zeroes in the binary representation,
    ///   i.e. <c>CTZ(0b0100)</c> returns 2. <br/> Specifically:
    ///     <li>if <c>bits</c> is non-zero, the 0-based index of the first set bit <c>[0,7]</c>.</li>
    ///     <li>if <c>bits</c> is zero, returns its bit width <c>[8]</c>.</li>
    /// </returns>
    /// 
    /// <remarks>
    ///   See also: <a href="https://en.wikipedia.org/wiki/Find_first_set">Find First Set (FFS)</a>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(byte bits)
    {
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

    /// <inheritdoc cref="Bitwise.CTZ(ulong)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(long bits)
    {
      return CTZ((ulong)bits);
    }

    /// <inheritdoc cref="Bitwise.CTZ(uint)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(int bits)
    {
      return CTZ((uint)bits);
    }

    /// <inheritdoc cref="Bitwise.CTZ(ushort)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(short bits)
    {
      return CTZ((ushort)bits);
    }

    /// <inheritdoc cref="Bitwise.CTZ(byte)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int CTZ(sbyte bits)
    {
      return CTZ((byte)bits);
    }


  #region Private data section

    // The following lookups were custom generated with a DeBruijn sequencer
    // that I (LP) wrote in C (https://github.com/Pyr3z/hurry-c-lookupgen):

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

