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
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModificationLogDAO">IMachineModificationLogDAO</see>
  /// </summary>
  public class MachineModificationLogDAO
    : SaveOnlyNHibernateDAO<MachineModificationLog, IMachineModificationLog, int>
    , IMachineModificationLogDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModificationLogDAO).FullName);

    /// <summary>
    /// Add a new analysis log for a specified modification, level and message
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    public void Add (IMachineModification modification,
                     LogLevel level,
                     string message)
    {
      if (modification.IsMainModificationTransient ()) {
        log.ErrorFormat ("Add: " +
                         "level={0} message={1}, " +
                         "no main modification or transient modification, give up",
                         level, message);
        return;
      }
      else {
        IMachineModificationLog analysisLog = ModelDAOHelper.ModelFactory
          .CreateMachineModificationLog (level, message, modification);
        analysisLog.Module = this.GetType ().Name;
        ModelDAOHelper.DAOFactory.MachineModificationLogDAO
          .MakePersistent (analysisLog);
      }
    }
  }
}
