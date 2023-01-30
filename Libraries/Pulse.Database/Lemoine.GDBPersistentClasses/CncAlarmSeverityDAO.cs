// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncAlarmSeverityDAO">ICncAlarmSeverityDAO</see>
  /// </summary>
  public class CncAlarmSeverityDAO
    : VersionableNHibernateDAO<CncAlarmSeverity, ICncAlarmSeverity, int>
    , ICncAlarmSeverityDAO
  {
    // List to be completed
    internal static string[] CNC_WITH_DEFAULT_VALUES = new[] {
      "CncTest",
      "DElectron",
      "Fidia (fapi)",
      "MTConnect - Okuma",
      "Okuma - ThincApi",
      "HeidenhainDNC",
      "MML3",
      "Fanuc",
      "HeidenhainLSV2",
      "Brother"
    };

    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityDAO).FullName);

    #region Queries
    /// <summary>
    /// Find a ICncAlarmSeverity with the specified cnc type and name
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="severityName"></param>
    /// <returns></returns>
    public ICncAlarmSeverity FindByCncName (string cncInfo, string severityName)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarmSeverity> ()
        .Add (Restrictions.Eq ("CncInfo", cncInfo))
        .Add (Restrictions.Eq ("Name", severityName))
        .UniqueResult<ICncAlarmSeverity> ();
    }

    /// <summary>
    /// Find all ICncAlarmSeverity for a specified cnc type
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="withStatusDisabled">true if we also want the disabled severities</param>
    /// <returns></returns>
    public IList<ICncAlarmSeverity> FindByCnc (string cncInfo, bool withStatusDisabled)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarmSeverity> ()
        .Add (Restrictions.Eq ("CncInfo", cncInfo));

      // Exclude status 3 if withStatusDisabled is false
      if (!withStatusDisabled) {
        criteria = criteria.Add (Restrictions.In ("Status", new object[] { 0, 1, 2 }));
      }

      return criteria.AddOrder (new Order ("Name", true)).List<ICncAlarmSeverity> ();
    }

    IList<string> GetAllCncWithDefaultValues ()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateQuery ("SELECT DISTINCT CncInfo FROM CncAlarmSeverity WHERE Status != 0;")
        .List<string> ();
    }
    #endregion Queries

    #region Default values
    /// <summary>
    /// This function defines severity patterns by CNC
    /// It can be completed
    /// (Don't forget the array "m_cncWithDefaultValues")
    /// </summary>
    /// <param name="cncType"></param>
    void UpdateDefaultValues (string cncType)
    {
      log.DebugFormat ("CncAlarmSeverityDAO: updating default values for cnc '{0}'", cncType);

      CncAlarmSeverity severity;
      switch (cncType) {
      case "CncTest":
        // 2 severity kinds for testing at the office
        severity = new CncAlarmSeverity (cncType, "small problem");
        severity.Description = "This is a small problem.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "big problem");
        severity.Description = "This is a big problem.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Yes;
        InsertDefaultValue (severity);
        break;
      case "DElectron":
        // We only have an information for the category "Initialization"
        severity = new CncAlarmSeverity (cncType, "severe");
        severity.Description = "Machine stop, possibly requiring a reboot.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Yes;
        InsertDefaultValue (severity);
        break;
      case "Fidia (fapi)":
        // 5 severities are described in the document "MDO1924v5_MSG_EN"
        severity = new CncAlarmSeverity (cncType, "informative");
        severity.Description = "No operation is required on the part of the operator.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "request");
        severity.Description = "The operator is requested to execute a specific operation on the machine tool.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "warning");
        severity.Description = "The operator will be required to execute a reset operation.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.Possibly;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error");
        severity.Description = "A programming error or an error in setting parameters is reported.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Yes;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "fatal");
        severity.Description = "Malfunctioning in which FIDIA Technical Assistance staff will be required to execute a reset operation.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Yes;
        InsertDefaultValue (severity);
        break;
      case "MTConnect - Okuma":
      case "Okuma - ThincApi":
        // 5 levels according to Linh Huynh
        severity = new CncAlarmSeverity (cncType, "P - hardware problem");
        severity.Description = "The machine will normally not function correctly.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Yes;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "A - immediate stop");
        severity.Description = "The alarm stops the machine immediately.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "B - stop after current block");
        severity.Description = "The alarm stops the machine after the current block.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.BlockEnd;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "C - stop at the end of the program");
        severity.Description = "The alarm stops the machine at the end of the program.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.ProgramEnd;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "D - simple warning");
        severity.Description = "Informative alarm that doesn't stop the machine.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);
        break;
      case "HeidenhainDNC":
      case "HeidenhainLSV2":
        severity = new CncAlarmSeverity (cncType, "warning, no stop");
        severity.Description = "Severity Warning, no effect on machine/execution state.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error with feed hold");
        severity.Description = "Severity Error, with execution feed hold.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.FeedHold;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error with program hold");
        severity.Description = "Severity Error, with execution program hold.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.BlockEnd;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error with program abort");
        severity.Description = "Severity Error, with execution program abort.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error with emergency stop");
        severity.Description = "Severity Error, with machine emergency stop.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error with emergency stop and control reset");
        severity.Description = "Severity Error, with machine emergency stop, requires control reset .";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);

        if (cncType == "HeidenhainDNC") {
          // Three more levels, not present for LSV2
          severity = new CncAlarmSeverity (cncType, "info, no stop");
          severity.Description = "Severity Info, no effect on machine/execution state.";
          severity.Focus = false;
          severity.StopStatus = CncAlarmStopStatus.No;
          InsertDefaultValue (severity);

          severity = new CncAlarmSeverity (cncType, "error, no stop");
          severity.Description = "Severity Error, no effect on machine/execution state.";
          severity.Focus = false;
          severity.StopStatus = CncAlarmStopStatus.No;
          InsertDefaultValue (severity);

          severity = new CncAlarmSeverity (cncType, "note, no stop");
          severity.Description = "Severity Note, no effect on machine/execution state.";
          severity.Focus = false;
          severity.StopStatus = CncAlarmStopStatus.No;
          InsertDefaultValue (severity);
        }
        break;
      case "MML3":
        severity = new CncAlarmSeverity (cncType, "warning");
        severity.Description = "A simple warning";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "alarm");
        severity.Description = "More serious than a warning.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Possibly;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "alarm with damage");
        severity.Description = "The system has probably been damaged.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);
        break;
      case "Fanuc":
        severity = new CncAlarmSeverity (cncType, "notice");
        severity.Description = "Severity note, no effect on machine / execution state.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error, program abort");
        severity.Description = "Severity error, with execution program abort.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Yes;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "error, machine stop");
        severity.Description = "Severity error, with machine emergency stop.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);
        break;
      case "Brother":
        severity = new CncAlarmSeverity (cncType, "immediate stop");
        severity.Description = "The alarm stops the machine immediately.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.Immediate;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "stop after current block");
        severity.Description = "The alarm stops the machine after the current block.";
        severity.Focus = true;
        severity.StopStatus = CncAlarmStopStatus.BlockEnd;
        InsertDefaultValue (severity);

        severity = new CncAlarmSeverity (cncType, "warning");
        severity.Description = "Simple warning that doesn't stop the machine.";
        severity.Focus = false;
        severity.StopStatus = CncAlarmStopStatus.No;
        InsertDefaultValue (severity);
        break;
      default:
        log.WarnFormat ("Couldn't update default values for cnc '{0}'", cncType);
        break;
      }
    }

    /// <summary>
    /// Return true if default values are available for a CNC
    /// </summary>
    /// <param name="cncType"></param>
    public bool AreDefaultValuesAvailable (string cncType)
    {
      foreach (var cncWithDefaultValue in CNC_WITH_DEFAULT_VALUES) {
        if (cncWithDefaultValue.Equals (cncType, StringComparison.CurrentCulture)) {
          return true;
        }
      }

      return false;
    }

    /// <summary>
    /// Insert the default values without overriding manual inputs
    /// </summary>
    internal void InsertDefaultValues ()
    {
      // Insert all kinds of default values, not forcing the update
      foreach (var cncWithDefaultValue in CNC_WITH_DEFAULT_VALUES) {
        UpdateDefaultValues (cncWithDefaultValue);
      }
    }

    /// <summary>
    /// Restore the default values of a cnc type
    /// </summary>
    /// <param name="cncType">cnc type to update</param>
    public void RestoreDefaultValues (string cncType)
    {
      // Clear values
      var severities = FindByCnc (cncType, true);
      foreach (var severity in severities) {
        MakeTransient (severity);
      }

      // Update them
      UpdateDefaultValues (cncType);
    }

    /// <summary>
    /// Function to use in "UpdateDefaultValues" if a default value is not used anymore
    /// </summary>
    /// <param name="cncType"></param>
    /// <param name="name"></param>
    void DeleteOldDefaultValue (string cncType, string name)
    {
      var patternTmp = FindByCncName (cncType, name);
      if (patternTmp != null && patternTmp.Status == EditStatus.DEFAULT_VALUE) {
        MakeTransient (patternTmp);
      }
    }

    /// <summary>
    /// Function to use in "UpdateDefaultValues" to insert or update a default value
    /// </summary>
    /// <param name="severity"></param>
    void InsertDefaultValue (ICncAlarmSeverity severity)
    {
      var severityTmp = FindByCncName (severity.CncInfo, severity.Name);
      if (severityTmp != null) {
        if (severityTmp.Status == EditStatus.DEFAULT_VALUE) {
          // Update the severity already stored in the database
          // ("Focus" and "Color" remain the same)
          severityTmp.Description = severity.Description;
          severityTmp.StopStatus = severity.StopStatus;
          MakePersistent (severityTmp);
        }
      }
      else {
        // Create a new severity
        severity.Status = EditStatus.DEFAULT_VALUE;
        MakePersistent (severity);
      }
    }
    #endregion Default values
  }
}
