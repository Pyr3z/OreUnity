/*! @file       Runtime/RecycledStringBuilder.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-06
**/

using JetBrains.Annotations;

using UnityEngine;

using System.Collections.Generic;

using System.Text;


namespace Ore
{
  /// <summary>
  ///   Temporary: Refer to Tests/Editor/MiscInEditor.cs to find usage examples
  ///   of this API. <br/> <br/>
  ///
  ///   TL;DR - construct these in using scopes.
  /// </summary>
  [PublicAPI]
  public struct RecycledStringBuilder : System.IDisposable
  {

    public RecycledStringBuilder(out StringBuilder bob)
    {
      bob = m_Builder = HireBuilder(string.Empty, DEFAULT_BOB_SIZE);
    }

    public RecycledStringBuilder(int initCapacity, out StringBuilder bob)
    {
      bob = m_Builder = HireBuilder(string.Empty, initCapacity);
    }

    public RecycledStringBuilder(string init, out StringBuilder bob)
    {
      bob = m_Builder = HireBuilder(init, DEFAULT_BOB_SIZE);
    }

    public RecycledStringBuilder(string init, int initCapacity, out StringBuilder bob)
    {
      bob = m_Builder = HireBuilder(init, initCapacity);
    }

    // the things I do for sweeter syntax... ;P


    public void Dispose()
    {
      FireBuilder(m_Builder);
      m_Builder = null;
    }


    StringBuilder m_Builder;


  #region Static section

    #if UNITY_INCLUDE_TESTS
    internal static int AliveCount { get; private set; }
    #endif

    const int DEFAULT_BOB_SIZE = 16;
    const int MAX_RET_BOB_SIZE = 1024 * 128;
    const int FIXED_POOL_SIZE  = 4;

    static int s_Next;
    static readonly StringBuilder[] UNEMPLOYED;

    static RecycledStringBuilder()
    {
      UNEMPLOYED    = new StringBuilder[FIXED_POOL_SIZE];
      UNEMPLOYED[0] = new StringBuilder(DEFAULT_BOB_SIZE);

      Application.lowMemory += OnLowMemory;
    }

    static void OnLowMemory()
    {
      System.Array.Clear(UNEMPLOYED, 1, FIXED_POOL_SIZE - 1);

      var luckyBastard = UNEMPLOYED[0];
      if (luckyBastard != null)
      {
        luckyBastard.Capacity = DEFAULT_BOB_SIZE;
      }

      s_Next = 0;
    }

    static StringBuilder HireBuilder(string init, int initCapacity)
    {
      ++ AliveCount;

      initCapacity = initCapacity.AtLeast(DEFAULT_BOB_SIZE);

      var bob = UNEMPLOYED[s_Next];

      if (bob is null)
        return new StringBuilder(init, initCapacity);

      UNEMPLOYED[s_Next] = null;
      while (s_Next > 0 && UNEMPLOYED[--s_Next] == null)
      {
        // no-op
      }

      bob.EnsureCapacity(initCapacity);
      bob.Append(init);

      return bob;
    }

    static void FireBuilder(StringBuilder bob)
    {
      if (bob == null)
        return;

      -- AliveCount;

      while (UNEMPLOYED[s_Next] != null)
      {
        if (s_Next == FIXED_POOL_SIZE - 1)
          return;

        ++ s_Next;
      }

      bob.Length = 0;

      if (bob.Capacity > MAX_RET_BOB_SIZE)
        bob.Capacity = MAX_RET_BOB_SIZE;

      UNEMPLOYED[s_Next] = bob;
    }

  #endregion Static section

  } // end struct RecycledStringBuilder
}