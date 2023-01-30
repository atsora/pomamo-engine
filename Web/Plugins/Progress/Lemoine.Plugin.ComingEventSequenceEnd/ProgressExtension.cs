// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Operation;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Extensions.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.ComingEventSequenceEnd
{
  public class ProgressExtension
    : Lemoine.Extensions.MultipleInstanceConfigurableExtension<Configuration>
    , IProgressExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ProgressExtension).FullName);

    IMonitoredMachine m_machine;
    string m_message;

    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <param name="machine"></param>
    /// <returns>the extension is active</returns>
    public bool Initialize (IMonitoredMachine machine)
    {
      m_machine = machine;

      Configuration configuration;
      if (!LoadConfiguration (out configuration)) {
        log.WarnFormat ("Initialize: " +
                        "the configuration is not valid");
        return false;
      }

      m_message = configuration.Message;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = configuration.MachineFilterId;
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

      return true;
    }

    /// <summary>
    /// Get coming events
    /// </summary>
    /// <returns></returns>
    public IList<Event> GetComingEvents (IProgressResponse cycleProgressResponse)
    {
      var events = new List<Event> ();

      if ((null != cycleProgressResponse) && (null != cycleProgressResponse.Operation)) {
        var machineModuleProgress = cycleProgressResponse.MachineModuleProgress;
        if (machineModuleProgress is null) {
          log.Error ($"GetComingEvents: machine module progress is null (deactivated) => do not return any event");
        }
        else {
          foreach (var byMachineModule in machineModuleProgress.Values) {
            if (byMachineModule.CurrentSequenceBeginDateTime.HasValue
              && byMachineModule.CurrentSequenceStandardTime.HasValue) {
              var estimatedDateTime = byMachineModule.CurrentSequenceBeginDateTime.Value
                .Add (byMachineModule.CurrentSequenceStandardTime.Value);
              var possiblyLate = false;
              if (byMachineModule.CurrentSequenceElapsed.HasValue) {
                if (byMachineModule.CurrentSequenceStandardTime.Value < byMachineModule.CurrentSequenceElapsed.Value) {
                  possiblyLate = true;
                }
              }
              var e = Event.CreateComingEvent (m_message, estimatedDateTime,
                new EventSeverity (EventSeverityLevel.Info),
                true, possiblyLate);
              events.Add (e);
            }
          }
        }
      }

      return events;
    }

    /// <summary>
    /// Get active events
    /// </summary>
    /// <returns></returns>
    public IList<Event> GetActiveEvents (IProgressResponse cycleProgressResponse)
    {
      var events = new List<Event> ();
      return events;
    }

  }
}
