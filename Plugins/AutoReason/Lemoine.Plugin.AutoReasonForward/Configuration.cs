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
using System.ComponentModel;

namespace Lemoine.Plugin.AutoReasonForward
{
  public sealed class Configuration
    : Lemoine.Extensions.AutoReason.AutoReasonConfiguration
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Machine where to forward the auto-reason
    /// </summary>
    [PluginConf ("Reason", "Source reasons", Description = "the reasons that trigger the auto-reason on the source machine. If empty, the target reason is used instead", Multiple = true, Optional = true)]
    public IEnumerable<int> SourceReasonIds { get; set; } = null;

    /// <summary>
    /// Machine where to forward the auto-reason
    /// </summary>
    [PluginConf ("Machine", "Target machine", Description = "the target machine where to forward the auto-reason", Multiple = false, Optional = true)]
    public int TargetMachineId { get; set; }

    /// <summary>
    /// Dynamic end of the created auto-reason.
    /// 
    /// If it is equal to "Source" (default), then the dynamic end of the source reason machine association is considered
    /// </summary>
    [PluginConf ("Text", "Dynamic end", Description = "Select the dynamic end of the created auto-reason. If it is Source (default), take the dynamic end of the source reason machine association", Parameters = "Source", Multiple = false, Optional = false)]
    [DefaultValue ("Source")]
    public String DynamicEnd { get; set; } = "Source";

    /// <summary>
    /// Optional dynamic start of the created auto-reason.
    /// 
    /// If set, compute a dynamic start time based on the start time of the source reason to extend the period
    /// </summary>
    [PluginConf ("Text", "Dynamic start", Description = "Optional dynamic start of the created auto-reason. Default (empty): do not use it, consider the start date/time of the source reason machine association", Parameters = "Source", Multiple = false, Optional = true)]
    [DefaultValue ("")]
    public String DynamicStart { get; set; } = "";

    /// <summary>
    /// Consider the reason machine association start date/time only,
    /// not the full date/time range.
    /// 
    /// Useful when the DynamicEnd option is used.
    /// 
    /// If DynamicEnd is an empty string, then consider StartOnly is false.
    /// </summary>
    [PluginConf ("Bool", "Start only", Description = "Consider the association start date/time only, not the full date/time range. If DynamicEnd is an empty string, by default false is considered, else true", Optional = true)]
    [DefaultValue (true)]
    public bool StartOnly { get; set; } = true;
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

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("AutoReasonForward.IsValid")) {
          if (null != this.SourceReasonIds) {
            foreach (var sourceReasonId in this.SourceReasonIds) {
              if (null == ModelDAOHelper.DAOFactory.ReasonDAO
                .FindById (sourceReasonId)) {
                log.Error ($"IsValid: source reason id {sourceReasonId} does not exist");
                errorList.Add ($"Source reason with ID {sourceReasonId} does not exist");
              }
            }
          }
          if ((0 == this.TargetMachineId) && ( (null == this.SourceReasonIds) || !this.SourceReasonIds.Any ())) {
            log.Error ($"IsValid: no TargetMachine and no source reason was set");
            errorList.Add ("Both TargetMachine and SourceReasons not set");
          }
          if (0 != this.TargetMachineId) {
            if (null == ModelDAOHelper.DAOFactory.MachineDAO
                  .FindById (this.TargetMachineId)) {
              log.Error ($"IsValid: Machine {this.TargetMachineId} does not exist");
              errorList.Add ($"Machine with ID {this.TargetMachineId} does not exist");
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
