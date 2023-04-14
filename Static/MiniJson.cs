/*! @file       Runtime/MiniJson.cs
 *  @author     Calvin Rien (https://gist.github.com/darktable/1411710)
 *              (license info at end of file)
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2023-04-01
**/

using JetBrains.Annotations;

using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text;

using Type = System.Type;
using TypeCode = System.TypeCode;
using SerializeField = UnityEngine.SerializeField;
using NonSerialized = System.NonSerializedAttribute;


namespace Ore
{
  using JsonObj   = IDictionary<string,object>;
  using JsonArr   = IList<object>;
  using MapMaker  = System.Func<int, IDictionary<string,object>>;
  using ListMaker = System.Func<int, IList<object>>;
    // using these statements because fully-qualified delegates can't participate in C# contravariance


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

    /// <inheritdoc cref="Deserialize(string,System.Type)"/>
    [CanBeNull]
    public static object Deserialize([CanBeNull] string json)
    {
      // ReSharper disable once AssignNullToNotNullAttribute
      return json.IsEmpty() ? null : RecursiveParser.Parse(new StringReader(json));
    }

    /// <param name="json">
    ///   The JSON string to parse.
    /// </param>
    /// <param name="type">
    ///   Optionally provide a runtime type to try and construct & deserialize the
    ///   json onto. WARNING: supplying this parameter means you agree to the
    ///   reflection overhead. Other deserializers <i>also</i> use reflection;
    ///   I'm just giving you fair warning. <br/> <br/>
    ///   Oh, and the type must be default constructible.
    /// </param>
    /// <returns>
    ///   If a type was provided, returns an object of that type, the type itself
    ///   (if it represents a static class), or null. <br/> <br/>
    ///   Otherwise, any of the standard JSON types. That is, a <see cref="JsonArr"/>,
    ///   a <see cref="JsonObj"/>, a double, a long, a bool, a string, or null.
    /// </returns>
    [CanBeNull]
    public static object Deserialize([CanBeNull] string json, Type type)
    {
      // ReSharper disable once AssignNullToNotNullAttribute
      return json.IsEmpty() ? null : DeserializeStream(new StringReader(json), type);
    }

    public static bool TryDeserialize<T>([CanBeNull] string json, out T obj)
      where T : new()
    {
      if (json.IsEmpty())
      {
        obj = default;
        return false;
      }

      // ReSharper disable once AssignNullToNotNullAttribute
      return TryDeserializeStream(new StringReader(json), out obj);
    }

    public static bool TryDeserializeOverwrite<T>([CanBeNull] string json, [NotNull] ref T obj)
    {
      // ReSharper disable once AssignNullToNotNullAttribute
      return !json.IsEmpty() && TryDeserializeStreamOverwrite(new StringReader(json), ref obj);
    }


    [CanBeNull]
    public static object DeserializeStream([NotNull] TextReader stream)
    {
      return RecursiveParser.Parse(stream);
    }

    [CanBeNull]
    public static object DeserializeStream([NotNull] TextReader stream, Type type)
    {
      var parsed = RecursiveParser.Parse(stream);
      if (type is null)
        return parsed;

      var obj = type.ConstructDefault();
      if (obj is null)
      {
        Orator.Warn(type, "Given type is not default constructible!");
        return parsed;
      }

      if (parsed is JsonObj jobj)
      {
        // reflection warning!
        _ = ReflectFields(type, jobj, ref obj);
      }

      return obj;
    }

    public static bool TryDeserializeStream<T>([NotNull] TextReader stream, out T obj)
      where T : new()
    {
      var parsed = RecursiveParser.Parse(stream);

      if (parsed is T casted)
      {
        obj = casted;
        return true;
      }

      if (parsed is JsonObj jobj)
      {
        obj = new T();

        if (obj is IDictionary<string,object> dict)
        {
          foreach (var kvp in jobj)
          {
            dict.Add(kvp);
          }
          return true;
        }

        // reflection warning!
        return ReflectFields(typeof(T), jobj, ref obj);
      }

      if (parsed is JsonArr jarr && typeof(IList).IsAssignableFrom(typeof(T)))
      {
        obj = new T();

        if (!(obj is IList list))
          return false;

        foreach (var item in jarr)
        {
          list.Add(item);
        }

        return true;
      }

      obj = default;
      return false;
    }

