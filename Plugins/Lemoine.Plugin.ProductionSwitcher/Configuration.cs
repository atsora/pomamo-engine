// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.ComponentModel;
using Lemoine.Extensions;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System.Linq;

namespace Lemoine.Plugin.ProductionSwitcher
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// % of the cycle duration that is necessary to trigger the plugin
    /// </summary>
    [PluginConf ("Int", "Cycle duration limit %", Description = "the % limit of a cycle duration that is necessary to trigger a production")]
    public int CycleDurationPercentageTrigger
    {
      get; set;
    }

    /// <summary>
    /// % of the between cycles duration that is necessary to trigger the plugin
    /// </summary>
    [PluginConf ("Int", "Between cycles limit %", Description = "the % limit of a between cycles duration that is necessary to trigger a production")]
    public int BetweenCyclesDurationPercentageTrigger
    {
      get; set;
    }

    /// <summary>
    /// List of Set-up machine state template Ids
    /// </summary>
    [PluginConf ("MachineStateTemplate", "Set-up", Description = "the list of machine state templates that correspond to a set-up. If none is selected, all apply", Optional = false, Multiple = true)]
    public IList<int> SetupMachineStateTemplateIds
    {
      get; set;
    } = new List<int> ();

    /// <summary>
    /// Production machine state template ID
    /// </summary>
    [PluginConf ("MachineStateTemplate", "Production", Description = "the machine state template that corresponds to the production", Optional = false, Multiple = false)]
    public int ProductionMachineStateTemplateId
    {
      get; set;
    }

    /// <summary>
    /// Minimum number of consecutive good cycles to trigger a switch to production
    ///
    /// 1 (default): any good cycle triggers the switch at once
    ///
    /// The switch is applied from the begin of the first good cycle of the serie,
    /// like the NextProductionStart dynamic time of the NGoodCyclesIsProduction plugin
    /// </summary>
    [PluginConf ("Int", "Number of good cycles", Description = "minimum number of consecutive good cycles to trigger a switch to production. Default: 1", Parameters = "20")]
    [DefaultValue (1)]
    public int NumberOfGoodCycles
    {
      get; set;
    } = 1;
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Configuration ()
    {
      this.NumberOfGoodCycles = 1;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      IEnumerable<string> baseErrors;
      var result = base.IsValid (out baseErrors);

      var errorList = new List<string> ();

      if (this.NumberOfGoodCycles < 1) {
        log.Error ($"IsValid: invalid number of good cycles {this.NumberOfGoodCycles} (< 1)");
        errorList.Add ("Invalid number of good cycles (< 1)");
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.ConfigurationErrors")) {
          if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
              .FindById (this.ProductionMachineStateTemplateId)) {
            log.Error ($"IsValid: Production MachineStateTemplateId {this.ProductionMachineStateTemplateId} does not exist");
            errorList.Add ($"MachineStateTemplate with ID {this.ProductionMachineStateTemplateId} does not exist");
          }

          foreach (var setupMachineStateTempateId in this.SetupMachineStateTemplateIds) {
            if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindById (setupMachineStateTempateId)) {
              log.Error ($"IsValid: Set-up MachineStateTemplateId {setupMachineStateTempateId} does not exist");
              errorList.Add ($"MachineStateTemplate with ID {setupMachineStateTempateId} does not exist");
            }
          }
        }
      }

      var allErrors = baseErrors.Concat (errorList).ToList ();
      errors = allErrors;
      return result && !allErrors.Any ();
    }

    /// <summary>
    /// <see cref="Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter"/>
    /// </summary>
    /// <returns></returns>
    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
    #endregion // Methods
  }
}
