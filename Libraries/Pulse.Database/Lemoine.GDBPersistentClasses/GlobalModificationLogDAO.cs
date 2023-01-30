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
  /// Implementation of <see cref="Lemoine.ModelDAO.IGlobalModificationLogDAO">IGlobalModificationLogDAO</see>
  /// </summary>
  public class GlobalModificationLogDAO
    : SaveOnlyNHibernateDAO<GlobalModificationLog, IGlobalModificationLog, int>
    , IGlobalModificationLogDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GlobalModificationLogDAO).FullName);

    /// <summary>
    /// Add a new analysis log for a specified modification, level and message
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    public void Add (IGlobalModification modification,
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
        IGlobalModificationLog analysisLog = ModelDAOHelper.ModelFactory
          .CreateGlobalModificationLog (level, message, modification);
        analysisLog.Module = this.GetType ().Name;
        ModelDAOHelper.DAOFactory.GlobalModificationLogDAO
          .MakePersistent (analysisLog);
      }
    }
  }
}
