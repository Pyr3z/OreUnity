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
      m_ContinuousKeys = new AnimationCurve();
      m_DiscreteKeys   = new HashMap<string,float>();
      // always make new objects here (K.I.S.S.)
    }

    public readonly DeviceDimension Dimension;

    readonly AnimationCurve        m_ContinuousKeys;
    readonly HashMap<string,float> m_DiscreteKeys;

    public bool IsEmpty => m_ContinuousKeys.length == 0 && m_DiscreteKeys.Count == 0;

    public bool IsContinuous => m_ContinuousKeys.length > 0;
    public bool IsDiscrete   => m_DiscreteKeys.Count > 0;


    internal AnimationCurve Curve => m_ContinuousKeys;


    [Pure]
    public float Evaluate()
    {
      if (Dimension.TryQueryValue(out float t, out string key))
      {
        return m_ContinuousKeys.Evaluate(t);
      }

      if (m_DiscreteKeys.TryGetValue(key, out float weight))
      {
        return weight;
      }

      return 0f;
    }


    public DeviceFactor Key(float key, float weight) // continuous
    {
      m_ContinuousKeys.AddKey(key, weight);
      return this;
    }

    public DeviceFactor Key(string key, float weight) // discrete (unless Timezone, AspectRatio)
    {
      if (key.IsEmpty())
      {
        m_DiscreteKeys[string.Empty] = weight;
        return this;
      }

      if (Dimension == DeviceDimension.Timezone &&
          Parsing.TryParseTimezoneOffset(key, out float offset))
      {
        m_ContinuousKeys.AddKey(offset, weight);
        return this;
      }

      if (Dimension == DeviceDimension.AspectRatio)
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

      if (Dimension.IsContinuous() && float.TryParse(key, out float f))
      {
        m_ContinuousKeys.AddKey(f, weight);
      }
      else
      {
        m_DiscreteKeys[key] = weight;
      }

      return this;
    }

    public DeviceFactor EaseCurve()
    {
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
      for (int i = 0; i < m_ContinuousKeys.length; ++i)
      {
        m_ContinuousKeys.SmoothTangents(i, weight);
      }

      return this;
    }

    public DeviceFactor Merge([NotNull] DeviceFactor other)
    {
      if (ReferenceEquals(this, other))
        return this;

      OAssert.True(Dimension == other.Dimension, "Dimensions must be equal");

      int ocl = other.m_ContinuousKeys.length;
      if (ocl > 0)
      {
        int cl = m_ContinuousKeys.length;
        if (cl == 0)
        {
          m_ContinuousKeys.keys = other.m_ContinuousKeys.keys;
        }
        else
        {
          var merged = new Keyframe[cl + ocl];

          m_ContinuousKeys.keys.CopyTo(merged, 0);
          other.m_ContinuousKeys.keys.CopyTo(merged, cl);

          // TODO sort keys?

          m_ContinuousKeys.keys = merged;
        }
      }

      if (other.m_DiscreteKeys.Count > 0)
      {
        // wanted to use HashMap.Union here, if not for the special additive behaviour
        foreach (var (key,val) in other.m_DiscreteKeys)
        {
          _ = m_DiscreteKeys.Find(key, out float pre);
          m_DiscreteKeys[key] = pre + val;
        }
      }

      return this;
    }


    public override string ToString()
    {
      return $"\"{Dimension}\" ({Dimension.QueryValue()})";
    }

  } // end class Evaluator

}
