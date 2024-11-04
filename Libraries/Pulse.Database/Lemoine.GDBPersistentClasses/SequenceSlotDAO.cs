// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate.Criterion;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ISequenceSlotDAO">ISequenceSlotDAO</see>
  /// </summary>
  public sealed class SequenceSlotDAO
    : GenericMachineModuleSlotDAO<SequenceSlot, ISequenceSlot>
    , ISequenceSlotDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    internal SequenceSlotDAO ()
      : base (false)
    {
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.ISequenceSlotDAO.FindAllAtAndAfter(IMachineModule, DateTime)">FindAllAtAndAfter</see>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<ISequenceSlot> FindAllAtAndAfter (IMachineModule machineModule,
                                                   DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Expression.Eq ("MachineModule", machineModule))
        .Add (Expression.Disjunction ()
              .Add (Expression.IsNull ("EndDateTime"))
              .Add (Expression.Gt ("EndDateTime", new UpperBound<DateTime> (dateTime))))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<ISequenceSlot> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.ISequenceSlotDAO.FindAllAtAndAfter(IMachineModule, DateTime, int)">FindAllAtAndAfter</see>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <param name="limit"></param>
    /// <returns></returns>
    public IList<ISequenceSlot> FindAllAtAndAfter (IMachineModule machineModule,
                                                   DateTime dateTime,
                                                   int limit)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Expression.Eq ("MachineModule", machineModule))
        .Add (Expression.Disjunction ()
              .Add (Expression.IsNull ("EndDateTime"))
              .Add (Expression.Gt ("EndDateTime", new UpperBound<DateTime> (dateTime))))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .SetMaxResults (limit)
        .List<ISequenceSlot> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.ISequenceSlotDAO.FindAllEndFrom">FindAllEndFrom</see>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<ISequenceSlot> FindAllEndFrom (IMachineModule machineModule,
                                                DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Expression.Eq ("MachineModule", machineModule))
        .Add (Expression.Disjunction ()
              .Add (Expression.IsNull ("EndDateTime"))
              .Add (Expression.Ge ("EndDateTime", new UpperBound<DateTime> (dateTime))))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .List<ISequenceSlot> ();
    }

    /// <summary>
    /// Find the first sequence slot whose end is before a specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public ISequenceSlot FindFirstBefore (IMachineModule machineModule,
                                          DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.IsNotNull ("EndDateTime"))
        .Add (Restrictions.Le ("EndDateTime", new UpperBound<DateTime> (dateTime)))
        .AddOrder (Order.Desc ("BeginDateTime"))
        .SetMaxResults (1)
        .UniqueResult<ISequenceSlot> ();
    }

    /// <summary>
    /// Find the first sequence slot whose begin is after (not strictly) a specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public ISequenceSlot FindFirstAfter (IMachineModule machineModule,
                                         DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Ge ("BeginDateTime", new LowerBound<DateTime> (dateTime)))
        .AddOrder (Order.Asc ("BeginDateTime"))
        .SetMaxResults (1)
        .UniqueResult<ISequenceSlot> ();
    }

    /// <summary>
    /// Find the last sequence slot for the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public ISequenceSlot FindLast (IMachineModule machineModule)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Expression.Eq ("MachineModule", machineModule))
        .AddOrder (Order.Desc ("BeginDateTime"))
        .SetMaxResults (1)
        .UniqueResult<ISequenceSlot> ();
    }

    /// <summary>
    /// Find the last sequence slot for the specified machine module
    /// whose begin is strictly before the specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public ISequenceSlot FindLastBefore (IMachineModule machineModule,
                                         DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Lt ("BeginDateTime", new LowerBound<DateTime> (dateTime)))
        .AddOrder (Order.Desc ("BeginDateTime"))
        .SetMaxResults (1)
        .UniqueResult<ISequenceSlot> ();
    }

    /// <summary>
    /// Find the last sequence slot for the specified machine module
    /// which intersects the specified date/time range
    /// and whose sequence is not null
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public ISequenceSlot FindLastWithSequence (IMachineModule machineModule,
                                               UtcDateTimeRange range)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Add (InUtcRange (range))
        .Add (Restrictions.IsNotNull ("Sequence"))
        .AddOrder (Order.Desc ("BeginDateTime"))
        .SetMaxResults (1)
        .UniqueResult<ISequenceSlot> ();
    }

    /// <summary>
    /// Find the sequence at the specific date/time.
    /// 
    /// Keep only the sequence slot with a not null sequence.
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <param name="at"></param>
    /// <returns></returns>
    public ISequenceSlot FindAtWithSequence (IMachineModule machineModule,
      DateTime at)
    {
      Debug.Assert (null != machineModule);

      var range = new UtcDateTimeRange (at, at, "[]");
      return FindLastWithSequence (machineModule, range);
    }

    /// <summary>
    /// Check if it exists a sequence slot with no associated sequence
    /// for the specified machine in the specified period
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="begin">in UTC</param>
    /// <param name="end">in UTC</param>
    /// <returns></returns>
    public bool ExistsNoSequenceBetween (IMonitoredMachine machine,
                                         DateTime begin,
                                         DateTime end)
    {
      if (end <= begin) { // Empty period
        return false;
      }
      
      // If you use a global requests for all the machine modules in the same time,
      // the request does not take profit of the partitioning
      foreach (IMachineModule machineModule in machine.MachineModules) {
        ISequenceSlot anySequenceSlot = NHibernateHelper.GetCurrentSession ()
          .CreateCriteria<SequenceSlot> ()
          .Add (Restrictions.Eq ("MachineModule", machineModule))
          .Add (Restrictions.IsNull ("Sequence"))
          .Add (Restrictions.Lt ("BeginDateTime", new LowerBound<DateTime> (end)))
          .Add (Expression.Disjunction ()
                .Add (Expression.IsNull ("EndDateTime"))
                .Add (Expression.Gt ("EndDateTime", new UpperBound<DateTime> (begin))))
          .SetMaxResults (1)
          .UniqueResult<ISequenceSlot> ();
        if (null != anySequenceSlot) {
          return true;
        }
      }
      return false;
    }
    
    /// <summary>
    /// Find all the sequence slots which begin is after (not strictly) the specified date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<ISequenceSlot> FindAllBeginAfter (IMachineModule machineModule,
                                            DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<SequenceSlot> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Ge ("BeginDateTime", new LowerBound<DateTime> (dateTime)))
        .AddOrder (Order.Desc ("BeginDateTime"))
        .List<ISequenceSlot> ();
    }
    
    /// <summary>
    /// <see cref="Lemoine.ModelDAO.ISequenceSlotDAO.FindAllNextBeginFrom">FindAllNextBeginFrom</see>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<ISequenceSlot> FindAllNextBeginFrom (IMachineModule machineModule,
                                                      DateTime dateTime)
    {
      IList<ISequenceSlot> sequenceSlots = FindAllBeginAfter (machineModule,
                                                              dateTime);
      ISequenceSlot lastBefore = FindLastBefore (machineModule, dateTime);
      if ( (null != lastBefore) && (lastBefore.NextBegin.HasValue)
          && (dateTime <= lastBefore.NextBegin.Value)) {
        sequenceSlots.Insert (0, lastBefore);
      }
      return sequenceSlots;
    }
  }
}
