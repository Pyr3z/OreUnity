/*! @file       Editor/PrefValue.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-10-17
**/

using UnityEditor;
using JetBrains.Annotations;

using Convert = System.Convert;
using IConvertible = System.IConvertible;
using IFormatProvider = System.IFormatProvider;


namespace Ore.Editor
{
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
          EditorPrefs.SetString(m_Key, Convert.ToString(m_Value, INVARIANT));
        }
      }
    }

    public static implicit operator T ([CanBeNull] PrefValue<T> pv)
    {
      if (pv is null)
        return default;

      return pv.Load();
    }


    private static readonly IFormatProvider INVARIANT = System.Globalization.CultureInfo.InvariantCulture;

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
        var str = EditorPrefs.GetString(m_Key);

        if (str is T casted)
        {
          m_Value = casted;
        }
        else if (!str.IsEmpty())
        {
          m_Value = (T)Convert.ChangeType(str, typeof(T));
        }

        m_Loaded = true;
      }

      return m_Value;
    }

  } // end class PrefValue<T>
}