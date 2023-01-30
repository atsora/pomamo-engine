// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Threading.Tasks;
using Pulse.Extensions.Web;

namespace Lemoine.Plugin.ActiveEventCncValue
{
  public class ProductionMachiningStatusExtension
     : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProductionMachiningStatusExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (ProductionMachiningStatusExtension).FullName);

    IMonitoredMachine m_machine;
    Configuration m_configuration;
    string m_message;
    IMachineModule m_machineModule;
    IField m_field;
    string m_cncValue;
    DateTime? m_previousDateTime;
    TimeSpan? m_minimumDelay;
    readonly ScriptOptions m_scriptOptions;
    Func<object, bool> m_lambda;

    public ProductionMachiningStatusExtension ()
    {
      m_scriptOptions = ScriptOptions.Default
        .AddReferences (new System.Reflection.Assembly[] {
          typeof (IProductionMachiningStatusExtension).Assembly,
          typeof (Convert).Assembly
        })
        .AddImports (new string[] { "System" });
    }

    public IList<Event> GetActiveEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse)
    {
      var events = new List<Event> ();

      log.Debug ($"GetActiveEvents: EventCncValue DisplayMachine={m_machine.Name} Module={m_machineModule.Name}, field={m_field}");
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var currentCncValue = ModelDAOHelper.DAOFactory.CurrentCncValueDAO
          .Find (m_machineModule, m_field);


        if (null != currentCncValue && m_cncValue.Equals (currentCncValue.String)) {
          log.Debug ($"GetActiveEvents: EventCncValue CncValue={currentCncValue.String}, Module={m_machineModule.Name}");
          // check value match lambda condition
          if (!String.IsNullOrEmpty (m_configuration.LambdaCondition) && !m_lambda.Invoke (currentCncValue.Value)) {
            return events;
          }

          // check duration
          if (!m_previousDateTime.HasValue) {
            m_previousDateTime = currentCncValue.DateTime;
          }
          var lastCncValue = ModelDAOHelper.DAOFactory.CncValueDAO
            .FindFirstOverlapsRange (m_machineModule, m_field, new UtcDateTimeRange ("(,)"), 1, true)
            .FirstOrDefault ();
          DateTime startDateTime;
          if ((null != lastCncValue) && (1 == lastCncValue.Int)) {
            startDateTime = lastCncValue.DateTimeRange.Lower.Value;
          }
          else if (m_previousDateTime.HasValue) {
            startDateTime = m_previousDateTime.Value;
          }
          else {
            startDateTime = currentCncValue.DateTime;
          }

          // m_minimumDelay <= duration
          var duration = currentCncValue.DateTime.Subtract (startDateTime);
          log.Debug ($"GetActiveEvents: EventCncValue duration={duration}, Module={m_machineModule.Name}");

          if (m_configuration.MinimumDelay.HasValue && (m_minimumDelay <= duration)) {
            log.Debug ($"GetActiveEvents: CreateActiveEvent, Module={m_machineModule.Name}");
            var activeEvent = Event.CreateActiveEvent (m_message, startDateTime, new EventSeverity (EventSeverityLevel.Error));
            events.Add (activeEvent);
          }
          else {
            m_previousDateTime = null;
          }
        }
        else {
          m_previousDateTime = null;
        }
      }
      return events;
    }

    public IList<Event> GetComingEvents (Lemoine.Extensions.Business.IPartProductionDataCurrentShift partProductionCurrentShiftResponse)
    {
      return new List<Event> ();
    }

    public bool Initialize (IMonitoredMachine machine)
    {
      m_machine = machine;

      if (!LoadConfiguration (out m_configuration)) {
        log.WarnFormat ("Initialize: " +
                        "the configuration is not valid");
        return false;
      }

      log.Debug ($"Initialize: Machine={machine.Id}");
      m_message = m_configuration.Message;
      m_minimumDelay = m_configuration.MinimumDelay;
      m_cncValue = m_configuration.CncValue;

      if (!String.IsNullOrEmpty (m_configuration.LambdaCondition)) {
        try {
          m_lambda = Task.Run (() => CSharpScript.EvaluateAsync<Func<object, bool>> (m_configuration.LambdaCondition, m_scriptOptions)).Result;
        }
        catch (Exception ex) {
          log.Error ($"InitializeAdditionalConfigurations: invalid expression {m_configuration.LambdaCondition}", ex);
          return false;
        }
      }

      log.Debug ($"Initialize: Check machine filter id={m_configuration.MachineFilterId}");
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = m_configuration.MachineFilterId;
        if (0 != machineFilterId) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (machineFilterId);
          if (null == machineFilter) {
            log.ErrorFormat ("Initialize: " +
                             "machine filter id {0} does not exist",
                             machineFilterId);
            return false;
          }
          // Note: machineFilter.IsMatch requires it is done in the same session
          if (!machineFilter.IsMatch (machine)) {
            return false;
          }
        }
      }
      log.Debug ($"Initialize: machine filter is ok. Machine={machine.Id}");

      m_field = ModelDAOHelper.DAOFactory.FieldDAO
        .FindById (m_configuration.FieldId);
      if (null == m_field) {
        log.Error ($"Initialize: unknown field id {m_configuration.FieldId}");
        return false;
      }

      if (0 != m_configuration.MachineModuleId) {
        m_machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
          .FindById (m_configuration.MachineModuleId);
        if (null == m_machineModule) {
          log.Error ($"InitializeAdditionalConfigurations: machine module id {m_configuration.MachineModuleId} does not exist");
          return false;
        }
      }
      return true;
    }
  }
}
