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
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModuleActivityDAO">IMachineModuleActivityDAO</see>
  /// </summary>
  public class MachineModuleActivityDAO
    : VersionableByMachineModuleNHibernateDAO<MachineModuleActivity, IMachineModuleActivity, int>
    , IMachineModuleActivityDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineModuleActivityDAO).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineModuleActivityDAO ()
      : base ("MachineModule")
    { }
    
    /// <summary>
    /// Get the last machine module activity for the specified machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public IMachineModuleActivity GetLast (IMachineModule machineModule)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModuleActivity> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .AddOrder (Order.Desc ("Begin"))
        .SetMaxResults (1)
        .UniqueResult<IMachineModuleActivity> ();
    }
    
    /// <summary>
    /// Find all machine module activities in a specified UTC date/time range
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public IList<IMachineModuleActivity> FindAllInUtcRange (IMachineModule machineModule,
                                                            UtcDateTimeRange range)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModuleActivity> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        .List<IMachineModuleActivity> ();
    }
    
    /// <summary>
    /// Get all the activities in a specified date/time range
    /// 
    /// Order the result
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="maxNumber">Max number of activities to retrieve</param>
    /// <returns></returns>
    public IList<IAutoSequencePeriod> FindAllAutoSequencePeriodsBetween (IMachineModule machineModule,
                                                                         UtcDateTimeRange range,
                                                                         int maxNumber)
    {
      log.DebugFormat ("FindAllBetween: " +
                       "machineModule={0} range={1} maxNumber={2}",
                       machineModule, range, maxNumber);
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineModuleActivity> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (InUtcRange (range))
        .AddOrder (Order.Asc ("Begin"))
        .SetMaxResults (maxNumber)
        .List<IAutoSequencePeriod> ();
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
          .Add (Restrictions.Gt ("End", range.Lower.Value));
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
