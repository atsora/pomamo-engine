// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of OperationMachineAssociationDAO.
  /// </summary>
  public class OperationMachineAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<OperationMachineAssociation, IOperationMachineAssociation, long>
    , IOperationMachineAssociationDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationMachineAssociationDAO).FullName);
  }
}
