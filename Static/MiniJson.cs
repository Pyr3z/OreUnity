/*! @file       Runtime/MiniJson.cs
 *  @author     Calvin Rien (https://gist.github.com/darktable/1411710)
 *              (license info at end of file)
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-01
**/

using JetBrains.Annotations;

using System.Collections;

using System.IO;
using System.Text;

using JsonObj = System.Collections.Generic.Dictionary<string,object>;
using JsonArr = System.Collections.Generic.List<object>;


namespace Ore
{
  /// <summary>
  ///   This class encodes and decodes JSON strings. <br/> <br/>
  ///
  ///   JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
  ///   All numbers are parsed to doubles. <br/> <br/>
  ///
  ///   Spec. details, see https://www.json.org/
  /// </summary>
  public static class MiniJson
  {

    /// <param name="json">
    ///   The JSON string to parse.
    /// </param>
    /// <returns>
    ///   A List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an
    ///   integer,a string, null, true, or false
    /// </returns>
    [PublicAPI] [CanBeNull]
    public static object Deserialize([CanBeNull] string json)
    {
      return RecursiveParser.Parse(json);
    }

    /// <summary>
    ///   Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
    /// </summary>
    /// <param name="obj">
    ///   A Dictionary&lt;string, object&gt; / List&lt;object&gt;
    /// </param>
    /// <returns>
    ///   A JSON encoded string, or null if object 'json' is not serializable
    /// </returns>
    [PublicAPI] [CanBeNull]
    public static string Serialize([CanBeNull] object obj)
    {
      return Serializer.ToJson(obj);
    }


    // beyond = impl: 

    struct RecursiveParser : System.IDisposable
    {
      public static object Parse(string jsonString)
      {
        if (jsonString.IsEmpty())
          return jsonString;

        using (var instance = new RecursiveParser(jsonString))
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


      RecursiveParser(string jsonString)
      {
        m_Stream = new StringReader(jsonString);
      }

      void System.IDisposable.Dispose()
      {
        m_Stream.Dispose();
        m_Stream = null;
      }

      JsonObj ParseObject()
      {
        var jobj = new JsonObj();

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
              return jobj;
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
              jobj[name] = ParseValue();
              continue;
          }
        }
      }

      JsonArr ParseArray()
      {
        var array = new JsonArr();

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
          default:
          case TOKEN.NULL:
            return null;
        }
      }

      string ParseString()
      {
        var builder = new StringBuilder();

        // ditch opening quote
        m_Stream.Read();

        while (true)
        {
          if (m_Stream.Peek() == -1)
            return builder.ToString();

          char c = NextChar;

          switch (c)
          {
            case '"':
              return builder.ToString();

            case '\\':
              if (m_Stream.Peek() == -1)
                return builder.ToString();

              c = NextChar;

              switch (c)
              {
                case '"':
                case '\\':
                case '/':
                  builder.Append(c);
                  break;
                case 'b':
                  builder.Append('\b');
                  break;
                case 'f':
                  builder.Append('\f');
                  break;
                case 'n':
                  builder.Append('\n');
                  break;
                case 'r':
                  builder.Append('\r');
                  break;
                case 't':
                  builder.Append('\t');
                  break;
                case 'u':
                  var hex = new char[4];

                  for (int i = 0; i < 4; ++i)
                  {
                    hex[i] = NextChar;
                  }

                  builder.Append((char)System.Convert.ToInt32(new string(hex), 16));
                  break;
              }
              break;

            default:
              builder.Append(c);
              break;
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

    } // end nested class RecursiveParser


    static class Serializer
    {
      const int BUILDER_INIT_CAP = 256;
      static readonly StringBuilder s_Builder = new StringBuilder(BUILDER_INIT_CAP);

      public static string ToJson(object obj)
      {
        s_Builder.Clear();

        SerializeValue(obj);

        return s_Builder.ToString();
      }

      static void SerializeValue(object value)
      {
        if (value == null)
        {
          s_Builder.Append("null");
        }
        else if (value is string str)
        {
          SerializeString(str);
        }
        else if (value is bool b)
        {
          s_Builder.Append(b ? "true" : "false");
        }
        else if (value is IDictionary dict)
        {
          SerializeObject(dict);
        }
        else if (value is IEnumerable list)
        {
          SerializeArray(list);
        }
        else if (value is char c)
        {
          s_Builder.Append('\"');
          s_Builder.Append(c);
          s_Builder.Append('\"');
        }
        else
        {
          SerializeOther(value);
        }
      }

      static void SerializeObject(IDictionary dict)
      {
        s_Builder.Append('{');

        bool first = true;

        var iter = dict.GetEnumerator();
        while (iter.MoveNext())
        {
          #if UNITY_ASSERTIONS
          OAssert.NotNull(iter.Key);
          #endif

          if (!first)
            s_Builder.Append(',');

          first = false;

          // ReSharper disable once PossibleNullReferenceException
          SerializeString(iter.Key.ToString());

          s_Builder.Append(':');

          SerializeValue(iter.Value);
        }

        s_Builder.Append('}');
      }

      static void SerializeArray(IEnumerable array)
      {
        s_Builder.Append('[');

        bool first = true;

        foreach (object obj in array)
        {
          if (!first)
            s_Builder.Append(',');

          SerializeValue(obj);

          first = false;
        }

        s_Builder.Append(']');
      }

      static void SerializeString(string str)
      {
        s_Builder.Append('\"');

        foreach (var c in str)
        {
          switch (c)
          {
            case '"':
              s_Builder.Append("\\\"");
              break;
            case '\\':
              s_Builder.Append("\\\\");
              break;
            case '\b':
              s_Builder.Append("\\b");
              break;
            case '\f':
              s_Builder.Append("\\f");
              break;
            case '\n':
              s_Builder.Append("\\n");
              break;
            case '\r':
              s_Builder.Append("\\r");
              break;
            case '\t':
              s_Builder.Append("\\t");
              break;
            default:
              int codepoint = System.Convert.ToInt32(c);
              if ((codepoint >= 32) && (codepoint <= 126))
              {
                s_Builder.Append(c);
              }
              else
              {
                s_Builder.Append("\\u");
                s_Builder.Append(codepoint.ToString("x4"));
              }
              break;
          }
        }

        s_Builder.Append('\"');
      }

      static void SerializeOther(object value)
      {
        // NOTE: decimals lose precision during serialization.
        // They always have, I'm just letting you know.
        // Previously floats and doubles lost precision too.

        var type = value.GetType();
        var code = System.Type.GetTypeCode(type);

        switch (code)
        {
          default:
          case System.TypeCode.String:
            SerializeString(value.ToString());
            break;

          case System.TypeCode.Single:
          case System.TypeCode.Double:
          case System.TypeCode.Decimal:
            s_Builder.Append(System.Convert.ToDouble(value).ToString("R"));
            break;

          case System.TypeCode.Int32:
          case System.TypeCode.Int64:
          case System.TypeCode.UInt32:
          case System.TypeCode.UInt64:
          case System.TypeCode.Int16:
          case System.TypeCode.UInt16:
          case System.TypeCode.Byte:
          case System.TypeCode.SByte:
            s_Builder.Append(value);
            break;

          case System.TypeCode.DateTime:
            s_Builder.Append(((System.DateTime)value).ToBinary());
            break;

          case System.TypeCode.Empty:
          case System.TypeCode.DBNull:
            s_Builder.Append("null");
            break;
        }
      }

    } // end nested class Serializer

  } // end static class MiniJson

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
