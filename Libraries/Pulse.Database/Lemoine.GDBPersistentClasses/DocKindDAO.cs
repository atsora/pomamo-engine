// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="IDocKindDAO"/>
  /// </summary>
  public class DocKindDAO
    : VersionableNHibernateDAO<DocKind, IDocKind, int>
    , IDocKindDAO
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DocKindDAO).FullName);
  }
}
