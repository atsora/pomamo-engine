// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IOperationSourceWorkPieceDAO">IOperationSourceWorkPieceDAO</see>
  /// </summary>
  public class OperationSourceWorkPieceDAO
    : VersionableNHibernateDAO<OperationSourceWorkPiece, IOperationSourceWorkPiece, int>, IOperationSourceWorkPieceDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSourceWorkPieceDAO).FullName);
    
    /// <summary>
    /// Try to get the OperationSourceWorkPiece entities related to an operation
    /// </summary>
    /// <param name="operation"></param>
    /// <returns></returns>
    public IList<IOperationSourceWorkPiece> GetOperationSourceWorkPiece(IOperation operation)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<OperationSourceWorkPiece> ()
        .Add (Restrictions.Eq ("Operation", operation))
        .List<IOperationSourceWorkPiece>();
    }
  }
}
