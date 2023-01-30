// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Web DAO session: fictive DAO session, it does nothing
  /// </summary>
  public sealed class WebDAOSession: Lemoine.ModelDAO.IDAOSession
  {
    readonly ILog log = LogManager.GetLogger(typeof (WebDAOSession).FullName);

    #region IDAOSession implementation
    public IEnumerable<ILockTableToPartition> CreateLockTableToPartition (object partitionId, string[] tableNames)
    {
      throw new NotImplementedException ();
    }

    public IDAOTransaction BeginTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null, bool notTop = false)
    {
      return new WebDAOTransaction (name);
    }

    public IDAOTransaction BeginReadOnlyTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null)
    {
      return new WebDAOTransaction (name);
    }

    public IDAOTransaction BeginReadOnlyDeferrableTransaction (string name = "", TransactionLevel transactionLevel = TransactionLevel.Default, bool transactionLevelOptional = false, IEnumerable<ILockTableToPartition> lockedTables = null)
    {
      return new WebDAOTransaction (name);
    }

    public void CancelActiveTransaction()
    {
      throw new NotImplementedException();
    }
    public void Clear()
    {
      throw new NotImplementedException();
    }
    #endregion
    #region IDisposable implementation
    public void Dispose()
    {
      // Nothing to do
    }

    public void ForceUniqueSession ()
    {
    }
    #endregion
  }
}
