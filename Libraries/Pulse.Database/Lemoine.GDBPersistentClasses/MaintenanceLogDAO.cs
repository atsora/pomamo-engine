// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMaintenanceLogDAO">IMaintenanceLogDAO</see>
  /// </summary>
  public class MaintenanceLogDAO
    : SaveOnlyNHibernateDAO<MaintenanceLog, IMaintenanceLog, int>
    , IMaintenanceLogDAO
  {
  }
}
