// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Core.Log;
using System.ComponentModel;
using Lemoine.Model;

namespace Lemoine.Plugin.ActivityIsProduction
{
  public sealed class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

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
    /// Minimum activity time that triggers a production
    /// 
    /// Default: null (no minimum activity time)
    /// </summary>
    [PluginConf ("DurationPicker", "Minimum activity time", Description = "Optionally a minimum required time of activity that triggers a production")]
    public TimeSpan? MinimumActivityDuration
    {
      get; set;
    } = null;

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
    [PluginConf ("DurationPicker", "Maximum duration for short periods", Description = "Optionally a maximum duration for the short periods", Parameters = "0:00:01", Optional = true)]
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

    /// <summary>
    /// Constructor
    /// </summary>
    public Configuration ()
    {
    }

    /// <summary>
    /// Were the ignore short periods settings set?
    /// </summary>
    /// <returns></returns>
    public bool IsIgnoreShortActive () => (null != this.IgnoreShortMachineModeIds) && this.IgnoreShortMachineModeIds.Any ()
        && (null != this.IgnoreShortMaximumDuration) && (0 < this.IgnoreShortMaximumDuration.Value.Ticks);

    /// <summary>
    /// Does a fact corresponds to a short period
    /// </summary>
    public bool IsShort (DateTime start, DateTime end, IMachineMode machineMode, IEnumerable<IMachineMode> shortMachineModes)
    {
      if (!IsIgnoreShortActive ()) {
        return false;
      }

      if (shortMachineModes is null || !shortMachineModes.Any ()) {
        return false;
      }

      if (end < start) {
        log.Error ($"IsShort: end {end} is before start {start}");
        return false;
      }
      var duration = end - start;

      if (this.IgnoreShortMaximumDuration <= duration) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsShort: {start}-{end} is longer than {this.IgnoreShortMaximumDuration}, return false");
        }
        return false;
      }

      return shortMachineModes.Any (m => IsSubMachineMode (m, machineMode));
    }

    bool IsSubMachineMode (IMachineMode ancestor, IMachineMode descendant)
    {
      return Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (ancestor, descendant));
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

    /// <summary>
    /// <see cref="Extensions.Configuration.Implementation.ConfigurationWithMachineFilter"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired () => false;
  }
}
