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

namespace Lemoine.Plugin.ActiveEventCompletedOperation
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
        log.Warn ("Initialize: the configuration is not valid");
        return false;
      }

      m_message = configuration.Message;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        int machineFilterId = configuration.MachineFilterId;
        if (0 != machineFilterId) {
          var machineFilter = ModelDAOHelper.DAOFactory.MachineFilterDAO
            .FindById (machineFilterId);
          if (null == machineFilter) {
            log.Error ($"Initialize: machine filter id {machineFilterId} does not exist");
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
    public IList<Event> GetComingEvents (IProgressResponse progressResponse)
    {
      var events = new List<Event> ();
      return events;
    }

    /// <summary>
    /// Get active events
    /// </summary>
    /// <returns></returns>
    public IList<Event> GetActiveEvents (IProgressResponse progressResponse)
    {
      var events = new List<Event> ();

      // cycle completion
      if (progressResponse.CompletionDateTime.HasValue) {
        var eventSeverity = new EventSeverity (EventSeverityLevel.Info);
        var completionEvent = Event.CreateActiveEvent (m_message, progressResponse.CompletionDateTime.Value, eventSeverity);
        events.Add (completionEvent);
      }

      return events;
    }

  }
}
