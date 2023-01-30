// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.WebDataAccess
{
  /// <summary>
  /// Web transaction: fictive transaction. It does nothing
  /// 
  /// Auto-commit is considered here
  /// </summary>
  public class WebDAOTransaction: Lemoine.ModelDAO.IDAOTransaction
  {
    readonly ILog log = LogManager.GetLogger(typeof (WebDAOTransaction).FullName);
    
    #region Constructors
    internal WebDAOTransaction ()
    { }
    
    internal WebDAOTransaction (string name)
      : this ()
    { }
    #endregion // Constructors
    
    #region IDAOTransaction implementation
    public void Commit()
    {
      // Nothing to do: auto-commit
    }
    public void CommitNew()
    {
      throw new NotImplementedException();
    }
    public void Rollback()
    {
      throw new NotImplementedException();
    }

    public void FlagSerializationFailure ()
    { 
    }

    public Lemoine.ModelDAO.SynchronousCommit? SynchronousCommitOption {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public Lemoine.ModelDAO.TransactionLevel TransactionLevelOption {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool ReadOnly {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool Deferrable {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    public bool TopTransaction {
      get;
    }
    #endregion
    #region IDisposable implementation
    public void Dispose()
    {
      // Nothing to do
    }

    public void Add (ILockTableToPartition lockedTable)
    {
      throw new NotImplementedException ();
    }
    #endregion
  }
}
