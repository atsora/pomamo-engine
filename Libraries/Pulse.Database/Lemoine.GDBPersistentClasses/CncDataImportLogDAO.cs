// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.ICncDataImportLogDAO">ICncDataImportLogDAO</see>
  /// </summary>
  public class CncDataImportLogDAO
    : SaveOnlyNHibernateDAO<CncDataImportLog, ICncDataImportLog, int>
    , ICncDataImportLogDAO
  {
  }
}