    public static bool TryDeserializeStreamOverwrite<T>([NotNull] TextReader stream, [NotNull] ref T obj)
    {
      // reflection warning!
      return ReflectFields(typeof(T), RecursiveParser.Parse(stream) as JsonObj, ref obj);
    }


    /// <summary>
    ///   Converts an IDictionary / IList object or a simple type (string, int,
    ///   etc.) into a JSON string
    /// </summary>
    /// <param name="obj">
    ///   A Dictionary&lt;string, object&gt; / List&lt;object&gt;
    /// </param>
    /// <param name="pretty">
    ///   Pretty print the JSON.
    /// </param>
    /// <returns>
    ///   A JSON encoded string, or null if object 'json' is not serializable
    /// </returns>
    [CanBeNull]
    public static string Serialize([CanBeNull] object obj, bool pretty = EditorBridge.IS_DEBUG)
    {
      return RecursiveSerializer.MakeString(obj, pretty);
    }

    public static void SerializeTo([NotNull] TextWriter stream, [CanBeNull] object obj, bool pretty = EditorBridge.IS_DEBUG)
    {
      RecursiveSerializer.WriteTo(stream, obj, pretty);
    }


    internal struct ParserScope : System.IDisposable
    {
      public ParserScope(MapMaker objMaker, ListMaker arrMaker)
      {
        m_RestoreObjMaker = JsonObjMaker;
        JsonObjMaker      = objMaker ?? JsonAuthority.DefaultMapMaker;
        m_RestoreArrMaker = JsonArrMaker;
        JsonArrMaker      = arrMaker ?? JsonAuthority.DefaultListMaker;
      }

      public void Dispose()
      {
        JsonObjMaker = m_RestoreObjMaker ?? JsonAuthority.DefaultMapMaker;
        JsonArrMaker = m_RestoreArrMaker ?? JsonAuthority.DefaultListMaker;
      }

      readonly MapMaker  m_RestoreObjMaker;
      readonly ListMaker m_RestoreArrMaker;

    } // end struct ParserScope


    //
    // beyond = impl: 
    //

    static bool ReflectFields<T>(Type type, JsonObj data, ref T target)
    {
      if (data is null)
        return false;

      try
      {
        foreach (var field in type.GetFields(TypeMembers.INSTANCE))
        {
          if (!field.IsDefined<NonSerialized>() && data.TryGetValue(field.Name, out object value))
          {
            field.SetValue(target, value);
          }
        }

        return true;
      }
      catch (System.Exception ex)
      {
        Orator.NFE(ex);
        return false;
      }
    }


    static MapMaker  JsonObjMaker = JsonAuthority.DefaultMapMaker;
    static ListMaker JsonArrMaker = JsonAuthority.DefaultListMaker;


    struct RecursiveParser : System.IDisposable
    {
      public static object Parse([NotNull] TextReader stream)
      {
        if (stream.Peek() == -1)
        {
          return null;
        }

        using (var instance = new RecursiveParser(stream))
        {
          return instance.ParseByToken(instance.NextToken);
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


      TextReader m_Stream;


      RecursiveParser(TextReader stream)
      {
        m_Stream = stream;
      }

      void System.IDisposable.Dispose()
      {
        m_Stream.Dispose();
        m_Stream = null;
      }

      JsonObj ParseObject()
      {
        var jobj = JsonObjMaker(1);

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
              jobj[name] = ParseByToken(NextToken);
              continue;
          }
        }
      }

      JsonArr ParseArray()
      {
        var array = JsonArrMaker(1);

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
        // ditch opening quote
        m_Stream.Read();

        using (new RecycledStringBuilder(out var builder))
        {
          loop:

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
              } // end inner switch
              break;

            default:
              builder.Append(c);
              break;
          } // end outer switch

          goto loop;
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
          using (new RecycledStringBuilder(out var word))
          {
            while (!IsWordBreak(PeekChar))
            {
              word.Append(NextChar);

              if (m_Stream.Peek() == -1)
                break;
            }

            return word.ToString();
          }
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


