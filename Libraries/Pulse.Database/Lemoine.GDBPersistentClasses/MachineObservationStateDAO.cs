// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      // 2: Unattended
      IMachineObservationState unattended = new MachineObservationState ((int)MachineObservationStateId.Unattended, "MachineObservationStateUnattended", false);
      InsertDefaultValue (unattended);
      // 1: Attended
      IMachineObservationState attended = new MachineObservationState ((int)MachineObservationStateId.Attended, "MachineObservationStateAttended", true);
      attended.OnSite = true;
      attended.SiteAttendanceChange = unattended;
      attended.IsProduction = true;
      InsertDefaultValue (attended);
      // 3: On-site
      IMachineObservationState onSite = new MachineObservationState ((int)MachineObservationStateId.OnSite, "MachineObservationStateOnSite", true);
      onSite.OnSite = true;
      onSite.SiteAttendanceChange = unattended;
      InsertDefaultValue (onSite);
      { // 4: On-call
        IMachineObservationState onCall = new MachineObservationState ((int)MachineObservationStateId.OnCall, "MachineObservationStateOnCall", true);
        onCall.OnSite = false;
        onCall.SiteAttendanceChange = onSite;
        InsertDefaultValue (onCall);
      }
      { // 5: Off
        IMachineObservationState off = new MachineObservationState ((int)MachineObservationStateId.Off, "MachineObservationStateOff", false);
        InsertDefaultValue (off);
      }
      { // 6: Unknown
        IMachineObservationState unknown = new MachineObservationState ((int)MachineObservationStateId.Unknown, "MachineObservationStateUnknown", false);
        InsertDefaultValue (unknown);
      }
      { // 7: Setup
        IMachineObservationState setUp = new MachineObservationState ((int)MachineObservationStateId.SetUp, "MachineObservationStateSetUp", true);
        setUp.OnSite = true;
        setUp.SiteAttendanceChange = unattended;
        InsertDefaultValue (setUp);
      }
      { // 8: Quality check
        IMachineObservationState qualityCheck = new MachineObservationState ((int)MachineObservationStateId.QualityCheck, "MachineObservationStateQualityCheck", false);
        InsertDefaultValue (qualityCheck);
      }
      { // 9: Production
        IMachineObservationState production = new MachineObservationState ((int)MachineObservationStateId.Production, "MachineObservationStateProduction", true);
        production.OnSite = true;
        production.SiteAttendanceChange = unattended;
        production.IsProduction = true;
        InsertDefaultValue (production);
      }
      { // 10: Maintenance
        IMachineObservationState maintenance = new MachineObservationState ((int)MachineObservationStateId.Maintenance, "MachineObservationStateMaintenance", false);
        InsertDefaultValue (maintenance);
      }
      { // 11: Break
        IMachineObservationState machineObservationState =
          new MachineObservationState ((int)MachineObservationStateId.Break, "MachineObservationStateBreak", true);
        machineObservationState.OnSite = true;
        machineObservationState.SiteAttendanceChange = unattended;
        machineObservationState.IsProduction = false;
        InsertDefaultValue (machineObservationState);
      }
      { // 12: Cleanup
        IMachineObservationState machineObservationState =
          new MachineObservationState ((int)MachineObservationStateId.Cleanup, "MachineObservationStateCleanup", true);
        machineObservationState.OnSite = true;
        machineObservationState.SiteAttendanceChange = unattended;
        machineObservationState.IsProduction = false;
        InsertDefaultValue (machineObservationState);
      }
      { // 13: Week-end (Production)
        IMachineObservationState weekEnd = new MachineObservationState ((int)MachineObservationStateId.Weekend, "MachineObservationStateWeekEnd", true);
        weekEnd.IsProduction = true;
        InsertDefaultValue (weekEnd);
      }
      { // 14: Night (Production)
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
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          machineObservationState.Id, machineObservationState.TranslationKey);
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
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new machine observation state {0} " +
                         "failed with {1}",
                         machineObservationState,
                         ex);
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
        log.ErrorFormat ("ResetSequence: " +
                         "resetting the sequence failed with {0}",
                         ex);
      }
    }
    #endregion // DefaultValues
  }
}
