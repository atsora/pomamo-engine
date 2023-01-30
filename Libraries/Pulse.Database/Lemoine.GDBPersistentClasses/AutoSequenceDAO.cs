// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IAutoSequenceDAO">IAutoSequenceDAO</see>
  /// </summary>
  public class AutoSequenceDAO
    : VersionableByMachineModuleNHibernateDAO<AutoSequence, IAutoSequence, int>
    , IAutoSequenceDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public AutoSequenceDAO ()
      : base ("MachineModule")
    { }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IAutoSequenceDAO.FindAllAtAndAfter">FindAllAtAndAfter</see>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IAutoSequence GetFirstAfter (IMachineModule machineModule,
                                        DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<AutoSequence> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.IsNull ("End"))
              .Add (Restrictions.Gt ("End", (UpperBound<DateTime>)dateTime)))
        .SetMaxResults (1)
        .UniqueResult<IAutoSequence> ();
    }

    /// <summary>
    /// <see cref="Lemoine.ModelDAO.IAutoSequenceDAO.FindAllAtAndAfter">FindAllAtAndAfter</see>
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public IList<IAutoSequence> FindAllAtAndAfter (IMachineModule machineModule,
                                                   DateTime dateTime)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<AutoSequence> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Disjunction ()
              .Add (Restrictions.IsNull ("End"))
              .Add (Restrictions.Gt ("End", (UpperBound<DateTime>)dateTime)))
        .List<IAutoSequence> ();
    }

    /// <summary>
    /// Find all the auto-sequences for a given monitored machine
    /// 
    /// Order the result by ascending begin date/time
    /// </summary>
    /// <param name="monitoredMachine"></param>
    /// <returns></returns>
    public IList<IAutoSequence> FindAll (IMonitoredMachine monitoredMachine)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("AutoSequenceMonitoredMachine");
      query.SetParameter ("Machine", monitoredMachine);
      return query.List<IAutoSequence> ();
    }

    /// <summary>
    /// Find all the auto-sequences for a specified machine module in a specific range
    /// 
    /// Order the result by ascending begin date/time
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IAutoSequence> FindAllBetween (IMachineModule machineModule,
                                                UtcDateTimeRange range)
    {
      IList<IAutoSequence> autoSequences = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<AutoSequence> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        .List<IAutoSequence> ();
      return autoSequences;
    }
    
    /// <summary>
    /// Delete all the auto-sequences before a specified date/time
    /// (because they have already been processed)
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="dateTime"></param>
    public void DeleteBefore (IMachineModule machineModule,
                              DateTime dateTime)
    {
      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("AutoSequenceDeleteBefore");
      query.SetParameter ("MachineModuleParameter", machineModule);
      query.SetParameter ("BeforeParameter", (UpperBound<DateTime>)dateTime);
      query.ExecuteUpdate ();
    }
    
    AbstractCriterion InUtcRange (UtcDateTimeRange range)
    {
      if (!range.Lower.HasValue && !range.Upper.HasValue) { // (-oo,+oo)
        return Expression.Sql ("TRUE");
      }
      
      if (range.IsEmpty ()) {
        return Expression.Sql ("FALSE");
      }
      
      // From constraint
      Junction result = Restrictions.Conjunction ();
      if (range.Lower.HasValue) {
        result = result
          .Add (Restrictions.Disjunction ()
                .Add (Restrictions.IsNull ("End"))
                .Add (Restrictions.Gt ("End", (UpperBound<DateTime>)range.Lower.Value)));
        }
      
      // To constraint
      if (range.Upper.HasValue) {
        result = result
          .Add (Restrictions.Lt ("Begin", range.Upper.Value));
      }
      
      return result;
    }
  }
}
