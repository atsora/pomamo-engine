// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Extensions.Configuration.GuiBuilder;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.CncSummaryByStateSlot2
{
  /// <summary>
  /// 
  /// </summary>
  public class CustomAction
    : Lemoine.Extensions.Configuration.IConfiguration
    , Lemoine.Extensions.CustomAction.ICustomAction<Configuration>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CustomAction).FullName);

    #region Getters / Setters
    /// <summary>
    /// Help text for the configuration
    /// </summary>
    public string Help => "Select a period in which data will be computed, then validate. This operation may take time.";

    /// <summary>
    /// 
    /// </summary>
    [PluginConf ("UtcDateTimeRange", "Period", Optional = false)]
    public UtcDateTimeRange Range { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CustomAction ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Return true if the configuration is valid
    /// </summary>
    public bool IsValid (out IEnumerable<string> errors)
    {
      var errorList = new List<string> ();

      if (this.Range.IsEmpty ()) {
        errorList.Add ("Empty period");
      }
      if (!this.Range.Lower.HasValue) {
        errorList.Add ("No lower value");
      }
      if (!this.Range.Upper.HasValue) {
        errorList.Add ("No upper value");
      }

      errors = errorList;
      return (0 == errorList.Count);
    }

    public void DoAction (IEnumerable<Configuration> configurations, ref IList<string> warnings, ref int revisionId)
    {
      // Limits of computation
      var range = this.Range;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ("CncSummaryByStateSlot2.Action.Delete")) {
        NHibernateHelper.GetCurrentSession ()
          .CreateSQLQuery (string.Format (@"
DELETE FROM plugins.cncsummarybystateslot2_values
WHERE startdatetime < '{1}' AND enddatetime > '{0}'
", this.Range.Lower.Value.ToString ("yyyy-MM-dd HH:mm:ss"), this.Range.Upper.Value.ToString ("yyyy-MM-dd HH:mm:ss")))
.ExecuteUpdate ();
        transaction.Commit ();
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // Compute data for each field and each machine module
        var machineModules = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindAll ();
        var fieldIds = new HashSet<int> ();
        foreach (var configuration in configurations) {
          foreach (var cncFieldId in configuration.CncFieldIds) {
            fieldIds.Add (cncFieldId);
          }
        }
        foreach (var fieldId in fieldIds) {
          IField field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (fieldId);
          if (field != null) {
            foreach (var machineModule in machineModules) {
              using (IDAOTransaction transaction = session.BeginTransaction ("CncSummaryByStateSlot2.Action.Recompute")) {
                CncByMachineModuleField implementation = new CncByMachineModuleField (machineModule, field);
                implementation.Recompute (range);
                transaction.Commit ();
              }
            }
          }
          else {
            log.Error ("field not found: cannot compute data");
          }
        }
      }
    }
    #endregion // Methods
  }
}
