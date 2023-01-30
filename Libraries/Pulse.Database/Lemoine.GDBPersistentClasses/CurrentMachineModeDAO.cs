// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using System.Diagnostics;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICurrentMachineModeDAO">ICurrentMachineModeDAO</see>
  /// </summary>
  public class CurrentMachineModeDAO
    : VersionableNHibernateDAO<CurrentMachineMode, ICurrentMachineMode, int>
    , ICurrentMachineModeDAO
  {
    /// <summary>
    /// Find the ICurrentMachineMode for the specified monitoredMachine
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public ICurrentMachineMode Find(IMachine machine)
    {
      Debug.Assert (null != machine);
      
      using (var disableSeqScan = new DisableSeqScan ("CurrentMachineMode"))
      {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<CurrentMachineMode> ()
            .Add (Restrictions.Eq ("MonitoredMachine.Id", machine.Id))
            .UniqueResult<ICurrentMachineMode> ();
        }
      }
    
    /// <summary>
    /// Find the ICurrentMachineMode for the specified monitoredMachine
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public ICurrentMachineMode FindWithMachineMode (IMachine machine)
    {
      Debug.Assert (null != machine);

      using (var disableSeqScan = new DisableSeqScan ("CurrentMachineMode"))
      {
          return NHibernateHelper.GetCurrentSession ()
            .CreateCriteria<CurrentMachineMode> ()
            .Add (Restrictions.Eq ("MonitoredMachine.Id", machine.Id))
            .Fetch (SelectMode.Fetch, "MachineMode")
            .UniqueResult<ICurrentMachineMode> ();
      }
    }
  }
}
