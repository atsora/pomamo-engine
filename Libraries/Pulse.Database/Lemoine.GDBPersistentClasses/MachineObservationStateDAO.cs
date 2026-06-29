// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineObservationStateDAO">IMachineObservationStateDAO</see>
  /// </summary>
  public class MachineObservationStateDAO
    : VersionableNHibernateDAO<MachineObservationState, IMachineObservationState, int>
    , IMachineObservationStateDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineObservationStateDAO).FullName);

    /// <summary>
    /// Configuration key to insert default values that are more mold shops
    /// or shops that were machines are running without any people:
    /// Unattended, Attended, OnSite, OnCall
    /// 
    /// It must be on for unit tests databases
    /// </summary>
    static readonly string UNATTENDED_TO_ON_CALL_KEY = "Database.Default.MachineObservationState.UnattendedToOnCall";
    static readonly bool UNATTENDED_TO_ON_CALL_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default values Production and NoProduction
    /// that are applicable to most production shops
    /// 
    /// On by default
    /// </summary>
    static readonly string PRODUCTION_NO_PRODUCTION_KEY = "Database.Default.MachineObservationState.ProductionNoProduction";
    static readonly bool PRODUCTION_NO_PRODUCTION_DEFAULT = true;

    /// <summary>
    /// Configuration key to insert default value Off
    /// </summary>
    static readonly string OFF_KEY = "Database.Default.MachineObservationState.Off";
    static readonly bool OFF_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value Unknown
    /// </summary>
    static readonly string UNKNOWN_KEY = "Database.Default.MachineObservationState.Unknown";
    static readonly bool UNKNOWN_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value SetUp
    /// </summary>
    static readonly string SETUP_KEY = "Database.Default.MachineObservationState.SetUp";
    static readonly bool SETUP_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value QualityCheck
    /// </summary>
    static readonly string QUALITY_CHECK_KEY = "Database.Default.MachineObservationState.QualityCheck";
    static readonly bool QUALITY_CHECK_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value Maintenance
    /// </summary>
    static readonly string MAINTENANCE_KEY = "Database.Default.MachineObservationState.Maintenance";
    static readonly bool MAINTENANCE_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value Break
    /// </summary>
    static readonly string BREAK_KEY = "Database.Default.MachineObservationState.Break";
    static readonly bool BREAK_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value CleanUp
    /// </summary>
    static readonly string CLEANUP_KEY = "Database.Default.MachineObservationState.CleanUp";
    static readonly bool CLEANUP_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value WeekEnd
    /// </summary>
    static readonly string WEEKEND_KEY = "Database.Default.MachineObservationState.WeekEnd";
    static readonly bool WEEKEND_DEFAULT = false;

    /// <summary>
    /// Configuration key to insert default value Night
    /// </summary>
    static readonly string NIGHT_KEY = "Database.Default.MachineObservationState.Night";
    static readonly bool NIGHT_DEFAULT = false;

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      if (Lemoine.Info.ConfigSet.LoadAndGet (UNATTENDED_TO_ON_CALL_KEY, UNATTENDED_TO_ON_CALL_DEFAULT)) {
        // 2: Unattended (used in unit tests)
        IMachineObservationState unattended = new MachineObservationState ((int)MachineObservationStateId.Unattended, "MachineObservationStateUnattended", false);
        InsertDefaultValue (unattended);
        // 1: Attended (used in unit tests)
        IMachineObservationState attended = new MachineObservationState ((int)MachineObservationStateId.Attended, "MachineObservationStateAttended", true);
        attended.OnSite = true;
        attended.SiteAttendanceChange = unattended;
        attended.IsProduction = true;
        InsertDefaultValue (attended);
        // 3: On-site (not used in unit tests)
        IMachineObservationState onSite = new MachineObservationState ((int)MachineObservationStateId.OnSite, "MachineObservationStateOnSite", true);
        onSite.OnSite = true;
        onSite.SiteAttendanceChange = unattended;
        InsertDefaultValue (onSite);
        { // 4: On-call (not used in unit tests)
          IMachineObservationState onCall = new MachineObservationState ((int)MachineObservationStateId.OnCall, "MachineObservationStateOnCall", true);
          onCall.OnSite = false;
          onCall.SiteAttendanceChange = onSite;
          InsertDefaultValue (onCall);
        }
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (OFF_KEY, OFF_DEFAULT)) { // 5: Off
        IMachineObservationState off = new MachineObservationState ((int)MachineObservationStateId.Off, "MachineObservationStateOff", false);
        InsertDefaultValue (off);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (UNKNOWN_KEY, UNKNOWN_DEFAULT)) { // 6: Unknown
        IMachineObservationState unknown = new MachineObservationState ((int)MachineObservationStateId.Unknown, "MachineObservationStateUnknown", false);
        InsertDefaultValue (unknown);
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (PRODUCTION_NO_PRODUCTION_KEY, PRODUCTION_NO_PRODUCTION_DEFAULT)) {
        // 15: NoProduction
        IMachineObservationState noProduction = new MachineObservationState ((int)MachineObservationStateId.NoProduction, "MachineObservationStateNoProduction", true);
        noProduction.OnSite = false;
        noProduction.IsProduction = false;
        InsertDefaultValue (noProduction);
        // 9: Production
        IMachineObservationState production = new MachineObservationState ((int)MachineObservationStateId.Production, "MachineObservationStateProduction", true);
        production.OnSite = true;
        production.SiteAttendanceChange = noProduction;
        production.IsProduction = true;
        InsertDefaultValue (production);
      }

      if (Lemoine.Info.ConfigSet.LoadAndGet (SETUP_KEY, SETUP_DEFAULT)) { // 7: Setup
        IMachineObservationState setUp = new MachineObservationState ((int)MachineObservationStateId.SetUp, "MachineObservationStateSetUp", true);
        setUp.OnSite = true;
        InsertDefaultValue (setUp);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (QUALITY_CHECK_KEY, QUALITY_CHECK_DEFAULT)) { // 8: Quality check
        IMachineObservationState qualityCheck = new MachineObservationState ((int)MachineObservationStateId.QualityCheck, "MachineObservationStateQualityCheck", false);
        InsertDefaultValue (qualityCheck);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (MAINTENANCE_KEY, MAINTENANCE_DEFAULT)) { // 10: Maintenance
        IMachineObservationState maintenance = new MachineObservationState ((int)MachineObservationStateId.Maintenance, "MachineObservationStateMaintenance", false);
        InsertDefaultValue (maintenance);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (BREAK_KEY, BREAK_DEFAULT)) { // 11: Break
        IMachineObservationState machineObservationState =
          new MachineObservationState ((int)MachineObservationStateId.Break, "MachineObservationStateBreak", true);
        machineObservationState.OnSite = true;
        machineObservationState.IsProduction = false;
        InsertDefaultValue (machineObservationState);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (CLEANUP_KEY, CLEANUP_DEFAULT)) { // 12: Cleanup
        IMachineObservationState machineObservationState =
          new MachineObservationState ((int)MachineObservationStateId.Cleanup, "MachineObservationStateCleanup", true);
        machineObservationState.OnSite = true;
        machineObservationState.IsProduction = false;
        InsertDefaultValue (machineObservationState);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (WEEKEND_KEY, WEEKEND_DEFAULT)) { // 13: Week-end (Production by default, but this can be changed)
        IMachineObservationState weekEnd = new MachineObservationState ((int)MachineObservationStateId.Weekend, "MachineObservationStateWeekEnd", true);
        weekEnd.IsProduction = true;
        InsertDefaultValue (weekEnd);
      }
      if (Lemoine.Info.ConfigSet.LoadAndGet (NIGHT_KEY, NIGHT_DEFAULT)) { // 14: Night (Production)
        IMachineObservationState night = new MachineObservationState ((int)MachineObservationStateId.Night, "MachineObservationStateNight", true);
        night.IsProduction = true;
        InsertDefaultValue (night);
      }

      ResetSequence (100);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="machineObservationState">not null</param>
    private void InsertDefaultValue (IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machineObservationState);

      try {
        if (null == FindById (machineObservationState.Id)) { // the config does not exist => create it
          log.Info ($"InsertDefaultValue: add id={machineObservationState.Id} translationKey={machineObservationState.TranslationKey}");
          // Use a raw SQL Command, else the Id is resetted
          string onSite = "NULL";
          if (machineObservationState.OnSite.HasValue) {
            onSite = machineObservationState.OnSite.Value ? "TRUE" : "FALSE";
          }
          string attendanceChange = "NULL";
          if (null != machineObservationState.SiteAttendanceChange) {
            attendanceChange = machineObservationState.SiteAttendanceChange.Id.ToString ();
          }
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
            command.CommandText = string.Format (@"INSERT INTO machineobservationstate (machineobservationstateid, machineobservationstatetranslationkey, machineobservationstateuserrequired, machineobservationstateonsite, machineobservationstateidsiteattendancechange, machineobservationstateisproduction)
VALUES ({0}, '{1}', {2}, {3}, {4}, {5})",
                                                 machineObservationState.Id,
                                                 machineObservationState.TranslationKey,
                                                 machineObservationState.UserRequired ? "TRUE" : "FALSE",
                                                 onSite,
                                                 attendanceChange,
                                                 machineObservationState.IsProduction ? "TRUE" : "FALSE");
            command.ExecuteNonQuery ();
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"InsertDefaultValue: inserting new machine observation state {machineObservationState} failed", ex);
      }
    }

    private void ResetSequence (int minId)
    {
      try {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand ()) {
          command.CommandText = string.Format (@"
WITH maxid AS (SELECT MAX({1}) AS maxid FROM {0})
SELECT SETVAL('{0}_{1}_seq', CASE WHEN (SELECT maxid FROM maxid) < {2} THEN {2} ELSE (SELECT maxid FROM maxid) + 1 END);",
                                               "machineobservationstate", "machineobservationstateid",
                                               minId);
          command.ExecuteNonQuery ();
        }
      }
      catch (Exception ex) {
        log.Error ($"ResetSequence: resetting the sequence failed", ex);
      }
    }
    #endregion // DefaultValues
  }
}
