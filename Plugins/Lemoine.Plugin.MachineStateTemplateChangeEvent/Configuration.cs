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

namespace Lemoine.Plugin.MachineStateTemplateChangeEvent
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

    static readonly ILog log = LogManager.GetLogger(typeof (Configuration).FullName);

    #region Getters / Setters    
    /// <summary>
    /// New machine state template ID
    /// 
    /// 0 (default): applies to all machine state templates
    /// </summary>
    [PluginConf ("MachineStateTemplate", "New machine state template", Description = "the machine state template that generates the event (none corresponds to all)", Optional = true, Multiple = false)]
    public int NewMachineStateTemplateId {
      get; set;
    }
    
    /// <summary>
    /// Event level ID
    /// </summary>
    [PluginConf ("EventLevel", "Event level", Description = "the associated event level", Optional = false, Multiple = false)]
    public int EventLevelId {
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
      if (0 == this.EventLevelId) {
        errorList.Add ("No event level id is defined");
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("ProductionSwitcher.ConfigurationErrors")) {
          if (0 != this.NewMachineStateTemplateId) {
            if (null == ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindById (this.NewMachineStateTemplateId)) {
              log.ErrorFormat ("GetConfigurationErrors: " +
                               "Production MachineStateTemplateId {0} does not exist",
                               this.NewMachineStateTemplateId);
              errorList.Add ("MachineStateTemplate with ID " + this.NewMachineStateTemplateId + " does not exist");
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
