/*! @file       Runtime/MiniJSON.cs
 *  @author     
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-01
**/

using JetBrains.Annotations;

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text;


namespace MiniJSON
{
  /// <summary>
  ///   This class encodes and decodes JSON strings. <br/>
  ///   JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
  ///   All numbers are parsed to doubles. <br/> <br/>
  ///
  ///   Spec. details, see https://www.json.org/
  /// </summary>
  public static class Json
  {

    /// <summary>
    ///   Parses the string json into a value
    /// </summary>
    /// <param name="json">
    ///   A JSON string.
    /// </param>
    /// <returns>
    ///   A List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an
    ///   integer,a string, null, true, or false
    /// </returns>
    [PublicAPI]
    public static object Deserialize(string json)
    {
      // save the string for debug information
      if (json == null)
        return null;

      return Parser.Parse(json);
    }

    /// <summary>
    ///   Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
    /// </summary>
    /// <param name="json">
    ///   A Dictionary&lt;string, object&gt; / List&lt;object&gt;
    /// </param>
    /// <returns>
    ///   A JSON encoded string, or null if object 'json' is not serializable
    /// </returns>
    [PublicAPI]
    public static string Serialize(object obj)
    {
      return Serializer.Serialize(obj);
    }


    // beyond = impl: 

    sealed class Parser : System.IDisposable
    {
      public static object Parse(string jsonString)
      {
        using (var instance = new Parser(jsonString))
        {
          return instance.ParseValue();
        }
      }


      static bool IsWordBreak(char c)
      {
        const string WORD_BREAK = "{}[],:\"";
        return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
      }

      enum TOKEN
      {
        NONE,
        CURLY_OPEN,
        CURLY_CLOSE,
        SQUARED_OPEN,
        SQUARED_CLOSE,
        COLON,
        COMMA,
        STRING,
        NUMBER,
        TRUE,
        FALSE,
        NULL
      };


      StringReader m_Stream;


      Parser(string jsonString)
      {
        m_Stream = new StringReader(jsonString);
      }

      void System.IDisposable.Dispose()
      {
        m_Stream.Dispose();
        m_Stream = null;
      }

      Dictionary<string,object> ParseObject()
      {
        var table = new Dictionary<string,object>();

        // ditch opening brace
        m_Stream.Read();

        // {
        while (true)
        {
          switch (NextToken)
          {
            case TOKEN.NONE:
              return null;
            case TOKEN.COMMA:
              continue;
            case TOKEN.CURLY_CLOSE:
              return table;
            default:
              // name
              string name = ParseString();
              if (name == null)
                return null;

              // :
              if (NextToken != TOKEN.COLON)
                return null;

              // ditch the colon
              m_Stream.Read();

              // value
              table[name] = ParseValue();
              break;
          }
        }
      }

      List<object> ParseArray()
      {
        var array = new List<object>();

        // ditch opening bracket
        m_Stream.Read();

        // [
        while (true)
        {
          var nextToken = NextToken;

          switch (nextToken) 
          {
            case TOKEN.NONE:
              return null;
            case TOKEN.COMMA:
              continue;
            case TOKEN.SQUARED_CLOSE:
              return array;
            default:
              array.Add(ParseByToken(nextToken));
              break;
          }
        }
      }

      object ParseValue()
      {
        return ParseByToken(NextToken);
      }

      object ParseByToken(TOKEN token)
      {
        switch (token)
        {
          case TOKEN.STRING:
            return ParseString();
          case TOKEN.NUMBER:
            return ParseNumber();
          case TOKEN.CURLY_OPEN:
            return ParseObject();
          case TOKEN.SQUARED_OPEN:
            return ParseArray();
          case TOKEN.TRUE:
            return true;
          case TOKEN.FALSE:
            return false;
          case TOKEN.NULL:
            return null;
          default:
            return null;
        }
      }

      string ParseString()
      {
        var s = new StringBuilder();

        // ditch opening quote
        m_Stream.Read();

        while (true)
        {
          if (m_Stream.Peek() == -1)
            return s.ToString();

          char c = NextChar;

          switch (c)
          {
            case '"':
              return s.ToString();
            case '\\':
              if (m_Stream.Peek() == -1)
                return s.ToString();

              c = NextChar;

              switch (c)
              {
                case '"':
                case '\\':
                case '/':
                  s.Append(c);
                  break;
                case 'b':
                  s.Append('\b');
                  break;
                case 'f':
                  s.Append('\f');
                  break;
                case 'n':
                  s.Append('\n');
                  break;
                case 'r':
                  s.Append('\r');
                  break;
                case 't':
                  s.Append('\t');
                  break;
                case 'u':
                  var hex = new char[4];

                  for (int i = 0; i < 4; ++i)
                  {
                    hex[i] = NextChar;
                  }

                  s.Append((char)System.Convert.ToInt32(new string(hex), 16));
                  break;
              }
              break;
            default:
              s.Append(c);
              return s.ToString();
          }
        }
      }

      object ParseNumber()
      {
        string number = NextWord;

        if (number.IndexOf('.') == -1)
        {
          _ = long.TryParse(number, out long parsedInt);
          return parsedInt;
        }

        _ = double.TryParse(number, out double parsedDouble);
        return parsedDouble;
      }

