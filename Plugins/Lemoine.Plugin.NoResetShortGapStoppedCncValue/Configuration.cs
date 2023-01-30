// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;
using Newtonsoft.Json;

namespace Lemoine.Plugin.NoResetShortGapStoppedCncValue
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration
    : Pulse.Extensions.Configuration.Implementation.ConfigurationWithMachineFilter
    , Pulse.Extensions.Configuration.IConfigurationWithMachineFilter
  {
    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Max duration
    /// </summary>
    [PluginConf ("DurationPicker", "Maximum duration", Description = "the maximum gap. Default: 0:00:20")]
    [DefaultValue ("0:00:20")]
    public TimeSpan MaxGap
    {
      get; set;
    } = TimeSpan.FromSeconds (20);

    /// <summary>
    /// Applicable field
    /// 
    /// Default: Feedrate (mm/min)
    /// </summary>
    [PluginConf ("Field", "Applicable field", Description = "Applicable field. Default: Feedrate (mm/min)", Optional = true, Multiple = false)]
    [DefaultValue (100)]
    public int FieldId
    {
      get; set;
    } = 100;
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
    /// ToString implementation
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return JsonConvert.SerializeObject (this);
    }

    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public override bool IsValid (out IEnumerable<string> errors)
    {
      IEnumerable<string> baseErrors;
      var result = base.IsValid (out baseErrors);

      var errorList = new List<string> ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("NoResetShortGapStoppedCncValue.ConfigurationErrors")) {
          if (null == ModelDAOHelper.DAOFactory.FieldDAO
              .FindById (this.FieldId)) {
            log.ErrorFormat ("GetConfigurationErrors: " +
                             "Field {0} does not exist",
                             this.FieldId);
            errorList.Add ("Field with ID "
                        + this.FieldId
                        + " does not exist");
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
