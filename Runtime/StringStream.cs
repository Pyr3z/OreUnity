/*! @file       Runtime/StringStream.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-14
 *
 *  Adapter pattern for the marriage of System.Text.StringBuilder and
 *  System.IO.TextWriter.
**/

using JetBrains.Annotations;

using StringBuilder = System.Text.StringBuilder;
using TextWriter    = System.IO.TextWriter;
using StringWriter  = System.IO.StringWriter;


namespace Ore
{
  public struct StringStream : System.IDisposable
  {
    [PublicAPI]
    public static StringStream ForBuilder()
    {
      return new StringStream(RecycledStringBuilder.Borrow());
    }

    [PublicAPI]
    public static StringStream ForBuilder(StringBuilder bob)
    {
      return new StringStream(bob);
    }

    [PublicAPI]
    public static StringStream ForWriter(TextWriter writer)
    {
      return new StringStream(writer);
    }


    // [LP] Potential NREs in the following "Put" methods, but we should not
    // attempt to catch/handle them b/c it indicates bad usage of this adapter.

    public void Put(char c, int n = 1)
    {
      m_PutChar(c, n);
    }

    public void Put(string s)
    {
      m_PutStr(s);
    }


    public override string ToString()
    {
      if (m_Builder != null)
        return m_Builder.ToString();
      if (m_Writer is StringWriter strWriter)
        return strWriter.ToString();
      return string.Empty;
    }


    public void Dispose()
    {
      if (m_Builder != null)
      {
        RecycledStringBuilder.Return(m_Builder);
        m_Builder = null;
      }
      if (m_Writer != null)
      {
        m_Writer.Close();
        m_Writer = null;
      }
    }


    StringStream(StringBuilder outStream)
    {
      m_Builder = outStream;
      m_Writer  = null;
      m_PutChar = null;
      m_PutStr  = null;
      if (outStream != null)
      {
        m_PutChar = BuilderPut;
        m_PutStr  = BuilderPut;
      }
    }

    StringStream(TextWriter outStream)
    {
      m_Builder = null;
      m_Writer  = outStream;
      m_PutChar = null;
      m_PutStr  = null;
      if (outStream != null)
      {
        m_PutChar = WriterPut;
        m_PutStr  = WriterPut;
      }
    }


    StringBuilder m_Builder;
    TextWriter    m_Writer;

    System.Action<char,int> m_PutChar;
    System.Action<string>   m_PutStr;


    void BuilderPut(char c, int n)
    {
      m_Builder.Append(c, n);
    }

    void BuilderPut(string s)
    {
      m_Builder.Append(s);
    }

    void WriterPut(char c, int n)
    {
      while (n --> 0)
      {
        m_Writer.Write(c);
      }
    }

    void WriterPut(string s)
    {
      m_Writer.Write(s);
    }

  } // end struct StringStream
}