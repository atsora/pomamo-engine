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
  /// Implementation of <see cref="Lemoine.ModelDAO.INonConformanceReasonDAO">INonConformanceReasonDAO</see>
  /// </summary>
  public class NonConformanceReasonDAO
    : VersionableNHibernateDAO<NonConformanceReason, INonConformanceReason, int>
    , INonConformanceReasonDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (NonConformanceReasonDAO).FullName);
  }
}
