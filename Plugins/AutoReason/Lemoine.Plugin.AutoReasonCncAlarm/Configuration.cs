// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using System.Linq;

namespace Lemoine.Plugin.AutoReasonCncAlarm
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Filter on focused cnc alarms only
    /// </summary>
    [PluginConf ("Bool", "Focused only", Description = "Keep only the focused cnc or machine alarms", Parameters = "", Multiple = false, Optional = false)]
    public bool FocusedOnly { get; set; }

    /// <summary>
    /// Filter on machine module
    /// 
    /// 0 (default): no filter on machine module is applicable
    /// </summary>
    [PluginConf ("MachineModule", "Machine module filter", Description = "Add a condition on the machine module. If not set (default), no filter applies", Parameters = "", Multiple = false, Optional = true)]
    public int MachineModuleId { get; set; }

    /// <summary>
    /// Filter on cnc alarm severity
    /// 
    /// 0 (default): no filter on cnc alarm severity is applicable
    /// </summary>
    [PluginConf ("CncAlarmSeverity", "Cnc alarm severity filter", Description = "Add a condition on the cnc alarm severity. If not set (default), no filter applies", Parameters = "", Multiple = false, Optional = true)]
    public int CncAlarmSeverityId { get; set; }

    /// <summary>
    /// Message regex filter with the Ignore case and Invariant culture options
    /// 
    /// If empty, do not apply it
    /// </summary>
    [PluginConf ("Text", "Message filter", Description = "Add a regex condition on the message. If empty, all alarms are considered, no filter applies", Parameters = "", Multiple = false, Optional = true)]
    public string MessageRegex { get; set; }

    /// <summary>
    /// Exclude regex filter on the message with the Ignore case and Invariant culture options
    /// 
    /// If empty, do not apply it
    /// </summary>
    [PluginConf ("Text", "Exclude filter on message", Description = "Add a regex condition to exclude some alarms based on the message. If empty, the filter is not applied and all alarms are considered", Parameters = "", Multiple = false, Optional = true)]
    [DefaultValue ("")]
    public string ExcludeRegex { get; set; } = "";

    /// <summary>
    /// Dynamic end of the created auto-reason.
    /// 
    /// Default is "NextProductionStart"
    /// </summary>
    [PluginConf ("Text", "Dynamic end", Description = "Select the dynamic end of the created auto-reason. Default: NextProductionStart", Parameters = "NextProductionStart", Multiple = false, Optional = false)]
    [DefaultValue ("NextProductionStart")]
    public String DynamicEnd { get; set; } = "NextProductionStart";

    /// <summary>
    /// Reason details prefix
    /// </summary>
    [PluginConf ("Text", "Reason details prefix", Description = "Text to add at start of the reason details. If empty (default), do not add anything", Parameters = "", Multiple = false, Optional = true)]
    [DefaultValue ("")]
    public string DetailsPrefix { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration () {}
    #endregion // Constructors

    /// <summary>
    /// <see cref="Lemoine.Extensions.Configuration.IConfiguration"/>
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      IEnumerable<string> baseErrors;
      var result = base.IsValid (out baseErrors);

      IList<string> errorList = new List<string> ();
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginReadOnlyTransaction ("Plugin.AutoReasonCncAlarm.IsValidConfiguration")) {
          if (0 != this.CncAlarmSeverityId) {
            if (this.CncAlarmSeverityId <= 0) {
              var message = $"invalid cnc alarm severity id {this.CncAlarmSeverityId}: not strictly positive";
              log.Error ($"IsValid: {message}");
              errorList.Add (message);
            }
            else {
              var cncAlarmSeverity = ModelDAOHelper.DAOFactory.CncAlarmSeverityDAO
                .FindById (this.CncAlarmSeverityId);
              if (null == cncAlarmSeverity) {
                string message = $"invalid cnc alarm severity id {this.CncAlarmSeverityId}: unknown cnc alarm severity";
                log.Error ($"IsValid: {message}");
                errorList.Add (message);
              }
            }
          }
          if (0 != this.MachineModuleId) {
            if (this.MachineModuleId <= 0) {
              var message = $"invalid machine module id {this.MachineModuleId}: not strictly positive";
              log.Error ($"IsValid: {message}");
              errorList.Add (message);
            }
            else {
              var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
                .FindById (this.MachineModuleId);
              if (null == machineModule) {
                string message = $"invalid machine module id {this.MachineModuleId}: unknown machine module";
                log.Error ($"IsValid: {message}");
                errorList.Add (message);
              }
            }
          }
        }
      }
      result &= (0 == errorList.Count);

      errors = baseErrors.Concat (errorList);
      return result;
    }
  }
}
