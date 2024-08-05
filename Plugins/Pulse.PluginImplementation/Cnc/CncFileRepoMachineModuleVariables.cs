// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Cnc;
using Lemoine.Model;
using System.Diagnostics;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Collections.Generic;
using Lemoine.Extensions.Configuration;
using Pulse.Extensions.Configuration.Implementation;
using System.Linq;

namespace Pulse.PluginImplementation.Cnc
{
  /// <summary>
  /// Abstract class to use when the CNC variables keys are drawn from the machinemodule table
  /// </summary>
  /// <typeparam name="TConfiguration"></typeparam>
  public abstract class CncFileRepoMachineModuleVariables<TConfiguration>
    : FilteredByMachineCncFileRepo<TConfiguration>
    , ICncFileRepoExtension
    where TConfiguration : ConfigurationWithMachineFilter, new()
  {
    ILog log = LogManager.GetLogger (typeof (CncFileRepoMachineModuleVariables<TConfiguration>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configurationLoader"></param>
    protected CncFileRepoMachineModuleVariables (IConfigurationLoader<TConfiguration> configurationLoader)
      : base (configurationLoader)
    {
    }

    /// <summary>
    /// Constructor with a default configuration loader
    /// </summary>
    public CncFileRepoMachineModuleVariables ()
      : base ()
    {
    }

    /// <summary>
    /// Return the cnc variables that are set in the database
    /// </summary>
    /// <param name="machineModule"></param>
    /// <returns></returns>
    public override IEnumerable<string> GetCncVariableKeys (IMachineModule machineModule)
    {
      var result = new List<string> ();
      if (!string.IsNullOrEmpty (machineModule.CycleVariable)) {
        result.Add (machineModule.CycleVariable);
      }
      if (!string.IsNullOrEmpty (machineModule.StartCycleVariable)) {
        result.Add (machineModule.StartCycleVariable);
      }
      if (!string.IsNullOrEmpty (machineModule.SequenceVariable)) {
        result.Add (machineModule.SequenceVariable);
      }
      if (!string.IsNullOrEmpty (machineModule.MilestoneVariable)) {
        result.Add (machineModule.MilestoneVariable);
      }
      return result;
    }
  }
}