    struct RecursiveSerializer : System.IDisposable
    {
      public static string MakeString(object obj, bool pretty, int indentIncr = 2)
      {
        using (var stringer = new RecursiveSerializer(pretty, indentIncr))
        {
          stringer.WriteValue(obj);
          return stringer.m_Stream.ToString();
        }
      }

      public static void WriteTo(TextWriter stream, object obj, bool pretty, int indentIncr = 2)
      {
        using (var writer = new RecursiveSerializer(stream, pretty, indentIncr))
        {
          writer.WriteValue(obj);
        }
      }


      RecursiveSerializer(bool pretty, int indentIncr)
      {
        m_Stream     = StringStream.ForBuilder();
        m_Indent     = 0;
        m_Pretty     = pretty;
        m_IndentIncr = indentIncr;

        if (pretty)
        {
          m_OpenObj = "{\n";
          m_OpenArr = "[\n";
          m_Comma   = ",\n";
          m_Colon   = ": ";
        }
        else
        {
          m_OpenObj = "{";
          m_OpenArr = "[";
          m_Comma   = ",";
          m_Colon   = ":";
        }
      }

      RecursiveSerializer(TextWriter stream, bool pretty, int indentIncr)
      {
        m_Stream     = StringStream.ForWriter(stream);
        m_Indent     = 0;
        m_Pretty     = pretty;
        m_IndentIncr = indentIncr;

        if (pretty)
        {
          m_OpenObj = "{\n";
          m_OpenArr = "[\n";
          m_Comma   = ",\n";
          m_Colon   = ": ";
        }
        else
        {
          m_OpenObj = "{";
          m_OpenArr = "[";
          m_Comma   = ",";
          m_Colon   = ":";
        }
      }

      void System.IDisposable.Dispose()
      {
        m_Stream.Dispose();
      }


      StringStream m_Stream;
      int          m_Indent;

      readonly bool   m_Pretty;
      readonly int    m_IndentIncr;
      readonly string m_OpenObj;
      readonly string m_OpenArr;
      readonly string m_Comma;
      readonly string m_Colon;


      void WriteValue(object value)
      {
        if (value == null)
        {
          m_Stream.Put("null");
        }
        else if (value is string str)
        {
          WriteString(str);
        }
        else if (value is bool b)
        {
          m_Stream.Put(b ? "true" : "false");
        }
        else if (value is IDictionary dict)
        {
          WriteObject(dict);
        }
        else if (value is IEnumerable list)
        {
          WriteArray(list);
        }
        else if (value is char c)
        {
          m_Stream.Put('\"');
          m_Stream.Put(c);
          m_Stream.Put('\"');
        }
        else
        {
          WriteOther(value);
        }
      }

      void WriteObject(IDictionary dict)
      {
        m_Indent += m_IndentIncr;

        int indent = m_Pretty ? m_Indent : 0;

        m_Stream.Put(m_OpenObj);

        bool first = true;

        var iter = dict.GetEnumerator();
        while (iter.MoveNext())
        {
          if (iter.Key is null)
            continue;

          if (!first)
            m_Stream.Put(m_Comma);

          first = false;

          m_Stream.Put(' ', indent);

          WriteString(iter.Key.ToString());

          m_Stream.Put(m_Colon);

          WriteValue(iter.Value);
        }

        m_Indent -= m_IndentIncr;

        if (m_Pretty)
        {
          m_Stream.Put('\n');
          m_Stream.Put(' ', m_Indent);
        }

        m_Stream.Put('}');
      }

      void WriteReflectedFields(object obj, Type type, bool nonPublic)
      {
        // reflection warning!

        m_Indent += m_IndentIncr;

        int indent = m_Pretty ? m_Indent : 0;

        m_Stream.Put(m_OpenObj);

        bool first = true;

        var flags = obj is null ? TypeMembers.STATIC : TypeMembers.INSTANCE;

        foreach (var field in type.GetFields(flags))
        {
          if (field.IsNotSerialized || ( !nonPublic && !field.IsPublic && !field.IsDefined<SerializeField>() ))
            continue;

          if (!first)
            m_Stream.Put(m_Comma);

          first = false;

          m_Stream.Put(' ', indent);

          WriteString(field.Name);

          m_Stream.Put(m_Colon);

          WriteValue(field.GetValue(obj));
        }

        if (obj is null)
        {
          foreach (var prop in type.GetProperties(flags))
          {
            if (prop.IsDefined<NonSerialized>())
              continue;

            var getter = prop.GetGetMethod(nonPublic);
            if (getter is null || getter.IsDefined<NonSerialized>())
              continue;

            if (!first)
              m_Stream.Put(m_Comma);

            first = false;

            m_Stream.Put(' ', indent);

            WriteString(prop.Name);

            m_Stream.Put(m_Colon);

            WriteValue(getter.Invoke(null, System.Array.Empty<object>()));
          }
        }

        m_Indent -= m_IndentIncr;

        if (m_Pretty)
        {
          m_Stream.Put('\n');
          m_Stream.Put(' ', m_Indent);
        }

        m_Stream.Put('}');
      }

