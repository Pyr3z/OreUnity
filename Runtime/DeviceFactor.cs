/*! @file     Runtime/DeviceEvaluator.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-05-10
**/

using System.Collections.Generic;

using UnityEngine;


namespace Ore
{

  public class DeviceFactor
  {
    public DeviceDimension Dimension => m_Dimension;

    public bool IsContinuous => m_ContinuousKeys.length > 0;
    public bool IsDiscrete   => m_DiscreteKeys.Count > 0;

    public int Count => (m_ContinuousKeys?.length ?? 0) + (m_DiscreteKeys?.Count ?? 0);


    internal AnimationCurve Curve => m_ContinuousKeys;


    private readonly DeviceDimension       m_Dimension;
    private readonly AnimationCurve        m_ContinuousKeys = new AnimationCurve();
    private readonly HashMap<string,float> m_DiscreteKeys   = new HashMap<string,float>();


    public DeviceFactor(DeviceDimension dim)
    {
      m_Dimension = dim;
    }


    public DeviceFactor Key(float key, float weight) // continuous
    {
      Debug.Assert(m_Dimension.IsContinuous());

      m_ContinuousKeys.AddKey(key, weight);

      return this;
    }

    public DeviceFactor Key(string key, float weight) // discrete (unless Timezone or float key)
    {
      if (key.IsEmpty())
      {
        m_DiscreteKeys[string.Empty] = weight;
        return this;
      }

      if (m_Dimension == DeviceDimension.Timezone &&
          Parsing.TryParseTimezoneOffset(key, out float offset))
      {
        m_ContinuousKeys.AddKey(offset, weight);
        return this;
      }

      if (m_Dimension == DeviceDimension.AspectRatio)
      {
        int colon = key.IndexOf(':');
        if (colon > 0 && float.TryParse(key.Remove(colon),      out float w) &&
                         float.TryParse(key.Substring(colon+1), out float h))
        {
          if (w < h) (w,h) = (h,w);
          m_ContinuousKeys.AddKey(w / h, weight);
          return this;
        }
      }

      if (m_Dimension.IsContinuous() && float.TryParse(key, out float f))
      {
        m_ContinuousKeys.AddKey(f, weight);
      }
      else // add discrete:
      {
        m_DiscreteKeys[key] = weight;
      }

      return this;
    }

    public DeviceFactor EaseCurve()
    {
      Debug.Assert(m_Dimension.IsContinuous());

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
      Debug.Assert(m_Dimension.IsContinuous());

      for (int i = 0; i < m_ContinuousKeys.length; ++i)
      {
        m_ContinuousKeys.SmoothTangents(i, weight);
      }

      return this;
    }


    public override string ToString()
    {
      if (m_Dimension == DeviceDimension.None)
        return "\"None\"";
      else
        return $"\"{m_Dimension}\" ({m_Dimension.QueryValue()})";
    }


    public float Evaluate()
    {
      if (m_Dimension.TryQueryValue(out float t, out string key))
      {
        return m_ContinuousKeys.Evaluate(t);
      }

      if (m_DiscreteKeys.TryGetValue(key, out float weight))
      {
        return weight;
      }

      return 0f;
    }

  } // end class Evaluator

}
