// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
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
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Configuration ()
    {
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
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.ConfigurationErrors")) {
          if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
              .FindById (this.ProductionMachineStateTemplateId)) {
            log.ErrorFormat ("IsValid: " +
                             "Production MachineStateTemplateId {0} does not exist",
                             this.ProductionMachineStateTemplateId);
            errorList.Add ("MachineStateTemplate with ID " + this.ProductionMachineStateTemplateId + " does not exist");
          }

          foreach (var setupMachineStateTempateId in this.SetupMachineStateTemplateIds) {
            if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindById (setupMachineStateTempateId)) {
              log.ErrorFormat ("IsValid: " +
                               "Set-up MachineStateTemplateId {0} does not exist",
                               this.SetupMachineStateTemplateIds);
              errorList.Add ("MachineStateTemplate with ID " + setupMachineStateTempateId + " does not exist");
            }
          }
        }
      }

      errors = baseErrors.Concat (errorList);
      return result && (!errors.Any ());
    }

    protected override bool IsMachineFilterRequired ()
    {
      return false;
    }
    #endregion // Methods
  }
}