      void WriteArray(IEnumerable array)
      {
        m_Indent += m_IndentIncr;

        int indent = m_Pretty ? m_Indent : 0;

        m_Stream.Put(m_OpenArr);

        bool first = true;

        foreach (object obj in array)
        {
          if (!first)
            m_Stream.Put(m_Comma);

          first = false;

          m_Stream.Put(' ', indent);

          WriteValue(obj);
        }

        m_Indent -= m_IndentIncr;

        if (m_Pretty)
        {
          m_Stream.Put('\n');
          m_Stream.Put(' ', m_Indent);
        }

        m_Stream.Put(']');
      }

      void WriteString(string str)
      {
        m_Stream.Put('\"');

        foreach (var c in str)
        {
          switch (c)
          {
            case '"':
              m_Stream.Put("\\\"");
              break;
            case '\\':
              m_Stream.Put("\\\\");
              break;
            case '\b':
              m_Stream.Put("\\b");
              break;
            case '\f':
              m_Stream.Put("\\f");
              break;
            case '\n':
              m_Stream.Put("\\n");
              break;
            case '\r':
              m_Stream.Put("\\r");
              break;
            case '\t':
              m_Stream.Put("\\t");
              break;
            default:
              int codepoint = System.Convert.ToInt32(c);
              if ((codepoint >= 32) && (codepoint <= 126))
              {
                m_Stream.Put(c);
              }
              else
              {
                m_Stream.Put("\\u");
                m_Stream.Put(codepoint.ToString("x4"));
              }
              break;
          }
        }

        m_Stream.Put('\"');
      }

      void WriteOther(object value)
      {
        // NOTE: decimals lose precision during serialization.
        // They always have, I'm just letting you know.
        // Previously floats and doubles lost precision too.

        var type = value.GetType();
        var code = Type.GetTypeCode(type);

        switch (code)
        {
          default:
          case TypeCode.String:
            WriteString(value.ToString());
            break;

          case TypeCode.Single:
          case TypeCode.Double:
          case TypeCode.Decimal:
            m_Stream.Put(System.Convert.ToDouble(value).ToString("R", Strings.InvariantFormatter));
            break;

          case TypeCode.Int32:
          case TypeCode.Int64:
          case TypeCode.UInt32:
          case TypeCode.UInt64:
          case TypeCode.Int16:
          case TypeCode.UInt16:
          case TypeCode.Byte:
          case TypeCode.SByte:
            string str = ((System.IConvertible)value).ToInvariant();
            if (type.IsEnum)
              WriteString(str);
            else
              m_Stream.Put(str);
            break;

          case TypeCode.DateTime:
            m_Stream.Put(((System.DateTime)value).ToUniversalTime().ToISO8601());
            break;

          case TypeCode.Empty:
          case TypeCode.DBNull:
            m_Stream.Put("null");
            break;

          case TypeCode.Object:
            if (value is System.TimeSpan span)
            {
              m_Stream.Put(span.Ticks.ToInvariant());
            }
            else if (value is SerialVersion sver)
            {
              WriteString(sver.ToString());
            }
            else if (value is Type ztatic)
            {
              WriteReflectedFields(null, ztatic, nonPublic: false);
            }
            else if (type.IsSerializable)
            {
              WriteReflectedFields(value, type, nonPublic: false);
            }
            else if (type.IsValueType)
            {
              WriteReflectedFields(value, type, nonPublic: true);
            }
            else
            {
              WriteString(value.ToString());
            }
            break;
        }
      }

    } // end nested class RecursiveSerializer

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
