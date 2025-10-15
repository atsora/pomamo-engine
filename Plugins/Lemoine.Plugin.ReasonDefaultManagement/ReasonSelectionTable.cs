// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Reason;
using Lemoine.Business;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.ReasonDefaultManagement
{
  /// <summary>
  /// Implementation of IReasonSelectionExtension using the reasonselection table / persistent class
  /// </summary>
  public sealed class ReasonSelectionTable
    : Lemoine.Extensions.NotConfigurableExtension
    , IReasonSelectionExtension
    , IReasonLegendExtension
  {
    #region Members
    IMachine m_machine;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (ReasonSelectionTable).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ReasonSelectionTable ()
    {
    }

    #region IReasonSelectionExtension implementation
    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public bool Initialize (IMachine machine)
    {
      Debug.Assert (null != machine);

      m_machine = machine;
      log = LogManager.GetLogger (typeof (ReasonSelectionTable).FullName + "." + machine.Id);

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var reasonSelections = ModelDAOHelper.DAOFactory.ReasonSelectionDAO.FindAll ();
        if (!reasonSelections.Any (x => x.Selectable)) {
          log.Info ("Initialize: there is no selectable reason in table reasonselection => return false");
          return false;
        }
      }

      return true;
    }

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    /// <param name="range"></param>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    /// <param name="includeExtraAutoReasons"></param>
    /// <returns></returns>
    public IEnumerable<IReasonSelection> GetReasonSelections (UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons)
      => GetPossibleReasonSelections (machineMode, machineObservationState, includeExtraAutoReasons);

    /// <summary>
    /// <see cref="Lemoine.Extensions.Database.IReasonSelectionExtension"/>
    /// </summary>
    public IEnumerable<IReasonSelection> GetPossibleReasonSelections (IMachineMode machineMode, IMachineObservationState machineObservationState, bool includeExtraAutoReasons)
    {
      var reasonSelectionFindRequest = new ReasonSelectionFind (m_machine, machineMode, machineObservationState);
      IEnumerable<IReasonSelection> reasonSelections = ServiceProvider
        .Get<IEnumerable<IReasonSelection>> (reasonSelectionFindRequest)
        .Where (r => r.Selectable);
      return reasonSelections;
    }
    #endregion // IReasonSelectionExtension implementation

    #region IReasonLegendExtension implementation
    /// <summary>
    /// <see cref="IReasonLegendExtension"/>
    /// </summary>
    /// <returns></returns>
    public bool Initialize ()
    {
      return true;
    }

    /// <summary>
    /// <see cref="IReasonLegendExtension"/>
    /// </summary>
    /// <returns></returns>
    public IEnumerable<IReason> GetUsedReasons ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var reasons = ModelDAOHelper.DAOFactory.ReasonSelectionDAO
          .FindReasons ();
        // Note: normally not required because FindReasons already fetch the reason groups, but this is safer
        foreach (var reason in reasons) {
          ModelDAOHelper.DAOFactory.Initialize (reason.ReasonGroup);
        }
        return reasons;
      }
    }
    #endregion // IReasonLegendExtension implementation
  }
}
