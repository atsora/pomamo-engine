// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Diagnostics;
using Lemoine.WebClient;
using Lemoine.Core.Log;
using System.Globalization;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Description of ReasonMachineAssociationDAO.
  /// </summary>
  public class ReasonMachineAssociationDAO : Lemoine.ModelDAO.IReasonMachineAssociationDAO
  {
    static readonly string MANUAL_REASON_PRIORITY_KEY = "Reason.Manual.Priority";
    static readonly int MANUAL_REASON_PRIORITY_DEFAULT = 1000;

    #region IGenericByMachineDAO implementation
    public Lemoine.Model.IReasonMachineAssociation FindById (long id, IMachine machine)
    {
      throw new NotImplementedException ();
    }
    #endregion
    #region IBaseGenericDAO implementation
    public System.Collections.Generic.IList<Lemoine.Model.IReasonMachineAssociation> FindAll ()
    {
      throw new NotImplementedException ();
    }
    /// <summary>
    /// Insert a new row in database with a new manual reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    /// <returns></returns>
    public long InsertManualReason (IMachine machine, UtcDateTimeRange range, IReason reason, double reasonScore, string details, string jsonData)
    {
      var association = ModelDAO.ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (machine, range);
      association.SetManualReason (reason, reasonScore, details, jsonData);
      association.Option = AssociationOption.TrackSlotChanges;
      association.Priority = Lemoine.Info.ConfigSet
        .LoadAndGet (MANUAL_REASON_PRIORITY_KEY, MANUAL_REASON_PRIORITY_DEFAULT);
      return Insert (association);
    }

    /// <summary>
    /// Insert a new row in database to reset a reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public long InsertResetReason (IMachine machine, UtcDateTimeRange range)
    {
      var association = ModelDAO.ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (machine, range);
      association.ResetManualReason ();
      return Insert (association);
    }

    /// <summary>
    /// Insert a new row in database with an auto-reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    /// <param name="dynamic"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    public long InsertAutoReason (IMachine machine, UtcDateTimeRange range, IReason reason, double reasonScore, string details, string dynamic, bool overwriteRequired, AssociationOption? option)
    {
      var association = ModelDAO.ModelDAOHelper.ModelFactory.CreateReasonMachineAssociation (machine, range);
      association.SetAutoReason (reason, reasonScore, overwriteRequired, details);
      association.Dynamic = dynamic;
      association.Option = option;
      return Insert (association);
    }

    /// <summary>
    /// Insert a row in database that corresponds to a sub-modification
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range"></param>
    /// <param name="preChange"></param>
    /// <param name="parent">optional: alternative parent</param>
    /// <returns></returns>
    public IMachineModification InsertSub (IReasonMachineAssociation association, UtcDateTimeRange range, Action<IReasonMachineAssociation> preChange, IMachineModification parent)
    {
      throw new NotImplementedException ();
    }

    long Insert (IReasonMachineAssociation entity)
    {
      // Only Save is valid here
      Debug.Assert (0 == ((Lemoine.Collections.IDataWithId<long>)entity).Id);
      Debug.Assert (null != entity.Machine);

      throw new NotImplementedException ();
    }

    public void MakeTransient (Lemoine.Model.IReasonMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }
    public void Lock (Lemoine.Model.IReasonMachineAssociation entity)
    {
      throw new NotImplementedException ();
    }

    public IList<IReasonMachineAssociation> FindAppliedManualInRange (IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }

    public IList<IReasonMachineAssociation> FindAppliedAutoInRange (IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }

    public IReasonMachineAssociation FindById (int id, IMachine machine)
    {
      throw new NotImplementedException ();
    }

    public IReasonMachineAssociation GetFirstNewConsolidateMatchingRange (IMachine machine, UtcDateTimeRange range)
    {
      throw new NotImplementedException ();
    }

    public IReasonMachineAssociation GetNextAncestorAuto (IMachine machine, long modificationId)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
