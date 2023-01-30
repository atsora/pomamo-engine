// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.Core.SharedData;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IToolLifeDAO">IToolLifeDAO</see>
  /// </summary>
  public class ToolLifeDAO
    : VersionableByMachineModuleNHibernateDAO<ToolLife, IToolLife, int>
    , IToolLifeDAO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public ToolLifeDAO ()
      : base ("MachineModule")
    { }

    /// <summary>
    /// Get all IToolLifes for a specific position in a machine module
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="position"></param>
    /// <returns></returns>
    public IList<IToolLife> FindAllByMachinePosition (IMachineModule machineModule, IToolPosition position)
    {
      Debug.Assert (machineModule != null, "Machine module cannot be null");

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<ToolLife> ()
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        .Add (Restrictions.Eq ("Position", position))
        .List<IToolLife> ();
    }

    /// <summary>
    /// Get all IToolLifes for a specific machine module with an eager fetch of position
    /// </summary>
    /// <param name="machineModule">not null</param>
    /// <returns></returns>
    public IList<IToolLife> FindAllByMachineModule (IMachineModule machineModule)
    {
      Debug.Assert (machineModule != null);

      IQuery query = NHibernateHelper.GetCurrentSession ()
        .GetNamedQuery ("ToolLifeAllByMachineModule");
      query.SetParameter ("MachineModuleId", machineModule.Id);
      return query.List<IToolLife> ();

      /*
      return NHibernateHelper.GetCurrentSession()
        .CreateCriteria<ToolLife>()
        //.CreateAlias ("Position", "p")
        .Add (Restrictions.Eq ("MachineModule", machineModule))
        //.Add (Restrictions.Eq ("p.MachineModule.Id", machineModule.Id))
        //.Fetch (SelectMode.Fetch, "Position")
        .List<IToolLife>();
      // Note: SetFetchMode ("Position", ...) or CreateAlias ("Position", ...) do not work. Not sure why yet
       */
    }

    /// <summary>
    /// Get all IToolLife for a specific machine with an eager fetch of IToolPosition
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IEnumerable<IToolLife> FindAllByMachine (IMonitoredMachine machine)
    {
      Debug.Assert (machine != null);

      IEnumerable<IToolLife> result = new List<IToolLife> ();

      foreach (var machineModule in machine.MachineModules) {
        result = result.Union (FindAllByMachineModule (machineModule));
      }

      return result;
    }

    /// <summary>
    /// Find all monitored machines with an expired or 'in warning' tool
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindMachinesWithExpiredOrInWarningTools ()
    {
      var toolLifeDetachedCriteria = DetachedCriteria.For<ToolLife> ("tl")
        .SetProjection (Projections.Property ("tl.MachineModule"))
        .Add (Restrictions.EqProperty ("tl.MachineModule.Id", "machineModule.Id"))
        .Add (Restrictions.Disjunction ()
          .Add (GetInWarningCriterion ())
          .Add (GetIsLimitCriterion ()));
      var machineModuleDetachedCriteria = DetachedCriteria.For<MachineModule> ("machineModule")
        .SetProjection (Projections.Property ("machineModule.MonitoredMachine"))
        .Add (Restrictions.EqProperty ("machineModule.MonitoredMachine.Id", "machine.Id"))
        .Add (Subqueries.Exists (toolLifeDetachedCriteria));
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ("machine")
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Add (Subqueries.Exists (machineModuleDetachedCriteria))
        .List<IMonitoredMachine> ();
    }

    /// <summary>
    /// Find all monitored machines with an potentially expired tools
    /// </summary>
    /// <returns></returns>
    public IList<IMonitoredMachine> FindMachinesWithPotentiallyExpiredTools (TimeSpan maxRemainingDuration)
    {
      var toolLifeDetachedCriteria = DetachedCriteria.For<ToolLife> ("tl")
        .SetProjection (Projections.Property ("tl.MachineModule"))
        .Add (Restrictions.EqProperty ("tl.MachineModule.Id", "machineModule.Id"))
        .Add (Restrictions.Disjunction ()
          .Add (GetInWarningCriterion ())
          .Add (GetIsLimitCriterion ())
          .Add (GetIsUnitNumberOfCriterion ())
          .Add (GetLeDurationCriterion (maxRemainingDuration)));
      var machineModuleDetachedCriteria = DetachedCriteria.For<MachineModule> ("machineModule")
        .SetProjection (Projections.Property ("machineModule.MonitoredMachine"))
        .Add (Restrictions.EqProperty ("machineModule.MonitoredMachine.Id", "machine.Id"))
        .Add (Subqueries.Exists (toolLifeDetachedCriteria));
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MonitoredMachine> ("machine")
        .Add (Restrictions.Eq ("MonitoringType.Id", (int)MachineMonitoringTypeId.Monitored))
        .Add (Subqueries.Exists (machineModuleDetachedCriteria))
        .List<IMonitoredMachine> ();
    }

    ICriterion GetIsLimitCriterion (string property)
    {
      /* For information, IsLimitReached is:
      return toolLife.Limit.HasValue
        && toolLife.Limit.Value > 0
        && ((toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Up
             && toolLife.Value >= toolLife.Limit.Value)
            || (toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Down
                && toolLife.Value <= 0));
         and IsWarningReached is:
      return toolLife.Warning.HasValue
  && toolLife.Warning.Value > 0
  && ((toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Up
        && toolLife.Value >= toolLife.Warning.Value)
      || (toolLife.Direction == Lemoine.Core.SharedData.ToolLifeDirection.Down
          && toolLife.Value <= toolLife.Warning.Value));
      */
      return Restrictions.Conjunction ()
        .Add (Restrictions.IsNotNull (property))
        .Add (Restrictions.Gt (property, 0.0))
        .Add (Restrictions.Disjunction ()
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.Eq ("Direction", ToolLifeDirection.Up))
            .Add (Restrictions.GeProperty ("Value", property)))
          .Add (Restrictions.Conjunction ()
            .Add (Restrictions.Eq ("Direction", ToolLifeDirection.Down))
            .Add (Restrictions.LeProperty ("Value", property))));
    }
    ICriterion GetInWarningCriterion ()
    {
      return GetIsLimitCriterion ("Warning");
    }
    ICriterion GetIsLimitCriterion ()
    {
      return GetIsLimitCriterion ("Limit");
    }
    ICriterion GetIsUnitNumberOfCriterion ()
    {
      return Restrictions.Disjunction ()
        .Add (Restrictions.Eq ("Unit.Id", (int)UnitId.NumberOfCycles))
        .Add (Restrictions.Eq ("Unit.Id", (int)UnitId.NumberOfParts))
        .Add (Restrictions.Eq ("Unit.Id", (int)UnitId.ToolNumberOfTimes));
    }
    ICriterion GetLeDurationCriterion (TimeSpan v)
    {
      return Restrictions.Disjunction ()
        .Add (Restrictions.Eq ("Direction", ToolLifeDirection.Up)) // TODO: For the moment, use in the future the virtual column that will return the remaining time in seconds to be more restrictive
        .Add (Restrictions.Conjunction ()
          .Add (Restrictions.Eq ("Direction", ToolLifeDirection.Down))
          .Add (Restrictions.Eq ("Unit.Id", (int)UnitId.DurationHours))
          .Add (Restrictions.Le ("Value", v.TotalHours)))
        .Add (Restrictions.Conjunction ()
          .Add (Restrictions.Eq ("Direction", ToolLifeDirection.Down))
          .Add (Restrictions.Eq ("Unit.Id", (int)UnitId.DurationMinutes))
          .Add (Restrictions.Le ("Value", v.TotalMinutes)))
        .Add (Restrictions.Conjunction ()
          .Add (Restrictions.Eq ("Direction", ToolLifeDirection.Down))
          .Add (Restrictions.Eq ("Unit.Id", (int)UnitId.DurationSeconds))
          .Add (Restrictions.Le ("Value", v.TotalSeconds)));
    }
  }
}
