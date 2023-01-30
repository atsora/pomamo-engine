// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Configuration.GuiBuilder;
using System.Linq;

namespace Lemoine.Plugin.SetupSwitcher
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
    /// Set-up Machine State Template ID
    /// </summary>
    [PluginConf ("MachineStateTemplate", "Set-up", Description = "the machine state template that corresponds to a set-up", Optional = false, Multiple = false)]
    public int SetupMachineStateTemplateId
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
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("SetupSwitcher.ConfigurationErrors")) {
          if (0 == this.SetupMachineStateTemplateId) {
            log.ErrorFormat ("GetConfigurationErrors: " +
                             "SetupMachineStateTemplateId was not set",
                             this.SetupMachineStateTemplateId);
            errorList.Add ("Set-up machine state template not set");
          }
          else {
            if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
              .FindById (this.SetupMachineStateTemplateId)) {
              log.ErrorFormat ("GetConfigurationErrors: " +
                               "SetupMachineStateTemplateId {0} does not exist",
                               this.SetupMachineStateTemplateId);
              errorList.Add ("Set-up machine state template with ID "
                          + this.SetupMachineStateTemplateId
                          + " does not exist");
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
