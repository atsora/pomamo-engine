// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Database;
using Lemoine.ModelDAO;
using NHibernate.Cfg;

namespace Lemoine.Plugin.CycleDurationSummary
{
  public class NHibernateExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , INHibernateExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (NHibernateExtension).FullName);

    public bool ContainsMapping ()
    {
      return true;
    }

    public void UpdateConfiguration (ref Configuration configuration)
    {
    }
  }
}
