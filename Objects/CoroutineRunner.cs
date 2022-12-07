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
  public sealed class CoroutineRunner : OComponent, ICoroutineRunner
  {
    [PublicAPI]
    public int ActiveCoroutineCount => m_ActiveCoroutineCount;

    [PublicAPI]
    public int ActiveCoroutineWarnThreshold
    {
      get => m_CoroutineWarnThreshold;
      set
      {
        m_CoroutineWarnThreshold = value;
        CheckCoroutineThreshold();
      }
    }


    [SerializeField, Range(0, 64), Tooltip("Set to 0 to squelch the warning.")]
    private int m_CoroutineWarnThreshold = 16;

    [System.NonSerialized]
    private int m_NextCoroutineID, m_ActiveCoroutineCount;

    [System.NonSerialized]
    private readonly HashMap<object, CoroutineList> m_Map = new HashMap<object, CoroutineList>()
    {
      KeyComparator = new ObjectSavvyComparator()
    };


    public void Run(IEnumerator routine, Object key)
    {
      _ = StartCoroutine(routine, key);
    }

    public void Run(IEnumerator routine, string key)
    {
      _ = StartCoroutine(routine, key);
    }

    public void Run(IEnumerator routine, out string guidKey)
    {
      guidKey = Strings.MakeGUID();
            _ = StartCoroutine(routine, guidKey);
    }

    public void Run(IEnumerator routine)
    {
      _ = StartCoroutine(routine, this);
    }

    public void Halt(object key)
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

    public void HaltAll()
    {
      StopAllCoroutines();
      m_Map.Clear();
      m_ActiveCoroutineCount = 0;
    }


    [CanBeNull]
    private Coroutine StartCoroutine([NotNull] IEnumerator routine, [NotNull] object key)
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

      var scr = new SelfCleaningRoutine(this, routine, key, m_NextCoroutineID, contract);
      var coruPair = (base.StartCoroutine(scr), m_NextCoroutineID);

      list.Add(coruPair);

      ++ m_NextCoroutineID;
      ++ m_ActiveCoroutineCount;

      CheckCoroutineThreshold();

      return coruPair.Item1;
    }


    public void AdoptAndRun([NotNull] CoroutineRunnerBuffer buffer)
    {
      foreach (var (routine,key) in buffer)
      {
        if (routine is {} && key is {})
        {
          StartCoroutine(routine, key);
        }
      }

      buffer.HaltAll();
    }



    private void CheckCoroutineThreshold()
    {
      if (m_CoroutineWarnThreshold > 0 && m_ActiveCoroutineCount >= m_CoroutineWarnThreshold)
      {
        Orator.Warn($"Too many concurrent coroutines running on this object! n={m_ActiveCoroutineCount}", this);
      }
    }


    private struct SelfCleaningRoutine : IEnumerator
    {
      public object Current => m_Routine.Current;


      IEnumerator m_Routine;

      readonly CoroutineRunner m_Runner;
      readonly object          m_Key;
      readonly int             m_ID;
      readonly Object          m_Contract;


      internal SelfCleaningRoutine(CoroutineRunner runner, IEnumerator routine, object key, int id, Object contract)
      {
        m_Routine  = routine;
        m_Runner   = runner;
        m_Key      = key;
        m_ID       = id;
        m_Contract = contract;
      }


      bool IEnumerator.MoveNext()
      {
        if (m_Routine is null)
        {
          return false;
        }

        if (!m_Contract)
        {
          if (m_Runner.m_Map.Unmap(m_Key))
          {
            -- m_Runner.m_ActiveCoroutineCount;
          }

          m_Routine = null;
          return false;
        }

        if (m_Routine.MoveNext())
        {
          return true;
        }

        m_Routine = null;

        -- m_Runner.m_ActiveCoroutineCount;

        if (OAssert.Fails(m_Runner.m_Map.Pop(m_Key, out CoroutineList list), m_Runner))
        {
          return false;
        }

        int i = list.Count;

        while (i --> 0 && list[i].id != m_ID) ;

        if (i >= 0)
        {
          list.RemoveAt(i);
        }

        if (list.Count > 0)
        {
          // uncommon case: push the remainder of the list back
          m_Runner.m_Map.Map(m_Key, list);
        }

        return false;
      }

      void IEnumerator.Reset()
      {
        throw new System.InvalidOperationException();
      }
    }

  } // end class CoroutineRunner
}