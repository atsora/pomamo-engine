// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IAnalysisLogDAO">IAnalysisLogDAO</see>
  /// </summary>
  public class AnalysisLogDAO
    : IAnalysisLogDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (AnalysisLogDAO).FullName);

    /// <summary>
    /// Add a new analysis log for a specified modification, level and message
    /// </summary>
    /// <param name="modification"></param>
    /// <param name="level"></param>
    /// <param name="message"></param>
    public void Add (IModification modification,
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
        IModification mainModification = ((Modification)modification).MainModification;
        if (mainModification is IGlobalModification) {
          ModelDAOHelper.DAOFactory.GlobalModificationLogDAO
            .Add ((IGlobalModification)mainModification, level, message);
        }
        else if (mainModification is IMachineModification) {
          ModelDAOHelper.DAOFactory.MachineModificationLogDAO
            .Add ((IMachineModification)mainModification, level, message);
        }
        else {
          Debug.Assert (false);
          log.FatalFormat ("Add: " +
                           "not supported/implemented sub-type for {0}",
                           mainModification);
        }
      }
    }
  }
}
