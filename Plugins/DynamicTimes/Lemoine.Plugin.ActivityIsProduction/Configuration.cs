// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.ComponentModel;

namespace Lemoine.Plugin.ActivityIsProduction
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// [Optional] Name prefix
    /// </summary>
    [PluginConf ("Text", "Dynamic time prefix", Description = "Prefix to add to the dynamic time names")]
    public string NamePrefix
    {
      get; set;
    }

    /// <summary>
    /// NorManualNorAuto: include the periods where the auto/manual status could not be determined
    /// </summary>
    [PluginConf ("Bool", "Unknown auto/manual status", Description = "Include the periods where the auto/manual status could not be determined. Default: true")]
    [DefaultValue (true)]
    public bool NorManualNorAuto
    {
      get; set;
    } = true;

    /// <summary>
    /// Manual: include the manual periods
    /// </summary>
    [PluginConf ("Bool", "Include manual", Description = "Include the manual periods. Default: false")]
    [DefaultValue (false)]
    public bool Manual
    {
      get; set;
    } = false;

    /// <summary>
    /// Optionally a cache time out
    /// </summary>
    [PluginConf ("DurationPicker", "Cache time out", Description = "Optionally a cache time out", Parameters = "00:00:01", Optional = true)]
    public TimeSpan? CacheTimeOut
    {
      get; set;
    }

    /// <summary>
    /// Maximum time before giving up for LastProductionEnd
    /// 
    /// Set it to 3 days for example to limit some side performance effects
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum delay", Description = "Optionally a maximum delay for LastProductionEnd dynamic time", Parameters = "1.00:00:01", Optional = true)]
    public TimeSpan? Maximum
    {
      get; set;
    }

    /// <summary>
    /// List of machine modes whose short periods should be ignored
    /// 
    /// If set, <see cref="IgnoreShortMaximumDuration"/> must be set as well
    /// </summary>
    [PluginConf ("MachineMode", "Ignore short machine modes", Description = "List of machine modes whose short periods should be ignored", Multiple = true, Optional = true)]
    public IEnumerable<int> IgnoreShortMachineModeIds
    {
      get; set;
    } = null;

    /// <summary>
    /// Maximum duration for the short periods
    /// 
    /// <see cref="IgnoreShortMachineModeIds"/> must be set as well
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum duration for short periods", Description = "Optionally a maximum delay for LastProductionEnd dynamic time", Parameters = "0:00:01", Optional = true)]
    [DefaultValue (null)]
    public TimeSpan? IgnoreShortMaximumDuration
    {
      get; set;
    } = null;

    /// <summary>
    /// Optional list of additional specific machine modes to include (in addition to the standard settings). Not recursive
    /// </summary>
    [PluginConf ("MachineMode", "Include additional machine modes", Description = "Optional list of additional machine modes to include (not recursive)", Multiple = true, Optional = true)]
    public IEnumerable<int> IncludeMachineModeIds
    {
      get; set;
    } = null;

    /// <summary>
    /// Optional list of specific machine modes to exclude (from the standard settings). Not recursive
    /// </summary>
    [PluginConf ("MachineMode", "Exclude machine modes", Description = "Optional list of specific machine modes to exclude (not recursive)", Multiple = true, Optional = true)]
    public IEnumerable<int> ExcludeMachineModeIds
    {
      get; set;
    } = null;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      bool result = base.IsValid (out errors);

      using (var session = ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
        if ((null != this.IgnoreShortMachineModeIds) && this.IgnoreShortMachineModeIds.Any ()) {
          foreach (var ignoreShortMachineModeId in this.IgnoreShortMachineModeIds) {
            var ignoreShortMachineMode = ModelDAO.ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (ignoreShortMachineModeId);
            if (null == ignoreShortMachineMode) {
              log.Error ($"IsValid: machine mode with id {ignoreShortMachineModeId} does not exist");
              errors = errors.Concat (new List<string> { $"Unknown machine mode with id {ignoreShortMachineModeId}" });
              result = false;
            }
          }
        }
        if ((null != this.IncludeMachineModeIds) && this.IncludeMachineModeIds.Any ()) {
          foreach (var includeMachineModeId in this.IncludeMachineModeIds) {
            var includeMachineMode = ModelDAO.ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (includeMachineModeId);
            if (null == includeMachineMode) {
              log.Error ($"IsValid: include machine mode with id {includeMachineModeId} does not exist");
              errors = errors.Concat (new List<string> { $"Unknown machine mode with id {includeMachineModeId}" });
              result = false;
            }
          }
        }
        if ((null != this.ExcludeMachineModeIds) && this.ExcludeMachineModeIds.Any ()) {
          foreach (var excludeMachineModeId in this.ExcludeMachineModeIds) {
            var excludeMachineMode = ModelDAO.ModelDAOHelper.DAOFactory.MachineModeDAO
              .FindById (excludeMachineModeId);
            if (null == excludeMachineMode) {
              log.Error ($"IsValid: exclude machine mode with id {excludeMachineModeId} does not exist");
              errors = errors.Concat (new List<string> { $"Unknown machine mode with id {excludeMachineModeId}" });
              result = false;
            }
          }
        }
      }

      return result;
    }

    #endregion // Constructors

    /// <summary>
    /// <see cref="Extensions.Configuration.Implementation.ConfigurationWithMachineFilter"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
  }
}
