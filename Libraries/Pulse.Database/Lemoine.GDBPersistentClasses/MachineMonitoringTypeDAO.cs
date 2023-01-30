// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineMonitoringTypeDAO">IMachineMonitoringTypeDAO</see>
  /// </summary>
  public class MachineMonitoringTypeDAO
    : VersionableNHibernateDAO<MachineMonitoringType, IMachineMonitoringType, int>
    , IMachineMonitoringTypeDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineMonitoringTypeDAO).FullName);

    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      IMachineMonitoringType monitoringType;
      // Undefined
      monitoringType = new MachineMonitoringType ((int)MachineMonitoringTypeId.Undefined, "UndefinedValue");
      InsertDefaultValue (monitoringType);
      // Monitored
      monitoringType = new MachineMonitoringType ((int)MachineMonitoringTypeId.Monitored, "MonitoringTypeMonitored");
      InsertDefaultValue (monitoringType);
      // NotMonitored
      monitoringType = new MachineMonitoringType ((int)MachineMonitoringTypeId.NotMonitored, "MonitoringTypeNotMonitored");
      InsertDefaultValue (monitoringType);
      // Outsource
      monitoringType = new MachineMonitoringType ((int)MachineMonitoringTypeId.Outsource, "MonitoringTypeOutsource");
      InsertDefaultValue (monitoringType);
      // Obsolete
      monitoringType = new MachineMonitoringType ((int)MachineMonitoringTypeId.Obsolete, "MonitoringTypeObsolete");
      InsertDefaultValue (monitoringType);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="machineMonitoringType">not null</param>
    private void InsertDefaultValue (IMachineMonitoringType machineMonitoringType)
    {
      Debug.Assert (null != machineMonitoringType);
      
      try {
        if (null == FindById (machineMonitoringType.Id)) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          machineMonitoringType.Id, machineMonitoringType.TranslationKey);
          // Use a raw SQL Command, else the Id is resetted
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format (@"INSERT INTO machinemonitoringtype (machinemonitoringtypeid, machinemonitoringtypetranslationkey)
VALUES ({0}, '{1}')",
                                                 machineMonitoringType.Id, machineMonitoringType.TranslationKey);
            command.ExecuteNonQuery();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new machine monitoring type {0} " +
                         "failed with {1}",
                         machineMonitoringType,
                         ex);
      }
    }
    #endregion // DefaultValues
  }
}
