// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
  /// Implementation of the DAO of ManufacturingOrderMachineAssociation
  /// </summary>
  public class ManufacturingOrderMachineAssociationDAO
    : SaveOnlyByMachineNHibernateDAO<ManufacturingOrderMachineAssociation, IManufacturingOrderMachineAssociation, long>
    , IManufacturingOrderMachineAssociationDAO
  {
  }
}
