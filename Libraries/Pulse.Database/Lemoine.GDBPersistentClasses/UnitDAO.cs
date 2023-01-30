// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IUnitDAO">IUnitDAO</see>
  /// </summary>
  public class UnitDAO
    : VersionableNHibernateDAO<Unit, IUnit, int>
    , IUnitDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UnitDAO).FullName);
    
    /// <summary>
    /// Find a unit with the enum
    /// </summary>
    /// <param name="unitId"></param>
    /// <returns></returns>
    public IUnit FindByUnitId(UnitId unitId)
    {
      return FindById((int)unitId);
    }
    
    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      InsertDefaultValue (new Unit(UnitId.FeedRate, "Feedrate unit (mm/min)", "UnitFeedRate")); // 1
      InsertDefaultValue (new Unit(UnitId.FeedRateUS, "Feedrate US unit (IPM)", "UnitFeedRateUS"));
      InsertDefaultValue (new Unit(UnitId.RotationSpeed, "Rotation speed unit (RPM)", "UnitRotationSpeed"));
      InsertDefaultValue (new Unit(UnitId.Percent, "Percent (%)", "UnitPercent"));
      InsertDefaultValue (new Unit(UnitId.NumberOfParts, "Number of parts", "UnitNumberOfParts")); // 5
      InsertDefaultValue (new Unit(UnitId.None, "No unit", "UnitNone"));
      InsertDefaultValue (new Unit(UnitId.DistanceMillimeter, "Distance (mm)", "UnitDistanceMillimeter"));
      InsertDefaultValue (new Unit(UnitId.DistanceInch, "Distance (inch)", "UnitDistanceInch"));
      InsertDefaultValue (new Unit(UnitId.DistanceMeter, "Distance (m)", "UnitDistanceMeter"));
      InsertDefaultValue (new Unit(UnitId.DistanceFeet, "Distance (feet)", "UnitDistanceFeet")); // 10
      InsertDefaultValue (new Unit(UnitId.DurationSeconds, "Duration (s)", "UnitDurationSeconds"));
      InsertDefaultValue (new Unit(UnitId.DurationMinutes, "Duration (min)", "UnitDurationMinutes"));
      InsertDefaultValue (new Unit(UnitId.DurationHours, "Duration (h)", "UnitDurationHours"));
      InsertDefaultValue (new Unit(UnitId.Unknown, "Unknown", "UnitUnknown"));
      InsertDefaultValue (new Unit(UnitId.ToolNumberOfTimes, "Number of times", "UnitToolNumberOfTimes")); // 15
      InsertDefaultValue (new Unit(UnitId.ToolWear, "Tool wear", "UnitToolWear"));
      InsertDefaultValue (new Unit(UnitId.NumberOfCycles, "Number of cycles", "UnitNumberOfCycles"));
      InsertDefaultValue (new Unit (UnitId.FlowRate, "Flow rate (L/s)", "UnitFlowRate"));
      ResetSequence (100);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit">not null</param>
    void InsertDefaultValue (IUnit unit)
    {
      try {
        if (null == FindById (unit.Id)) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: add id={0} translationKey={1}",
                          unit.Id, unit.TranslationKey);
          // Use a raw SQL Command, else the Id is resetted
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format ("INSERT INTO unit (unitid, unitdescription, unittranslationkey) " +
                                                 "VALUES ({0}, '{1}', '{2}')", unit.Id, unit.Description, unit.TranslationKey);
            command.ExecuteNonQuery();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: inserting new role {0} failed with {1}", unit, ex);
      }
    }
    
    void ResetSequence (int minId)
    {
      try {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
        {
          command.CommandText = string.Format ("WITH maxid AS (SELECT MAX({1}) AS maxid FROM {0})" +
                                               "SELECT SETVAL('{0}_{1}_seq', " +
                                               "CASE WHEN (SELECT maxid FROM maxid) < {2} THEN {2} ELSE " +
                                               "(SELECT maxid FROM maxid) + 1 END);",
                                               "unit", "unitid", minId);
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("ResetSequence: resetting the sequence failed with {0}", ex);
      }
    }
    #endregion // DefaultValues
  }
}
