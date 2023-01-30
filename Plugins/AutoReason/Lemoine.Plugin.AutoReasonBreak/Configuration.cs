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

namespace Lemoine.Plugin.AutoReasonBreak
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Break machine observation state id
    /// </summary>
    [PluginConf ("MachineObservationState", "Machine state", Description = "a machine state", Multiple = false, Optional = false)]
    public int MachineObservationStateId { get; set; }

    /// <summary>
    /// Margin before and after a break
    /// 
    /// Default: 0s
    /// </summary>
    [PluginConf ("DurationPicker", "Margin", Description = "optionally a margin around the break", Parameters="0:00:00", Multiple = false, Optional = false)]
    public TimeSpan Margin { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
      this.ReasonId = (int)Lemoine.Model.ReasonId.Break;
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

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AutoReasonBreak.IsValidConfiguration")) {
          if (this.MachineObservationStateId <= 0) {
            string message = string.Format ("invalid machine observation state id {0}: not strictly positive", this.MachineObservationStateId);
            log.ErrorFormat ("IsValid: {0}", message);
            errorList.Add (message);
          }
          else {
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
