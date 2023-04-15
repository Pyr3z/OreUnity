/*! @file     Runtime/DeviceEvaluator.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-05-10
**/

using JetBrains.Annotations;

using UnityEngine;


namespace Ore
{
  public class DeviceFactor
  {
    public DeviceFactor(DeviceDimension dim)
    {
      Dimension = dim;

      if (dim.IsContinuous())
      {
        m_ContinuousKeys = new AnimationCurve();
        m_DiscreteKeys   = null;
      }
      else
      {
        m_ContinuousKeys = null;
        m_DiscreteKeys   = new HashMap<string,float>();
      }
    }

    public readonly DeviceDimension Dimension;

    readonly AnimationCurve        m_ContinuousKeys;
    readonly HashMap<string,float> m_DiscreteKeys;

    public bool IsEmpty => m_ContinuousKeys?.length == 0 && m_DiscreteKeys?.Count == 0;

    internal AnimationCurve Curve => m_ContinuousKeys;


    [Pure]
    public float Evaluate()
    {
      if (Dimension.TryQueryValue(out float t, out string key) &&
          m_ContinuousKeys != null)
      {
        return m_ContinuousKeys.Evaluate(t);
      }

      if (m_DiscreteKeys?.TryGetValue(key, out float weight) ?? false)
      {
        return weight;
      }

      return 0f;
    }


    public DeviceFactor Key(float key, float weight) // continuous
    {
      m_ContinuousKeys?.AddKey(key, weight);
      return this;
    }

    public DeviceFactor Key(string key, float weight) // discrete (unless Timezone, AspectRatio)
    {
      if (key.IsEmpty())
      {
        if (m_DiscreteKeys != null)
          m_DiscreteKeys[string.Empty] = weight;
        return this;
      }

      if (Dimension == DeviceDimension.Timezone &&
          Parsing.TryParseTimezoneOffset(key, out float offset))
      {
        if (m_ContinuousKeys != null)
          m_ContinuousKeys.AddKey(offset, weight);
        return this;
      }

      if (Dimension == DeviceDimension.AspectRatio)
      {
        if (m_ContinuousKeys is null)
          return this;

        int colon = key.IndexOf(':');
        if (colon > 0 && float.TryParse(key.Remove(colon),      out float w) &&
                         float.TryParse(key.Substring(colon+1), out float h))
        {
          if (w < h) (w,h) = (h,w);
          m_ContinuousKeys.AddKey(w / h, weight);
          return this;
        }
      }

      if (m_ContinuousKeys != null && float.TryParse(key, out float f))
      {
        m_ContinuousKeys.AddKey(f, weight);
      }
      else if (m_DiscreteKeys != null)
      {
        m_DiscreteKeys[key] = weight;
      }

      return this;
    }

    public DeviceFactor EaseCurve()
    {
      if (m_ContinuousKeys is null)
        return this;

      for (int i = 0; i < m_ContinuousKeys.length; ++i)
      {
        var key = m_ContinuousKeys[i];
        key.inTangent   = 0f;
        key.outTangent  = 0f;
        m_ContinuousKeys.MoveKey(i, key);
      }

      return this;
    }

    public DeviceFactor SmoothCurve(float weight = 1f)
    {
      if (m_ContinuousKeys is null)
        return this;

      for (int i = 0; i < m_ContinuousKeys.length; ++i)
      {
        m_ContinuousKeys.SmoothTangents(i, weight);
      }

      return this;
    }


    public override string ToString()
    {
      return $"\"{Dimension}\" ({Dimension.QueryValue()})";
    }

  } // end class Evaluator

}
