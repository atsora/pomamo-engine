// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncAlarmSeverityPatternDAO">ICncAlarmSeverityPatternDAO</see>
  /// </summary>
  public class CncAlarmSeverityPatternDAO
    : VersionableNHibernateDAO<CncAlarmSeverityPattern, ICncAlarmSeverityPattern, int>
    , ICncAlarmSeverityPatternDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CncAlarmSeverityPatternDAO).FullName);

    #region Queries
    /// <summary>
    /// Find all ICncAlarmSeverityPattern for a specified severity
    /// </summary>
    /// <param name="severity"></param>
    /// <param name="withStatusDisabled">true if we also want the disabled patterns</param>
    /// <returns></returns>
    public IList<ICncAlarmSeverityPattern> FindBySeverity (ICncAlarmSeverity severity, bool withStatusDisabled)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarmSeverityPattern> ()
        .Add (Restrictions.Eq ("Severity", severity));

      // Exclude status 3 if withStatusDisabled is false
      if (!withStatusDisabled) {
        criteria = criteria.Add (Restrictions.In ("Status", new object[] { 0, 1, 2 }));
      }

      return criteria.List<ICncAlarmSeverityPattern> ();
    }

    /// <summary>
    /// Find all ICncAlarmSeverityPattern for a specified cnc type
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="withStatusDisabled">true if we also want the disabled severities</param>
    /// <returns></returns>
    public IList<ICncAlarmSeverityPattern> FindByCnc (string cncInfo, bool withStatusDisabled)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarmSeverityPattern> ()
        .Add (Restrictions.Eq ("CncInfo", cncInfo));

      // Exclude status 3 if withStatusDisabled is false
      if (!withStatusDisabled) {
        criteria = criteria.Add (Restrictions.In ("Status", new object[] { 0, 1, 2 }));
      }

      return criteria.AddOrder (new Order ("Name", true)).List<ICncAlarmSeverityPattern> ();
    }

    /// <summary>
    /// Find a ICncAlarmSeverityPattern with the specified cnc type and name
    /// </summary>
    /// <param name="cncInfo"></param>
    /// <param name="patternName"></param>
    /// <returns></returns>
    public ICncAlarmSeverityPattern FindByCncName (string cncInfo, string patternName)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<CncAlarmSeverityPattern> ()
        .Add (Restrictions.Eq ("CncInfo", cncInfo))
        .Add (Restrictions.Eq ("Name", patternName))
        .UniqueResult<ICncAlarmSeverityPattern> ();
    }
    #endregion Queries

    #region Default values
    /// <summary>
    /// This function defines severity patterns by CNC
    /// It can be completed
    /// (Don't forget the array "CNC_WITH_DEFAULT_VALUES" in CncAlarmSeverityDAO)
    /// </summary>
    /// <param name="cncType"></param>
    void UpdateDefaultValues (string cncType)
    {
      log.DebugFormat ("CncAlarmSeverityPatternDAO: updating default values for cnc '{0}'", cncType);

      var alarmSeverityDAO = new CncAlarmSeverityDAO ();
      CncAlarmSeverityPatternRules patternRules;
      switch (cncType) {
      case "CncTest":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "1.*";
        InsertDefaultValue (cncType, "small problem", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "2.*14";
        InsertDefaultValue (cncType, "big problem", "2", patternRules);
        break;
      case "DElectron":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = ".*(02|07|09|14)";
        InsertDefaultValue (cncType, "severe", "1", patternRules);
        break;
      case "Fidia (fapi)":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^[IDHPL].*";
        InsertDefaultValue (cncType, "informative", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^R.*";
        InsertDefaultValue (cncType, "request", "2", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^W.*";
        InsertDefaultValue (cncType, "warning", "3", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^E.*";
        InsertDefaultValue (cncType, "error", "4", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^F.*";
        InsertDefaultValue (cncType, "fatal", "5", patternRules);
        break;
      case "MTConnect - Okuma":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^0*[0-9]{0,3}($|[^0-9].*)"; // Less than 1000
        InsertDefaultValue (cncType, "P - hardware problem", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^0*1[0-9]{3}($|[^0-9].*)";
        InsertDefaultValue (cncType, "A - immediate stop", "2", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^0*2[0-9]{3}($|[^0-9].*)";
        InsertDefaultValue (cncType, "B - stop after current block", "3", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^0*3[0-9]{3}($|[^0-9].*)";
        InsertDefaultValue (cncType, "C - stop at the end of the program", "4", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Number = "^0*([4-9]|[1-9][0-9])[0-9]{3}($|[^0-9].*)";
        InsertDefaultValue (cncType, "D - simple warning", "5", patternRules);
        break;
      case "Okuma - ThincApi":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["Level"] = "ALARM_P";
        InsertDefaultValue (cncType, "P - hardware problem", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["Level"] = "ALARM_A";
        InsertDefaultValue (cncType, "A - immediate stop", "2", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["Level"] = "ALARM_B";
        InsertDefaultValue (cncType, "B - stop after current block", "3", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["Level"] = "ALARM_C";
        InsertDefaultValue (cncType, "C - stop at the end of the program", "4", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["Level"] = "ALARM_D";
        InsertDefaultValue (cncType, "D - simple warning", "5", patternRules);
        break;
      case "HeidenhainDNC":
      case "HeidenhainLSV2":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["severity"] = "warning, no stop";
        InsertDefaultValue (cncType, "warning, no stop", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["severity"] = "error with feed hold";
        InsertDefaultValue (cncType, "error with feed hold", "2", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["severity"] = "error with program hold";
        InsertDefaultValue (cncType, "error with program hold", "3", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["severity"] = "error with program abort";
        InsertDefaultValue (cncType, "error with program abort", "4", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["severity"] = "error with emergency stop";
        InsertDefaultValue (cncType, "error with emergency stop", "5", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["severity"] = "error with emergency stop and control reset";
        InsertDefaultValue (cncType, "error with emergency stop and control reset", "6", patternRules);

        if (cncType == "HeidenhainDNC") {
          // 3 more rules, no present for LSV2
          patternRules = new CncAlarmSeverityPatternRules ();
          patternRules.Properties["severity"] = "info, no stop";
          InsertDefaultValue (cncType, "info, no stop", "7", patternRules);

          patternRules = new CncAlarmSeverityPatternRules ();
          patternRules.Properties["severity"] = "error, no stop";
          InsertDefaultValue (cncType, "error, no stop", "8", patternRules);

          patternRules = new CncAlarmSeverityPatternRules ();
          patternRules.Properties["severity"] = "note, no stop";
          InsertDefaultValue (cncType, "note, no stop", "9", patternRules);
        }
        break;
      case "MML3":
        // Warning is type "warning" and serious level "normal" (machine alarms) or type "1" (cnc alarms)
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["type"] = "warning";
        patternRules.Properties["serious level"] = "normal";
        InsertDefaultValue (cncType, "warning", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["type"] = "1";
        InsertDefaultValue (cncType, "warning", "2", patternRules);

        // Alarm is type "alarm" and serious level "normal" (machine alarms) or type "2" (cnc alarms)
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["type"] = "alarm";
        patternRules.Properties["serious level"] = "normal";
        InsertDefaultValue (cncType, "alarm", "3", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["type"] = "2";
        InsertDefaultValue (cncType, "alarm", "4", patternRules);

        // Alarm with damage is serious level "damage" (only for machine alarms)
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["serious level"] = "damage";
        InsertDefaultValue (cncType, "alarm with damage", "5", patternRules);
        break;
      case "Fanuc":
        // CLASS 3XX //
        // Notice
        // "Background P/S", "Parameter switch on" shared with 150
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Type = "^(Parameter switch on|Preventive function alarm|Background P/S)$";
        InsertDefaultValue (cncType, "notice", "1", patternRules);
        // Error, program abort
        // All shared with 150
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Type = "^(Power off parameter set|I/O error|Foreground P/S|Macro alarm)$";
        InsertDefaultValue (cncType, "error, program abort", "2", patternRules);
        // Error, machine stop
        // "Overtravel.*", "Overheat alarm", "Servo alarm", "Spindle alarm" shared with 160 and 150
        // "Synchronized error", "PMC error" shared with 150
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Type = "^(Overtravel.*|Overheat alarm|Servo alarm|Data I/O error|Spindle alarm|Synchronized error|PMC error)$";
        InsertDefaultValue (cncType, "error, machine stop", "3", patternRules);

        // CLASS 160 //
        // TODO: "P/S 100", "P/S 000", "P/S 101", "P/S other alarm", "System alarm", "APC alarm",
        // "P/S alarm (5000 ...), Punchpress alarm", "Laser alarm", "Rigid tap alarm"

        // CLASS 150 //
        // TODO: "Sub-CPU error", "Serious P/S", "System alarm"

        // PMC alarms (common for all three classes)
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Type = "^External.*";
        patternRules.Number = "^1[0-9]{3}$";
        InsertDefaultValue (cncType, "error, machine stop", "10", patternRules);
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Type = "^External.*";
        patternRules.Number = "^2[0-9]{3}$";
        InsertDefaultValue (cncType, "notice", "11", patternRules);

        /*****************
        * Machine alarms *
        *****************/

        // Johnford or Feeler
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.CncSubInfo = "johnford|feeler";
        patternRules.Number = "^1[0-9]{3}$";
        InsertDefaultValue (cncType, "error, machine stop", "100", patternRules);
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.CncSubInfo = "johnford|feeler";
        patternRules.Number = "^2[0-9]{3}$";
        InsertDefaultValue (cncType, "notice", "101", patternRules);
        break;
      case "Brother":
        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["StopLevel"] = "Immediate stop (servo system)";
        InsertDefaultValue (cncType, "Immediate stop", "1", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["StopLevel"] = "Immediate stop";
        InsertDefaultValue (cncType, "Immediate stop", "2", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["StopLevel"] = "Stop after current block";
        InsertDefaultValue (cncType, "stop after current block", "3", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["StopLevel"] = "Stop after current block (M02	or M30)";
        InsertDefaultValue (cncType, "stop after current block", "4", patternRules);

        patternRules = new CncAlarmSeverityPatternRules ();
        patternRules.Properties["StopLevel"] = "Warning, no stop";
        InsertDefaultValue (cncType, "warning", "5", patternRules);
        break;
      default:
        log.WarnFormat ("Couldn't update default patterns for cnc '{0}'", cncType);
        break;
      }
    }

    /// <summary>
    /// Insert the default values without overriding manual inputs
    /// </summary>
    internal void InsertDefaultValues ()
    {
      // Insert all kinds of default values, not forcing the update
      foreach (var cncWithDefaultValue in CncAlarmSeverityDAO.CNC_WITH_DEFAULT_VALUES) {
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
      var patterns = FindByCnc (cncType, true);
      foreach (var pattern in patterns) {
        MakeTransient (pattern);
      }

      // Update them
      UpdateDefaultValues (cncType);
    }

    /// <summary>
    /// Function to use in "UpdateDefaultValues" if a default value is not used anymore
    /// </summary>
    /// <param name="cncType"></param>
    /// <param name="patternName"></param>
    void DeleteOldDefaultValue (string cncType, string patternName)
    {
      var patternTmp = FindByCncName (cncType, patternName);
      if (patternTmp != null && patternTmp.Status == EditStatus.DEFAULT_VALUE) {
        MakeTransient (patternTmp);
      }
    }

    /// <summary>
    /// Function to use in "UpdateDefaultValues" to insert or update a default value
    /// </summary>
    /// <param name="cncType"></param>
    /// <param name="severityName"></param>
    /// <param name="patternName">Unique identifier of the pattern per CNC type</param>
    /// <param name="patternRules"></param>
    void InsertDefaultValue (string cncType, string severityName, string patternName, CncAlarmSeverityPatternRules patternRules)
    {
      var severity = (new CncAlarmSeverityDAO ()).FindByCncName (cncType, severityName);
      if (severity == null) {
        log.ErrorFormat ("Couldn't update a pattern of '{0}' because the severity '{1}' has not been found",
                        cncType, severityName);
        return;
      }

      // Pattern already existing?
      var patternTmp = FindByCncName (cncType, patternName);
      if (patternTmp != null) {
        // The update is only possible if the pattern has not been manually edited or deleted
        if (patternTmp.Status == EditStatus.DEFAULT_VALUE) {
          // Update the pattern already stored in the database
          // Note: not ideal, but necessary to use the readonly cache in .exe.config
          bool patternChanged = !object.Equals(patternTmp.Rules, patternRules);
          if (patternChanged || (patternTmp.Severity.Id != severity.Id)) {
            patternTmp.Rules = patternRules;
            patternTmp.Severity = severity;
            MakePersistent (patternTmp);
          }
        }
      }
      else {
        // Create a new pattern
        var pattern = new CncAlarmSeverityPattern (cncType, patternRules, severity);
        pattern.Name = patternName;
        pattern.Status = EditStatus.DEFAULT_VALUE;
        MakePersistent (pattern);
      }
    }
    #endregion Default values
  }
}
