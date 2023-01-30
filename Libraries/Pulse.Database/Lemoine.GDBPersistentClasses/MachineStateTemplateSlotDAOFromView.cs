// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Info;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate.Criterion;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineStateTemplateSlotDAO">IMachineStateTemplateSlotDAO</see>
  /// using the machinestatetemplateslot view
  /// </summary>
  public class MachineStateTemplateSlotDAOFromView
    : MachineStateTemplateSlotDAO
    , IMachineStateTemplateSlotDAO
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineStateTemplateSlotDAOFromView).FullName);
    
    #region IMachineStateTemplateSlotDAO implementation
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public override IMachineStateTemplateSlot FindAt(IMachine machine, DateTime dateTime)
    {
      Debug.Assert (null != machine);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
        // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
        .Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), dateTime, "@>"))
        .UniqueResult<IMachineStateTemplateSlot> ();
    }
    
    /// <summary>
    /// IMachineStateTemplateSlotDAO implementation
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    public override IList<IMachineStateTemplateSlot> FindOverlapsRange(IMachine machine, UtcDateTimeRange range)
    {
      Debug.Assert (null != machine);
      
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&"))
        .AddOrder (Order.Asc ("DateTimeRange"))
        .List<IMachineStateTemplateSlot> ();
    }

    /// <summary>
    /// Find all the observation state slots (on different machines)
    /// at a specific date/time with the specified machine state template
    /// 
    /// They are order by ascending begin date/time
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="at"></param>
    /// <returns></returns>
    public override IEnumerable<IMachineStateTemplateSlot> FindAt (IMachineStateTemplate machineStateTemplate, DateTime at)
    {
      Debug.Assert (DateTimeKind.Utc == at.Kind);
      
      NHibernate.ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateSlot> ();
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate", machineStateTemplate));
      }
      // Note: new SimpleExpression ("DateTimeRange", dateTime, "@>") does not work because it compares object of different types
      // The returned error is: Type mismatch in NHibernate.Criterion.SimpleExpression: DateTimeRange expected type Lemoine.Model.UtcDateTimeRange, actual type System.DateTime
      criteria.Add (new SimpleTypedExpression ("DateTimeRange", new Lemoine.NHibernateTypes.UTCDateTimeFullType (), at, "@>"));
      criteria.AddOrder (Order.Asc ("DateTimeRange"));
      return criteria.List<IMachineStateTemplateSlot> ();
    }

    /// <summary>
    /// Find all the machine state template slots that match a machine state template in a specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="machineStateTemplate"></param>
    /// <returns></returns>
    public override IEnumerable<IMachineStateTemplateSlot> FindOverlapsRangeMatchingMachineStateTemplate (IMachine machine,
                                                                                                          UtcDateTimeRange range,
                                                                                                          IMachineStateTemplate machineStateTemplate)
    {
      Debug.Assert (null != machine);
      
      NHibernate.ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineStateTemplateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .Add (new SimpleExpression ("DateTimeRange", range, "&&"));
      if (null == machineStateTemplate) {
        criteria.Add (Restrictions.IsNull ("MachineStateTemplate"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineStateTemplate", machineStateTemplate));
      }
      return criteria.AddOrder (Order.Asc ("DateTimeRange"))
        .List<IMachineStateTemplateSlot> ();
    }
    #endregion // IMachineStateTemplateSlotDAO implementation
  }
}
