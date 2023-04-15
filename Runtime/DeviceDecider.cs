/*! @file     Runtime/DeviceDecider.cs
 *  @author   Levi Perez (levi\@leviperez.dev)
 *  @date     2022-06-01
**/

using JetBrains.Annotations;

using System.Collections.Generic;

using UnityEngine;


namespace Ore
{
  /// <summary>
  ///   A.K.A. DeviceDaddy; A.K.A. the data structure that drove LAUD.
  /// </summary>
  public class DeviceDecider
  {
    public DeviceDecider()
    {
      m_Factors = new HashMap<DeviceDimension,DeviceFactor>();
    }

    public DeviceDecider(SerialDeviceDecider sdd)
    {
      m_Factors = new HashMap<DeviceDimension,DeviceFactor>();
      _         = TryDeserialize(sdd);
    }


    const float DEFAULT_THRESHOLD = 1f;
    const float MAX_THRESHOLD     = float.MaxValue / 2f;
    const bool  DEFAULT_VERBOSE   = false;

    static readonly char[] KEY_SEPARATORS = { '|' };


    [PublicAPI]
    public float Threshold { get; set; } = DEFAULT_THRESHOLD;

    [PublicAPI]
    public bool Verbose { get; set; } = DEFAULT_VERBOSE;


    public bool Decide()
    {
      return Decide(out _ );
    }

    public bool Decide(out float sum)
    {
      // this version doesn't short-circuit.

      sum = 0f;

      if (IsDisabled(out bool decision))
      {
        if (Verbose)
        {
          Orator.Log<DeviceDecider>($" is disabled. Default decision: {decision}");
        }

        return decision;
      }

      if (Verbose)
      {
        foreach (var factor in m_Factors.Values)
        {
          float f = factor.Evaluate();
          Orator.Log<DeviceDecider>($"{factor} evaluated to weight {f}");
          sum += f;
        }

        decision = sum >= Threshold;

        Orator.Log<DeviceDecider>($"FINAL SUM = {sum:F2}; (decision = {decision})");

        return decision;
      }

      foreach (var factor in m_Factors.Values)
      {
        sum += factor.Evaluate();
      }

      return sum >= Threshold;
    }

    public bool DecideShort()
    {
      // this version "earlies out" by short-circuiting on exceeding the threshold.

      if (IsDisabled(out bool decision))
      {
        if (Verbose)
        {
          Orator.Log<DeviceDecider>($" is disabled. Default decision: {decision}");
        }

        return decision;
      }

      float sum = 0f;
      decision = false;

      if (Verbose)
      {
        foreach (var factor in m_Factors.Values)
        {
          float f = factor.Evaluate();
          Orator.Log<DeviceDecider>($"{factor} evaluated to weight {f}");

          sum      += f;
          decision =  sum >= Threshold;

          if (decision)
            break;
        }

        Orator.Log<DeviceDecider>($"FINAL SUM = {sum}; (decision = {decision})");
        return decision;
      }

      foreach (var factor in m_Factors.Values)
      {
        sum += factor.Evaluate();
        if (sum >= Threshold)
          return true;
      }

      return false;
    }


    public void AddFactor([NotNull] DeviceFactor factor)
    {
      if (false == m_Factors.Map(factor.Dimension, factor, out var preexisting))
      {

      }
    }

    public void ClearFactors()
    {
      m_Factors.Clear();
      ContinuousCount = 0;
    }


    public bool TryDeserialize(SerialDeviceDecider data)
    {
      if (data.Rows == null)
        return false;

      if (data.Rows.Length == 0)
      {
        return true; // s'gotta be empty on purpose, right?
      }

      int count = 0;

      foreach (var row in data.Rows)
      {
        if (TryParseRow(row.Dimension, row.Key, row.Weight))
        {
          ++ count;
        }
      }

      foreach (var factor in GetContinuousFactors())
      {
        if (data.EaseCurves)
        {
          factor.EaseCurve();
        }

        if (!data.SmoothCurves.ApproximatelyZero())
        {
          factor.SmoothCurve(data.SmoothCurves);
        }
      }

      return count > 0;
    }

    public bool TryParseRow(string dimension, string key, string weight)
    {
      if (!float.TryParse(weight, out float w) ||
          !DeviceDimensions.TryParse(dimension, out DeviceDimension dim))
      {
        return false;
      }

      if (!m_Factors.TryGetValue(dim, out DeviceFactor factor) || factor is null)
      {
        m_Factors[dim] = factor = new DeviceFactor(dim);

        if (dim.IsContinuous())
        {
          ++ ContinuousCount;
        }
      }

      var splits = key.Split(KEY_SEPARATORS, System.StringSplitOptions.RemoveEmptyEntries);

      if (splits.IsEmpty())
        return false;

      foreach (var split in splits)
      {
        if (!split.IsEmpty())
        {
          _ = factor.Key(split.Trim(), w);
        }
      }

      return true;
    }


    public void EaseCurves()
    {
      foreach (var factor in GetContinuousFactors())
      {
        factor.EaseCurve();
      }
    }

    public void SmoothCurves(float weight = 1f)
    {
      foreach (var factor in GetContinuousFactors())
      {
        factor.SmoothCurve(weight);
      }
    }


    public void Disable(bool constantDecision)
    {
      Threshold = constantDecision ? 0f : MAX_THRESHOLD;
    }

    public bool IsDisabled()
    {
      return Threshold < Floats.Epsilon || Threshold >= MAX_THRESHOLD;
    }

    public bool IsDisabled(out bool defaultDecision)
    {
      defaultDecision = Threshold < MAX_THRESHOLD;
      return Threshold < Floats.Epsilon || Threshold >= MAX_THRESHOLD;
    }


    public IEnumerable<DeviceFactor> GetContinuousFactors()
    {
      foreach (var factor in m_Factors.Values)
      {
        if (factor.IsContinuous)
          yield return factor;
      }
    }

    public IEnumerable<DeviceFactor> GetDiscreteFactors()
    {
      foreach (var factor in m_Factors.Values)
      {
        if (factor.IsDiscrete)
          yield return factor;
      }
    }


    internal int FactorCount => m_Factors.Count;

    internal int ContinuousCount { get; private set; }


    readonly HashMap<DeviceDimension,DeviceFactor> m_Factors;

  } // end class DeviceDecider

}
