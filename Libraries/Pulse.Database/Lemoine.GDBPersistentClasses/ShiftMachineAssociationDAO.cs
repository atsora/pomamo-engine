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
  /// DAO Implementation
  /// </summary>
  public class ShiftMachineAssociationDAO // Note: public because it is used in Lemoine.Analysis and in Lemoine.Plugin.DefaultAccumulators
    : SaveOnlyByMachineNHibernateDAO<ShiftMachineAssociation, IShiftMachineAssociation, long>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ShiftMachineAssociationDAO).FullName);
  }
}
