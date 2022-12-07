/*! @file       Objects/CoroutineRunner.cs
 *  @author     Levi Perez (levi\@leviperez.dev)
 *  @date       2022-12-06
**/

using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

using IEnumerator = System.Collections.IEnumerator;


namespace Ore
{
  using CoroutineList = List<(Coroutine coru, int id)>;

  [DisallowMultipleComponent]
  [AddComponentMenu("Ore//Coroutine Runner")] // though usually instantiated dynamically~
  public class CoroutineRunner : OComponent, ICoroutineRunner
  {

    [SerializeField, Range(0, 64), Tooltip("Set to 0 to squelch the warning.")]
    private int m_CoroutineWarnThreshold = 16;

    [System.NonSerialized]
    private int m_NextCoroutineID, m_ActiveCoroutineCount;

    [System.NonSerialized]
    private readonly HashMap<object, CoroutineList> m_Map = new HashMap<object, CoroutineList>()
    {
      KeyComparator = new ContractComparator()
    };


    public void EnqueueCoroutine(IEnumerator routine, Object key)
    {
      _ = StartCoroutine(routine, key);
    }

    public void EnqueueCoroutine(IEnumerator routine, string key)
    {
      _ = StartCoroutine(routine, key);
    }

    public void EnqueueCoroutine(IEnumerator routine, out string key)
    {
      key = Strings.MakeGUID();
      _ = StartCoroutine(routine, key);
    }

    public void EnqueueCoroutine(IEnumerator routine)
    {
      _ = StartCoroutine(routine, this);
    }

    public void CancelCoroutinesFor(object key)
    {
      if (!m_Map.Pop(key, out CoroutineList list))
        return;

      int i = list.Count;
      while (i --> 0)
      {
        if (list[i].coru is {})
        {
          StopCoroutine(list[i].coru);
          -- m_ActiveCoroutineCount;
        }
      }

      // let the garbage collector eat it since we called m_Map.Pop
    }

    public void CancelAllCoroutines()
    {
      StopAllCoroutines();
      m_Map.Clear();
      m_ActiveCoroutineCount = 0;
    }


    [CanBeNull]
    public Coroutine StartCoroutine([NotNull] IEnumerator routine, [NotNull] object key)
    {
      if (key is Object contract)
      {
        if (!contract)
          return null;
      }
      else
      {
        contract = this;
      }

      if (null == m_Map.TryMap(key, new CoroutineList(), out CoroutineList list))
      {
        Orator.Error($"Failed to start coroutine for \"{key}\"; HashMap state error.");
        return null;
      }

      var cpc = new CoroutinePlusCleanup(this, routine, key, m_NextCoroutineID, contract);
      var coruPair = (base.StartCoroutine(cpc), m_NextCoroutineID);

      list.Add(coruPair);

      ++ m_NextCoroutineID;
      ++ m_ActiveCoroutineCount;

      CheckCoroutineThreshold();

      return coruPair.Item1;
    }


    public void AdoptQueue([NotNull] CoroutineQueue queue)
    {
      if (queue.IsEmpty)
        return;

      foreach (var (routine,key) in queue)
      {
        if (routine is {} && key is {})
        {
          StartCoroutine(routine, key);
        }
      }

      queue.Clear();
    }



    private void CheckCoroutineThreshold()
    {
      if (m_CoroutineWarnThreshold > 0 && m_ActiveCoroutineCount >= m_CoroutineWarnThreshold)
      {
        Orator.Warn($"Too many concurrent coroutines running on this object! n={m_ActiveCoroutineCount}", this);
      }
    }


    private struct CoroutinePlusCleanup : IEnumerator
    {
      public object Current => m_Routine.Current;


      IEnumerator     m_Routine;
      CoroutineRunner m_Runner;
      readonly object m_Key;
      readonly int    m_ID;
      Object          m_Contract;


      public CoroutinePlusCleanup(CoroutineRunner runner, IEnumerator routine, object key, int id, Object contract)
      {
        m_Routine  = routine;
        m_Runner   = runner;
        m_Key      = key;
        m_ID       = id;
        m_Contract = contract;
      }


      public bool MoveNext()
      {
        if (ReferenceEquals(m_Runner, null))
        {
          return false;
        }

        if (!m_Contract)
        {
          if (m_Runner.m_Map.Unmap(m_Key))
          {
            -- m_Runner.m_ActiveCoroutineCount;
          }

          m_Runner = null;
          return false;
        }

        if (m_Routine.MoveNext())
        {
          return true;
        }

        -- m_Runner.m_ActiveCoroutineCount;

        if (OAssert.Fails(m_Runner.m_Map.Pop(m_Key, out CoroutineList list), m_Runner))
        {
          m_Runner = null;
          return false;
        }

        int i = list.Count;
        while (i --> 0)
        {
          if (list[i].id == m_ID)
          {
            list.RemoveAt(i);
            break;
          }
        }

        if (list.Count > 0)
        {
          // uncommon case: push the remainder of the list back
          m_Runner.m_Map.Map(m_Key, list);
        }

        m_Runner = null;
        return false;
      }

      void IEnumerator.Reset()
      {
        throw new System.InvalidOperationException();
      }
    }


    private sealed class ContractComparator : Comparator<object>
    {
      public override bool IsNone(in object obj)
      {
        if (obj is Object uobj)
          return !uobj;
        return base.IsNone(in obj);
      }
    }
  } // end class CoroutineRunner
}