      void EatWhitespace()
      {
        while (char.IsWhiteSpace(PeekChar))
        {
          m_Stream.Read();

          if (m_Stream.Peek() == -1)
            break;
        }
      }

      char PeekChar => System.Convert.ToChar(m_Stream.Peek());

      char NextChar => System.Convert.ToChar(m_Stream.Read());

      string NextWord
      {
        get
        {
          var word = new StringBuilder();

          while (!IsWordBreak(PeekChar))
          {
            word.Append(NextChar);

            if (m_Stream.Peek() == -1)
              break;
          }

          return word.ToString();
        }
      }

      TOKEN NextToken
      {
        get
        {
          EatWhitespace();

          if (m_Stream.Peek() == -1)
            return TOKEN.NONE;

          switch (PeekChar)
          {
            case '{':
              return TOKEN.CURLY_OPEN;
            case '}':
              m_Stream.Read();
              return TOKEN.CURLY_CLOSE;
            case '[':
              return TOKEN.SQUARED_OPEN;
            case ']':
              m_Stream.Read();
              return TOKEN.SQUARED_CLOSE;
            case ',':
              m_Stream.Read();
              return TOKEN.COMMA;
            case '"':
              return TOKEN.STRING;
            case ':':
              return TOKEN.COLON;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
            case '-':
              return TOKEN.NUMBER;
          }

          switch (NextWord)
          {
            case "false":
              return TOKEN.FALSE;
            case "true":
              return TOKEN.TRUE;
            case "null":
              return TOKEN.NULL;
          }

          return TOKEN.NONE;
        }
      }
    }

    sealed class Serializer
    {
      readonly StringBuilder m_Builder = new StringBuilder();

      public static string Serialize(object obj)
      {
        var instance = new Serializer();

        instance.SerializeValue(obj);

        return instance.m_Builder.ToString();
      }

      void SerializeValue(object value)
      {
        IList asList;
        IDictionary asDict;
        string asStr;

        if (value == null)
        {
          m_Builder.Append("null");
        }
        else if ((asStr = value as string) != null)
        {
          SerializeString(asStr);
        }
        else if (value is bool)
        {
          m_Builder.Append((bool) value ? "true" : "false");
        }
        else if ((asList = value as IList) != null)
        {
          SerializeArray(asList);
        }
        else if ((asDict = value as IDictionary) != null)
        {
          SerializeObject(asDict);
        }
        else if (value is char)
        {
          SerializeString(new string((char) value, 1));
        }
        else
        {
          SerializeOther(value);
        }
      }

      void SerializeObject(IDictionary obj)
      {
        bool first = true;

        m_Builder.Append('{');

        foreach (object e in obj.Keys)
        {
          if (!first)
            m_Builder.Append(',');

          SerializeString(e.ToString());
          m_Builder.Append(':');

          SerializeValue(obj[e]);

          first = false;
        }

        m_Builder.Append('}');
      }

      void SerializeArray(IList anArray)
      {
        m_Builder.Append('[');

        bool first = true;

        foreach (object obj in anArray)
        {
          if (!first)
            m_Builder.Append(',');

          SerializeValue(obj);

          first = false;
        }

        m_Builder.Append(']');
      }

      void SerializeString(string str)
      {
        m_Builder.Append('\"');

        foreach (var c in str)
        {
          switch (c)
          {
            case '"':
              m_Builder.Append("\\\"");
              break;
            case '\\':
              m_Builder.Append("\\\\");
              break;
            case '\b':
              m_Builder.Append("\\b");
              break;
            case '\f':
              m_Builder.Append("\\f");
              break;
            case '\n':
              m_Builder.Append("\\n");
              break;
            case '\r':
              m_Builder.Append("\\r");
              break;
            case '\t':
              m_Builder.Append("\\t");
              break;
            default:
              int codepoint = System.Convert.ToInt32(c);
              if ((codepoint >= 32) && (codepoint <= 126))
              {
                m_Builder.Append(c);
              }
              else
              {
                m_Builder.Append("\\u");
                m_Builder.Append(codepoint.ToString("x4"));
              }
              break;
          }
        }

        m_Builder.Append('\"');
      }

      void SerializeOther(object value)
      {
        // NOTE: decimals lose precision during serialization.
        // They always have, I'm just letting you know.
        // Previously floats and doubles lost precision too.

        if (value is float)
        {
          m_Builder.Append(((float)value).ToString("R"));
        }
        else if (value is int    ||
                 value is uint   ||
                 value is long   ||
                 value is sbyte  ||
                 value is byte   ||
                 value is short  ||
                 value is ushort ||
                 value is ulong)
        {
          m_Builder.Append(value);
        }
        else if (value is double ||
                 value is decimal)
        {
          m_Builder.Append(System.Convert.ToDouble(value).ToString("R"));
        }
        else
        {
          SerializeString(value.ToString());
        }
      }
    }
  }
}


// Original COPYRIGHT:

/*
 * Copyright (c) 2013 Calvin Rien
 *
 * Based on the JSON parser by Patrick van Bergen
 * http://techblog.procurios.nl/k/618/news/view/14605/14863/How-do-I-write-my-own-parser-for-JSON.html
 *
 * Simplified it so that it doesn't throw exceptions
 * and can be used in Unity iPhone with maximum code stripping.
 *
 * Modified for Ore by Levi Perez (2023).
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
