// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Plugin.AutoReasonWeekend
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Weekend machine observation state id (default is weekend)
    /// </summary>
    [PluginConf ("MachineObservationState", "Machine state", Description = "a machine state", Multiple = false, Optional = false)]
    public int MachineObservationStateId { get; set; }

    /// <summary>
    /// Margin before the end of the weekend in which the production can restart
    /// 
    /// Default: 0s
    /// </summary>
    [PluginConf ("DurationPicker", "Margin", Description = "area where the production can start before the end of the weekend", Parameters = "1:00:00", Multiple = false, Optional = false)]
    public TimeSpan Margin { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }
    #endregion // Constructors

    #region IConfiguration implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.IConfiguration"/>
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      var baseResult = base.IsValid (out errors);
      var errorList = new List<string> ();

      // Test if the specified MachineObservationState (weekend) is ok
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AutoReasonWeekEnd.IsValidConfiguration")) {
          if (this.MachineObservationStateId <= 0) {
            string message = string.Format ("invalid machine observation state id {0}: not strictly positive", this.ReasonId);
            log.ErrorFormat ("IsValid: {0}", message);
            errorList.Add (message);
          } else {
            var machineObservationState = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
              .FindById (this.MachineObservationStateId);
            if (null == machineObservationState) {
              string message = string.Format ("invalid machine observation state id {0}: unknown machine observation state", this.MachineObservationStateId);
              log.ErrorFormat ("IsValid: {0}", message);
              errorList.Add (message);
            }
          }
        }
      }

      errors = errors.Concat (errorList);
      return baseResult && !errors.Any ();
    }
    #endregion // IConfiguration implementation
  }
}
