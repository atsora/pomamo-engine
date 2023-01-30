// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using NHibernate;

namespace Pulse.Database
{
  /// <summary>
  /// DefaultValuesFactory
  /// </summary>
  public sealed class DefaultValuesFactory: IDefaultValuesFactory
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultValuesFactory).FullName);

    public IDefaultValues CreateNocache (ISessionFactory sessionFactory)
    {
      return Lemoine.GDBPersistentClasses.DefaultValues.CreateNoCache (sessionFactory);
    }
  }
}
