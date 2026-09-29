// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICurrentCncAlarmDAO">ICurrentCncAlarmDAO</see>
  /// </summary>
  public class CurrentCncAlarmDAO :
    VersionableByMachineModuleNHibernateDAO<CurrentCncAlarm, ICurrentCncAlarm, int>,
    ICurrentCncAlarmDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public CurrentCncAlarmDAO() : base("MachineModule") {}
    
    /// <summary>
    /// Find all the ICurrentCncAlarm for the specified machine module
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<ICurrentCncAlarm> FindByMachineModule(IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<CurrentCncAlarm>()
        .Add(Restrictions.Eq("MachineModule.Id", machineModule.Id))
        .List<ICurrentCncAlarm>();
    }
    
    /// <summary>
    /// Get all ICurrentCncAlarm of a particular cnc, after a specific date
    /// The severity is loaded
    /// </summary>
    /// <param name="cncType">null or empty is all cnc</param>
    /// <returns></returns>
    public IList<ICurrentCncAlarm> FindByCncWithSeverity(string cncType)
    {
      var criteria = NHibernateHelper.GetCurrentSession()
        .CreateCriteria<CurrentCncAlarm>()
        .Fetch (SelectMode.Fetch, "MachineModule")
        .Fetch (SelectMode.Fetch, "MachineModule.MonitoredMachine");
      
      // Restriction of the cnc type?
      if (!string.IsNullOrEmpty(cncType)) {
        criteria = criteria.Add(Restrictions.Eq("CncInfo", cncType));
      }

      var alarms = criteria.List<ICurrentCncAlarm>();
      
      // Initialize the severities
      foreach (var alarm in alarms) {
        ModelDAOHelper.DAOFactory.Initialize(alarm.Severity);
      }

      return alarms;
    }
    
    /// <summary>
    /// Find all the ICurrentCncAlarm for the specified machine module
    /// The severity and machine modules are loaded
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<ICurrentCncAlarm> FindByMachineModuleWithSeverity(IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      var alarms = NHibernateHelper.GetCurrentSession()
        .CreateCriteria<CurrentCncAlarm>()
        .Add(Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .Fetch (SelectMode.Fetch, "MachineModule")
        .List<ICurrentCncAlarm>();
      
      // Initialize the severities
      foreach (var alarm in alarms) {
        ModelDAOHelper.DAOFactory.Initialize(alarm.Severity);
      }

      return alarms;
    }

    /// <summary>
    /// <see cref="ICurrentCncAlarmDAO"/>
    ///
    /// A projection is used so that the dynamic column cncalarmseverityid is not requested
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<ICurrentCncAlarm> FindByMachineModuleWithoutSeverity (IMachineModule machineModule)
    {
      Debug.Assert (null != machineModule);

      var rows = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CurrentCncAlarm> ()
        .Add (Restrictions.Eq ("MachineModule.Id", machineModule.Id))
        .SetProjection (Projections.ProjectionList ()
          .Add (Projections.Id ())
          .Add (Projections.Property ("Version"))
          .Add (Projections.Property ("DateTime"))
          .Add (Projections.Property ("CncInfo"))
          .Add (Projections.Property ("CncSubInfo"))
          .Add (Projections.Property ("Type"))
          .Add (Projections.Property ("Number"))
          .Add (Projections.Property ("Message"))
          .Add (Projections.Property ("Properties"))
          .Add (Projections.Property ("Display")))
        .List<object[]> ();

      return rows
        .Select (r => (ICurrentCncAlarm)new CurrentCncAlarm ((int)r[0], (int)r[1], machineModule, (DateTime)r[2],
                                                             (string)r[3], (string)r[4], (string)r[5], (string)r[6],
                                                             (string)r[7], (IDictionary<string, object>)r[8], (string)r[9]))
        .ToList ();
    }
  }
}
