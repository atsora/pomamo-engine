// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineObservationStateAssociationDAO">IMachineObservationStateAssociationDAO</see>
  /// </summary>
  public class MachineObservationStateAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<MachineObservationStateAssociation, IMachineObservationStateAssociation, long>
    , IMachineObservationStateAssociationDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationStateAssociationDAO).FullName);
  }
}


