/*! @file       Editor/PrefValue.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-17
**/

using UnityEditor;
using JetBrains.Annotations;
using UnityEngine;
using Convert = System.Convert;
using IConvertible = System.IConvertible;
using IFormatProvider = System.IFormatProvider;
using CultureInfo = System.Globalization.CultureInfo;


namespace Ore.Editor
{
  [PublicAPI]
  public class PrefValue<T> where T : IConvertible
  {
    public T Value
    {
      get => Load();
      set
      {
        if (!Load().Equals(value))
        {
          m_Value = value;
          EditorPrefs.SetString(m_Key, Convert.ToString(m_Value, CultureInfo.InvariantCulture));
        }
      }
    }

    public static implicit operator T ([CanBeNull] PrefValue<T> pv)
    {
      if (pv is null)
        return default;

      return pv.Load();
    }


    private readonly string m_Key;
    private T               m_Value;
    private bool            m_Loaded;


    public PrefValue([NotNull] string key, T @default = default)
    {
      m_Key    = key;
      m_Value  = @default;
      m_Loaded = false;
    }

    private PrefValue()
    {
      // deleted default constructor
    }


    private T Load()
    {
      if (!m_Loaded)
      {
        string str = EditorPrefs.GetString(m_Key);

        if (str is T casted)
        {
          m_Value = casted;
        }
        else if (!str.IsEmpty())
        {
          m_Value = (T)Convert.ChangeType(str, typeof(T));
        }
        else
        {
          EditorPrefs.SetString(m_Key, Convert.ToString(m_Value, CultureInfo.InvariantCulture));
        }

        m_Loaded = true;
      }

      return m_Value;
    }

  } // end class PrefValue<T>
}