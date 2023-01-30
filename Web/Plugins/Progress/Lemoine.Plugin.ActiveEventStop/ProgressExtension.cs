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

namespace Lemoine.Plugin.ActiveEventStop
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
      return events;
    }

    /// <summary>
    /// Get active events
    /// </summary>
    /// <returns></returns>
    public IList<Event> GetActiveEvents (IProgressResponse cycleProgressResponse)
    {
      var events = new List<Event> ();

      var machineModuleProgress = cycleProgressResponse.MachineModuleProgress;
      if (machineModuleProgress is null) {
        log.Error ($"GetActiveEvents: machine module progress is null (deactivated) => do not return any event");
      }
      else {
        bool activeStop = cycleProgressResponse.MachineModuleProgress.Values
          .Any (mmv => (null != mmv.CurrentSequence) && (SequenceKind.Stop == mmv.CurrentSequence.Kind));
        if (activeStop) {
          var stopSequences = cycleProgressResponse.MachineModuleProgress.Values
            .Where (mmv => (null != mmv.CurrentSequence) && (SequenceKind.Stop == mmv.CurrentSequence.Kind));
          var stopDateTime = stopSequences
            .Where (mmv => mmv.CurrentSequenceBeginDateTime.HasValue)
            .Min (mmv => mmv.CurrentSequenceBeginDateTime.Value);
          var eventSeverity = new EventSeverity (EventSeverityLevel.Error);
          var activeStopEvent = Event.CreateActiveEvent (m_message, stopDateTime, eventSeverity);
          var longText = string.Join (", ", stopSequences
            .Where (x => !string.IsNullOrWhiteSpace (x.CurrentSequence.Name))
            .Select (x => x.CurrentSequence.Name));
          if (!string.IsNullOrWhiteSpace (longText)) {
            activeStopEvent.LongText = longText;
          }
          events.Add (activeStopEvent);
        }
      }

      return events;
    }

  }
}
