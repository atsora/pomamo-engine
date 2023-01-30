// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IGoalTypeDAO">IGoalTypeDAO</see>
  /// </summary>
  public class GoalTypeDAO
    : VersionableNHibernateDAO<GoalType, IGoalType, int>
    , IGoalTypeDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GoalTypeDAO).FullName);
    
    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      IGoalType goalType;
      // - Utilization %
      goalType = new GoalType (GoalTypeId.UtilizationPercentage, "GoalUtilizationPercentage");
      goalType.Unit = new UnitDAO ().FindById (4);
      InsertDefaultValue (goalType);
      // - Adjustement % to get an expected quantity from a cycle duration during a production period
      goalType = new GoalType (GoalTypeId.QuantityVsProductionCycleDuration, "GoalQuantityVsProductionCycleDuration");
      goalType.Unit = new UnitDAO ().FindById (4);
      InsertDefaultValue (goalType);
      
      ResetSequence (100);
    }
    
    private void InsertDefaultValue (IGoalType goalType)
    {
      if (null == FindById (goalType.Id)) { // It does not exist, create it
        log.InfoFormat ("InsertDefaultValue: " +
                        "add id={0} config={1}",
                        goalType.Id, goalType.Display);
        // Use a raw SQL Command, else the Id is resetted
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
        {
          command.CommandText = string.Format (@"INSERT INTO goaltype (goaltypeid, goaltypetranslationkey, unitid)
VALUES ({0}, '{1}', {2})",
                                               goalType.Id, goalType.TranslationKey, goalType.Unit.Id);
          command.ExecuteNonQuery();
        }
      }
    }
    
    private void ResetSequence (int minId)
    {
      try {
        using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
        {
          command.CommandText = string.Format (@"
WITH maxid AS (SELECT MAX({1}) AS maxid FROM {0})
SELECT SETVAL('{0}_{1}_seq', CASE WHEN (SELECT maxid FROM maxid) < {2} THEN {2} ELSE (SELECT maxid FROM maxid) + 1 END);",
                                               "goaltype", "goaltypeid",
                                               minId);
          command.ExecuteNonQuery();
        }
      }
      catch (Exception ex) {
        log.Error ("ResetSequence: resetting the sequence failed", ex);
      }
    }
    #endregion // DefaultValues
  }
}
