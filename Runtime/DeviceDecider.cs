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
    public DeviceDecider(float threshold = DEFAULT_THRESHOLD, bool verbose = DEFAULT_VERBOSE)
    {
      m_Factors = new HashMap<DeviceDimension,DeviceFactor>();
      Threshold = threshold;
      Verbose   = verbose;
    }

    public DeviceDecider(DeviceDeciderData data, float threshold = DEFAULT_THRESHOLD, bool verbose = DEFAULT_VERBOSE)
    {
      m_Factors = new HashMap<DeviceDimension,DeviceFactor>();
      _         = Load(data);
      Threshold = threshold;
      Verbose   = verbose;
    }

    public bool Load(DeviceDeciderData data)
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
        if (ParseRow(row.Dimension, row.Key, row.Weight))
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


    const float DEFAULT_THRESHOLD = 1f;
    const float MAX_THRESHOLD     = float.MaxValue / 2f;
    const bool  DEFAULT_VERBOSE   = false;

    static readonly char[] KEY_SEPARATORS = { '|' };


    [PublicAPI]
    public float Threshold { get; set; }

    [PublicAPI]
    public bool Verbose { get; set; }


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


    public DeviceDecider Add([NotNull] DeviceFactor factor)
    {
      if (false == m_Factors.Map(factor.Dimension, factor, out var preexisting))
      {
        preexisting.Merge(factor);
      }
      else if (factor.Dimension.IsContinuous())
      {
        ++ ContinuousCount;
      }

      return this;
    }

    public int AddRow(DeviceDimension dimension, [NotNull] string discreteKey, float weight)
    {
      if (!m_Factors.Find(dimension, out var factor) || factor is null)
      {
        m_Factors[dimension] = factor = new DeviceFactor(dimension);

        if (dimension.IsContinuous())
        {
          ++ ContinuousCount;
        }
      }

      string[] splits = discreteKey.Split(KEY_SEPARATORS, System.StringSplitOptions.RemoveEmptyEntries);
      int added = 0;

      if (splits.IsEmpty())
        return added;

      foreach (string split in splits)
      {
        string trimmed = split.Trim();
        if (trimmed.Length > 0)
        {
          _ = factor.Key(trimmed, weight);
          ++ added;
        }
      }

      return added;
    }

    public bool ParseRow(string dimension, [NotNull] string key, string weight)
    {
      if (!float.TryParse(weight, out float w) ||
          !DeviceDimensions.TryParse(dimension, out DeviceDimension dim))
      {
        return false;
      }

      return AddRow(dim, key, w) > 0;
    }


    public void ClearFactors()
    {
      m_Factors.Clear();
      ContinuousCount = 0;
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


    public void Enable(float threshold = DEFAULT_THRESHOLD)
    {
      Threshold = threshold;
    }

    public void Disable(bool constantDecision = false)
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


    internal IEnumerable<DeviceFactor> GetContinuousFactors()
    {
      foreach (var factor in m_Factors.Values)
      {
        if (factor.IsContinuous)
          yield return factor;
      }
    }

    internal IEnumerable<DeviceFactor> GetDiscreteFactors()
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
