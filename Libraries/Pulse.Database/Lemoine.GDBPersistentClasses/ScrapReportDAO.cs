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
  /// Implementation of <see cref="Lemoine.ModelDAO.IScrapReportDAO"></see>
  /// </summary>
  public class ScrapReportDAO
    : SaveOnlyByMachineNHibernateDAO<ScrapReport, IScrapReport, long>
    , IScrapReportDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapReportDAO).FullName);

  }
}
