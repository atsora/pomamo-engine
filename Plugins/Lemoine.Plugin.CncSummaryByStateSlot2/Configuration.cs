// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration
    : Lemoine.Extensions.Configuration.IConfiguration
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// List of CNC Field Ids
    /// </summary>
    [PluginConf ("Field", "Cnc fields", Description = "the different CNC values which will be averaged per production period", Optional = false, Multiple = true)]
    public IEnumerable<int> CncFieldIds
    {
      get; set;
    } = new List<int> ();

    /// <summary>
    /// Track any change of the observation state slot (obsolete)
    /// </summary>
    [Obsolete("Deprecated property")]
    public bool TrackObservationStateSlotChange {
      get; set;
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Configuration ()
    {
    }

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      // All cnc values must be valid
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          foreach (var fieldId in this.CncFieldIds) {
            if (ModelDAOHelper.DAOFactory.FieldDAO.FindById (fieldId) == null) {
              log.ErrorFormat ("IsValid: field {0} does not exist", fieldId);
              errorList.Add ("field " + fieldId + " does not exist");
            }
          }
        }
      }

      errors = errorList;
      return (0 == errorList.Count);
    }
    #endregion // Methods
  }
}
